# 窗口居中工具 / Window Center Tool

一个为 Windows 设计的轻量级托盘常驻工具，用全局快捷键快速把当前窗口居中或贴靠到左右两侧。  
A lightweight Windows tray utility that uses global hotkeys to center the current window or dock it to the left or right edge.

## 当前功能 / Current Features

- 托盘常驻运行 / Runs from the system tray
- 支持开机自启 / Optional launch on login
- 支持全局快捷键 / Supports global hotkeys
- 支持基于当前宽度的连续窗口编排 / Keeps the current width while arranging windows

## 默认快捷键 / Default Hotkeys

- 居中拉满高度 / Center full-height
  `Alt+1` or `Alt+-`
- 左侧停靠 / Dock left
  `Alt+\`` or `Alt+0`
- 右侧停靠 / Dock right
  `Alt+2` or `Alt+=`

行为说明 / Behavior:

- 第一次居中会把窗口调整为上下拉满并水平居中，默认宽度为工作区的 50%
- 如果你之后手动调整了窗口宽度，再次居中时会保留当前宽度，只重新居中并拉满高度
- 左右停靠也会保留当前窗口宽度，只把窗口贴到左边或右边，并拉满高度

## 本地运行 / How to Run

在项目根目录打开 PowerShell：  
Open PowerShell in the project root:

```powershell
dotnet run --project .\WindowResizerApp\WindowResizerApp.csproj
```

或者先构建再运行：  
Or build first and then run:

```powershell
dotnet build .\WindowResizerApp\WindowResizerApp.csproj -c Debug
.\WindowResizerApp\bin\Debug\net7.0-windows\win-x64\WindowResizerApp.exe
```

## 当前状态 / Current Status

- 今天先发布源码版本 / Source-only version for now
- 安装包、图标、设置页美化后续补充 / Installer, icons, and UI polish will come later

## 路线图 / Roadmap

- 修正和打磨窗口排列手感 / Refine window arrangement behavior
- 增加更完整的设置页面 / Improve the settings UI
- 添加应用图标和品牌元素 / Add icons and branding
- 生成安装包与 GitHub Releases / Publish an installer and GitHub releases
