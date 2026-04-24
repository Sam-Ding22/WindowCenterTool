# WindowResizer

一个面向 Windows 的轻量托盘工具，用全局快捷键快速把当前窗口居中、贴左或贴右。

## Demo

![WindowResizer demo](./demo.gif)

## Current Features

- 托盘常驻运行
- 支持开机自启动
- 支持全局快捷键
- 支持从设置窗口调整快捷键
- 支持基于当前窗口宽度进行连续排布

## Default Hotkeys

- `Alt+-`: 上下拉满并水平居中
- `Alt+0`: 贴左并拉满高度
- `Alt+=`: 贴右并拉满高度

当前版本支持在设置页里修改这三个快捷键。

## Behavior

- 当窗口还没有上下拉满时，按 `Alt+-` 会把窗口调整为上下拉满并水平居中
- 如果你之后手动调整了窗口宽度，再按一次居中快捷键时，会保留当前宽度，只重新居中并拉满高度
- 左右停靠同样会保留当前窗口宽度，只把窗口贴到左边或右边，并拉满当前显示器工作区高度

## How to Run

在项目根目录打开 PowerShell：

```powershell
dotnet run --project .\WindowResizerApp\WindowResizerApp.csproj
```

或者先构建再运行：

```powershell
dotnet build .\WindowResizerApp\WindowResizerApp.csproj -c Debug
.\WindowResizerApp\bin\Debug\net7.0-windows\win-x64\WindowResizerApp.exe
```

启动后应用会进入系统托盘，不会弹出主窗口。

## Settings

右键托盘图标可以：

- 打开设置窗口
- 开关开机自启动
- 重载热键
- 打开设置目录
- 退出程序

配置文件默认位于：

```text
%AppData%\WindowResizer\settings.json
```

日志文件默认位于：

```text
%LocalAppData%\WindowResizer\logs\app.log
```

## Current Status

当前先以源码版本为主，安装包、应用图标、设置页视觉优化会在后续迭代中补上。

## Roadmap

- 修正并继续打磨窗口排布逻辑
- 优化设置页面和托盘菜单体验
- 添加应用图标和品牌元素
- 生成安装包并接入 GitHub Releases
