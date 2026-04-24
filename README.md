# WindowResizer / 窗口居中工具

一个面向 Windows 的轻量托盘工具，用全局快捷键快速把当前窗口居中、贴左或贴右。
A lightweight Windows tray utility that uses global hotkeys to center the current window or dock it to the left or right edge.

## Demo / 演示

![WindowResizer demo](./demo.gif)

## Current Features / 当前功能

- 托盘常驻运行 / Runs from the system tray
- 支持开机自启动 / Optional launch on login
- 支持全局快捷键 / Supports global hotkeys
- 支持从设置窗口调整快捷键 / Customize hotkeys from the settings window
- 支持基于当前窗口宽度进行连续排布 / Keeps the current width while arranging windows

## Default Hotkeys / 默认快捷键

| 功能 / Action | 快捷键 / Hotkey |
|---|---|
| 上下拉满并水平居中 / Center full-height | `Alt+-` |
| 贴左并拉满高度 / Dock left | `Alt+0` |
| 贴右并拉满高度 / Dock right | `Alt+=` |

当前版本支持在设置页里修改这三个快捷键。
These three hotkeys can be customized in the settings window.

## Behavior / 行为说明

- 当窗口还没有上下拉满时，按 `Alt+-` 会把窗口调整为上下拉满并水平居中
  When a window is not yet full-height, `Alt+-` centers it horizontally and stretches it to full height.
- 如果你之后手动调整了窗口宽度，再按一次居中快捷键时，会保留当前宽度，只重新居中并拉满高度
  If you manually resize the window width afterward, pressing the center hotkey again keeps the current width and only re-centers and stretches the height.
- 左右停靠同样会保留当前窗口宽度，只把窗口贴到左边或右边，并拉满当前显示器工作区高度
  Left/right docking also preserves the current window width, only snapping to the left or right edge and stretching to the full work area height.

## How to Run / 本地运行

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

启动后应用会进入系统托盘，不会弹出主窗口。
After launching, the app resides in the system tray and does not open a main window.

## Settings / 设置

右键托盘图标可以：
Right-click the tray icon to:

- 打开设置窗口 / Open settings window
- 开关开机自启动 / Toggle launch on login
- 重载热键 / Reload hotkeys
- 打开设置目录 / Open settings folder
- 退出程序 / Exit

配置文件默认位于：
Settings file location:

```text
%AppData%\WindowResizer\settings.json
```

日志文件默认位于：
Log file location:

```text
%LocalAppData%\WindowResizer\logs\app.log
```

## Current Status / 当前状态

当前先以源码版本为主，安装包、应用图标、设置页视觉优化会在后续迭代中补上。
Source-only release for now; installer, app icon, and settings UI polish will come in later iterations.

## Roadmap / 路线图

- 修正并继续打磨窗口排布逻辑 / Refine window arrangement behavior
- 优化设置页面和托盘菜单体验 / Improve settings UI and tray menu experience
- 添加应用图标和品牌元素 / Add app icon and branding
- 生成安装包并接入 GitHub Releases / Publish installer and set up GitHub Releases
