using OtterGui.Filesystem;
using OtterGui.Raii;
using Dalamud.Interface.Colors;
using ECommons.DalamudServices;
using Dalamud.Bindings.ImGui;
using System;
using System.Collections.Generic;
using ECommons;
using Newtonsoft.Json.Linq;
using System.Linq;
using Newtonsoft.Json;
using System.Numerics;
using ECommons.ImGuiMethods;
using ECommons.Logging;
using NightmareUI.PrimaryUI;

#pragma warning disable CS8618
#pragma warning disable

namespace NightmareUI.OtterGuiWrapper.FileSystems.Configuration;
public sealed class ConfigFileSystem: FileSystem<ConfigFileSystemEntry>
{
		public FileSystemSelector Selector { get; private set; }
		public ICollection<ConfigFileSystemEntry> DataStorage { get; private set; }
		// DataStorage 的同一份陣列：Draw 每幀走訪用它，經介面走訪每次都要配一個列舉器。
		private ConfigFileSystemEntry[] DataArray;
		private Func<ICollection<ConfigFileSystemEntry>> GetDataAction;
		public Func<ConfigFileSystemEntry> DefaultValueSelector;

    public ConfigFileSystem(Func<ICollection<ConfigFileSystemEntry>> getDataAction)
		{
				this.DefaultValueSelector = () => DataStorage.First();
        this.GetDataAction = getDataAction;
				try
				{
						Reload();
				}
				catch (Exception e)
				{
						e.Log();
				}
				var types = ECommonsMain.Instance.GetType().Assembly.GetTypes().Where(x => !x.IsInterface && !x.IsAbstract && x.IsAssignableTo(typeof(ConfigFileSystemEntry)));
				var dataTypes = DataStorage.Select(x => x.GetType());
				foreach(var type in types)
				{
						if(!dataTypes.Contains(type))
						{
								//PluginLog.Warning($"{type} was not found in ConfigFileSystem's Data, did you forgot to create an instance of it?");
						}
				}
		}

		private string SelectedPath;

		public void Reload()
		{
				if(Selector != null)
				{
						SelectedPath = Selector.Selected?.Path;
				}
				DataArray = GetDataAction().Where(x => x.ShouldDisplay()).ToArray();
				DataStorage = DataArray;
				var identifiers = new Dictionary<string, Dictionary<string, string>>()
				{
						["Data"] = DataStorage.ToDictionary(x => x.Path, x => x.Path)
				};
				//PluginLog.Information($"{JsonConvert.SerializeObject(identifiers)}");
				this.Load(JObject.Parse(JsonConvert.SerializeObject(identifiers)), DataStorage, (x) => x.Path, (x) => x.Path);
				Selector = new(this);
				//PluginLog.Information($"Selected: {SelectedPath}");
				if (SelectedPath != null)
				{
						var value = DataStorage.FirstOrDefault(x => x.Path == SelectedPath);
						if (value != null) Selector.SelectByValue(value);
				}
				foreach(var x in DataStorage)
				{
						x.Filter = () => Selector.Filter;
						try
						{
								if (x.Path.IsNullOrEmpty()) PluginLog.Error($"Item {x.GetType().FullName} has it's path null or empty");
								PluginLog.Verbose($"Item {x.GetType().FullName} pat {x.Path}");
						}
						catch(Exception e)
						{
								PluginLog.Error($"Item {x.GetType().FullName} does not implements path");
						}
				}
		}

		// 沒有 Builder 的分頁外框只由（選中的分頁、它的路徑、當下的 FrameBg 顏色）決定，三者都沒變就重用上一幀那份。
		// FrameBg 也要比：Section 第一次畫時把它記成自己的底色，之後不再讀，換樣式時要重建才會跟上。
		private ConfigFileSystemEntry FrameEntry;
		private string FramePath;
		private Vector4 FrameBg;
		private NuiBuilder FrameBuilder;

		public void Draw(float? width = null)
		{
				foreach(var x in DataArray)
				{
						if(x.Builder != null)
						{
								x.Builder.Filter = Selector.Filter;
						}
				}
				if(width == null)
				{
						width = 0f;
						foreach(var x in DataStorage)
						{
								var splitPath = x.Path.SplitDirectories();
								for (int i = 0; i < splitPath.Length; i++)
								{
										var newWidth = ImGui.CalcTextSize(splitPath[i]).X + (i + 1) * 25f + ImGui.GetStyle().ScrollbarSize;
										if (newWidth > width.Value) width = newWidth;
								}
						}
				}
				Selector.Draw(width.Value);
				ImGui.SameLine();
				if (ImGui.BeginChild("CFSChild"))
				{
						try
						{
								if (Selector.Selected == null && DefaultValueSelector != null)
								{
										Selector.SelectByValue(DefaultValueSelector());
								}
								if(Selector.Selected != null)
								{
										if (Selector.Selected.NoFrame || Selector.Selected.Builder != null)
										{
												Selector.Selected.Draw();
										}
										else
										{
												var path = Selector.Selected.Path;
												var frameBg = ImGui.GetStyle().Colors[(int)ImGuiCol.FrameBg];
												if(FrameBuilder == null || !ReferenceEquals(FrameEntry, Selector.Selected) || FramePath != path || FrameBg != frameBg)
												{
														FrameBuilder = new NuiBuilder()
																.Section(path.SplitDirectories().Join(" - "))
																.Widget(Selector.Selected.Draw);
														FrameEntry = Selector.Selected;
														FramePath = path;
														FrameBg = frameBg;
												}
												FrameBuilder.Draw();
										}
								} 
						}
						catch (Exception e)
						{
								e.Log();
						}
				}
				ImGui.EndChild();
		}

		public class FileSystemSelector : NightmareUI.OtterGuiWrapper.FileSystems.Configuration.SimplifiedSelector.FileSystemSelector<ConfigFileSystemEntry, FileSystemSelector.State>
		{
				public string Filter => this.FilterValue;
				public bool Sorted = false;
				public override ISortMode<ConfigFileSystemEntry> SortMode => Sorted?ISortMode<ConfigFileSystemEntry>.FoldersFirst:ISortMode<ConfigFileSystemEntry>.InternalOrder;

				ConfigFileSystem FS;
				public FileSystemSelector(ConfigFileSystem fs) : base(fs, Svc.KeyState, new(), (e) => e.Log())
				{
						this.FS = fs;
				}

				protected override bool FoldersDefaultOpen => true;
				protected override uint CollapsedFolderColor => ImGuiColors.DalamudViolet.ToUint();
				protected override uint ExpandedFolderColor => CollapsedFolderColor;

				protected sealed override void DrawLeafName(Leaf leaf, in State state, bool selected)
				{
						var flag = selected ? ImGuiTreeNodeFlags.Selected | LeafFlags : LeafFlags;
						flag |= ImGuiTreeNodeFlags.SpanFullWidth;
						var col = leaf.Value.GetColor();
						if (col != null) ImGui.PushStyleColor(ImGuiCol.Text, col.Value);
						// flag 一律含 LeafFlags 的 NoTreePushOnOpen：ImRaii.TreeNode 的結束動作本來就是空的，直接呼叫省掉每列每幀一個結束物件。
						ImGui.TreeNodeEx(leaf.Name, flag);
						if (col != null) ImGui.PopStyleColor();
				}

				public record struct State { }
				protected override bool ApplyFilters(IPath path)
				{
						return false;
				}
				protected override bool ApplyFiltersAndState(IPath path, out ConfigFileSystem.FileSystemSelector.State state)
				{
						return false;
				}
		}
}
