using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using WindowResizerPlugin;

namespace WindowResizerApp;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _notifyIcon;
    private readonly AppSettingsService _settingsService;
    private readonly WindowResizer _windowResizer;
    private readonly HotkeyManager _hotkeyManager;

    private AppSettings _settings;
    private readonly List<RegisteredHotkey> _registeredHotkeys = new();

    public TrayApplicationContext()
    {
        _settingsService = new AppSettingsService();
        _settings = _settingsService.Load();
        _windowResizer = new WindowResizer();
        _hotkeyManager = new HotkeyManager();

        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "窗口居中工具 / Window Centering Tool",
            Visible = true,
            ContextMenuStrip = BuildMenu()
        };

        _notifyIcon.DoubleClick += (_, _) => ShowStatus("窗口居中工具正在托盘运行。\nWindow Centering Tool is running in the tray.");

        ApplyStartupPreference();
        RegisterHotkeys();
        ShowStatus("Window Resizer is running in the tray.");
    }

    protected override void ExitThreadCore()
    {
        _hotkeyManager.Dispose();
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        base.ExitThreadCore();
    }

    private ContextMenuStrip BuildMenu()
    {
        var menu = new ContextMenuStrip();

        var openSettingsItem = new ToolStripMenuItem("设置 / Settings");
        openSettingsItem.Click += (_, _) => OpenSettings();

        var reloadItem = new ToolStripMenuItem("重新加载快捷键 / Reload hotkeys");
        reloadItem.Click += (_, _) => ReloadHotkeys();

        var openFolderItem = new ToolStripMenuItem("打开配置目录 / Open settings folder");
        openFolderItem.Click += (_, _) =>
        {
            System.Diagnostics.Process.Start("explorer.exe", _settingsService.SettingsDirectory);
        };

        var exitItem = new ToolStripMenuItem("退出 / Exit");
        exitItem.Click += (_, _) => ExitThread();

        menu.Items.Add(openSettingsItem);
        menu.Items.Add(reloadItem);
        menu.Items.Add(openFolderItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(exitItem);
        return menu;
    }

    private void ReloadHotkeys()
    {
        _settings = _settingsService.Load();
        RegisterHotkeys();
        ApplyStartupPreference();
        ShowStatus("设置已重新加载。\nSettings reloaded.");
    }

    private void RegisterHotkeys()
    {
        _hotkeyManager.UnregisterHotkey();
        _hotkeyManager.HotkeyPressed -= OnHotkeyPressed;
        _hotkeyManager.HotkeyPressed += OnHotkeyPressed;
        _registeredHotkeys.Clear();

        var hotkeys = new List<(uint Modifiers, uint VirtualKey)>();
        var seenCombinations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var action in HotkeyActions.Ordered)
        {
            foreach (var binding in GetBindingsForAction(action))
            {
                if (!HotkeyOptions.TryParseModifier(binding.Modifier, out var modifier))
                {
                    continue;
                }

                var combinationKey = $"{modifier}:{binding.VirtualKey}";
                if (!seenCombinations.Add(combinationKey))
                {
                    continue;
                }

                hotkeys.Add((modifier, binding.VirtualKey));
                _registeredHotkeys.Add(new RegisteredHotkey(action, modifier, binding.VirtualKey));
            }
        }

        var success = _hotkeyManager.RegisterHotkeys(hotkeys.ToArray());

        if (!success)
        {
            FileLogger.LogInfo("Hotkey registration failed.");
            ShowStatus("部分快捷键注册失败。\nSome hotkeys could not be registered.");
        }
    }

    private void ApplyStartupPreference()
    {
        var appPath = Application.ExecutablePath;
        const string appName = "WindowResizerApp";

        var success = _settings.LaunchOnLogin
            ? StartupManager.SetStartup(appPath, appName)
            : StartupManager.RemoveStartup(appName);

        if (!success)
        {
            FileLogger.LogInfo("Failed to update launch on login state.");
        }
    }

    private void OnHotkeyPressed(object? sender, HotKeyEventArgs e)
    {
        try
        {
            var foregroundWindow = _windowResizer.GetForegroundWindow();
            if (foregroundWindow == IntPtr.Zero)
            {
                return;
            }

            var action = ResolveAction(e);
            switch (action)
            {
                case HotkeyActions.CenterToggle:
                    _windowResizer.CenterFullHeight(foregroundWindow);
                    break;
                case HotkeyActions.LeftDock:
                    _windowResizer.ResizeToLeftQuarter(foregroundWindow);
                    break;
                case HotkeyActions.RightDock:
                    _windowResizer.ResizeToRightQuarter(foregroundWindow);
                    break;
            }
        }
        catch (Exception ex)
        {
            FileLogger.LogError(ex, "Hotkey action failed.");
            ShowStatus("窗口调整失败，请检查日志。\nWindow resize failed. Check the log for details.");
        }
    }

    private void ShowStatus(string message)
    {
        if (!_settings.ShowNotifications)
        {
            return;
        }

        _notifyIcon.BalloonTipTitle = "窗口居中工具 / Window Centering Tool";
        _notifyIcon.BalloonTipText = message;
        _notifyIcon.ShowBalloonTip(2000);
    }

    private void OpenSettings()
    {
        using var settingsForm = new SettingsForm(_settings);
        if (settingsForm.ShowDialog() == DialogResult.OK && settingsForm.Result is not null)
        {
            _settings = settingsForm.Result;
            _settingsService.Save(_settings);
            ReloadHotkeys();
        }
    }

    private string? ResolveAction(HotKeyEventArgs hotkeyEventArgs)
    {
        foreach (var entry in _registeredHotkeys)
        {
            if (entry.Modifier == hotkeyEventArgs.Modifier &&
                entry.VirtualKey == hotkeyEventArgs.VirtualKey)
            {
                return entry.Action;
            }
        }

        return null;
    }

    private IEnumerable<HotkeyBinding> GetBindingsForAction(string action)
    {
        foreach (var binding in GetBuiltInBindings(action))
        {
            yield return binding;
        }

        if (_settings.Hotkeys.TryGetValue(action, out var customBinding))
        {
            yield return customBinding;
        }
    }

    private static IEnumerable<HotkeyBinding> GetBuiltInBindings(string action)
    {
        switch (action)
        {
            case HotkeyActions.CenterToggle:
                yield return new HotkeyBinding("Alt", 0x31);
                yield return new HotkeyBinding("Alt", 0xBD);
                break;
            case HotkeyActions.LeftDock:
                yield return new HotkeyBinding("Alt", 0xC0);
                yield return new HotkeyBinding("Alt", 0x30);
                break;
            case HotkeyActions.RightDock:
                yield return new HotkeyBinding("Alt", 0x32);
                yield return new HotkeyBinding("Alt", 0xBB);
                break;
        }
    }

    private sealed record RegisteredHotkey(string Action, uint Modifier, uint VirtualKey);
}
