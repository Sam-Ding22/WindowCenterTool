using System.Collections.Generic;

namespace WindowResizerApp;

internal sealed class AppSettings
{
    public bool LaunchOnLogin { get; set; } = true;

    public bool ShowNotifications { get; set; } = true;

    public Dictionary<string, HotkeyBinding> Hotkeys { get; set; } = new()
    {
        ["center_toggle"] = new("Alt", 0x31),
        ["left_dock"] = new("Alt", 0xC0),
        ["right_dock"] = new("Alt", 0x32)
    };

    public void Normalize()
    {
        Hotkeys ??= new Dictionary<string, HotkeyBinding>();

        if (Hotkeys.TryGetValue("left_quarter", out var oldLeft) && !Hotkeys.ContainsKey(HotkeyActions.LeftDock))
        {
            Hotkeys[HotkeyActions.LeftDock] = oldLeft;
        }

        if (Hotkeys.TryGetValue("right_quarter", out var oldRight) && !Hotkeys.ContainsKey(HotkeyActions.RightDock))
        {
            Hotkeys[HotkeyActions.RightDock] = oldRight;
        }

        if (!Hotkeys.ContainsKey(HotkeyActions.CenterToggle))
        {
            Hotkeys[HotkeyActions.CenterToggle] = new HotkeyBinding("Alt", 0x31);
        }

        if (!Hotkeys.ContainsKey(HotkeyActions.LeftDock))
        {
            Hotkeys[HotkeyActions.LeftDock] = new HotkeyBinding("Alt", 0xC0);
        }

        if (!Hotkeys.ContainsKey(HotkeyActions.RightDock))
        {
            Hotkeys[HotkeyActions.RightDock] = new HotkeyBinding("Alt", 0x32);
        }
    }
}

internal sealed record HotkeyBinding(string Modifier, uint VirtualKey);

internal static class HotkeyActions
{
    public const string CenterToggle = "center_toggle";
    public const string LeftDock = "left_dock";
    public const string RightDock = "right_dock";

    public static readonly IReadOnlyList<string> Ordered = new[]
    {
        CenterToggle,
        LeftDock,
        RightDock
    };

    public static string GetDisplayName(string action)
    {
        return action switch
        {
            CenterToggle => "居中拉满高度 / Center full-height",
            LeftDock => "左侧停靠 / Dock left",
            RightDock => "右侧停靠 / Dock right",
            _ => action
        };
    }

    public static string GetDefaultShortcutText(string action)
    {
        return action switch
        {
            CenterToggle => "Alt+1 / Alt+-",
            LeftDock => "Alt+` / Alt+0",
            RightDock => "Alt+2 / Alt+=",
            _ => string.Empty
        };
    }
}
