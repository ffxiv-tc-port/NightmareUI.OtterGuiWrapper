<!-- ffxiv-tc-port 繁體中文說明開始 -->
# NightmareUI.OtterGuiWrapper(台服 fork)

[NightmareXIV/NightmareUI.OtterGuiWrapper](https://github.com/NightmareXIV/NightmareUI.OtterGuiWrapper) 的台服 fork。把 OtterGui 的樹狀清單選擇器接到 NightmareUI 上：`ConfigFileSystem` 是「左側分頁清單＋右側內容」的設定視窗骨架，`GenericFileSystem` 是可以分資料夾的資料清單。

## 台服 fork 的目的

讓台服版插件引用同一份對著台服 Dalamud 建置的原始碼，並修正設定視窗每畫一幀都會發生的兩個問題。

## 與上游的差異

- 設定清單左側的樣式改在子視窗結束前就還原，不再每畫一幀就觸發一次 ImGui 樣式堆疊不成對的斷言。
- 設定清單左側與沒有內容建構器的分頁外框，不再每幀建立暫存物件。

畫面與操作都和上游相同。

## 誰在用它

- [AutoRetainer](https://github.com/ffxiv-tc-port/AutoRetainer)：設定視窗（`ConfigFileSystem`），以子模組引用。
- [Lifestream](https://github.com/ffxiv-tc-port/Lifestream)：地址簿與自訂別名的清單（`GenericFileSystem`），以子模組引用。

授權：本 repo 沒有 LICENSE 檔。上游在本 fork 的基底版本之後才加上 MIT 授權（見[上游 repo 的 LICENSE](https://github.com/NightmareXIV/NightmareUI.OtterGuiWrapper/blob/main/LICENSE)）。

---

以下為上游原始 README，內容未經修改：

<!-- ffxiv-tc-port 繁體中文說明結束 -->

# This library is currently for internal use only!
