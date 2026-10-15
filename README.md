<!-- ffxiv-tc-port 繁體中文說明開始 -->
# NightmareUI.OtterGuiWrapper(台服 fork)

把 OtterGui 的資料夾式清單（檔案系統選擇器）包成 NightmareXIV 系插件可以直接拿來管理設定項目的元件（上游 [NightmareXIV/NightmareUI.OtterGuiWrapper](https://github.com/NightmareXIV/NightmareUI.OtterGuiWrapper)）。

## 台服 fork 的目的

- 讓插件的子模組網址指向本 org，建置不依賴上游 repo 是否還在、有沒有被改寫。

## 與上游的差異

- 沒有我方的 commit，分支內容與上游相同。
- 消費端釘的是較舊的上游版本（以 .NET 9 為目標的那一代）；分支頂端已是上游改用 .NET 10 之後的版本，本 org 的插件沒有採用。

## 誰在用它

- [AutoRetainer](https://github.com/ffxiv-tc-port/AutoRetainer)：子模組 `NightmareUI.OtterGuiWrapper`（設定介面）。
- [Lifestream](https://github.com/ffxiv-tc-port/Lifestream)：子模組 `NightmareUI.OtterGuiWrapper`（地址簿、自訂別名的資料夾清單）。

---

以下為上游原始 README，內容未經修改：

<!-- ffxiv-tc-port 繁體中文說明結束 -->
# This library is currently for internal use only!
