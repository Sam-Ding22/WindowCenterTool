using System.Collections.Generic;
using System.Linq;
using WindowResizerPlugin;

namespace WindowResizerApp;

internal sealed record KeyOption(string Label, uint VirtualKey)
{
    public override string ToString() => Label;
}

internal static class HotkeyOptions
{
    public static readonly IReadOnlyList<string> Modifiers = new[]
    {
        "Alt",
        "Ctrl",
        "Shift",
        "Win"
    };

    public static readonly IReadOnlyList<KeyOption> Keys = BuildKeys();

    public static bool TryParseModifier(string modifier, out uint modifierValue)
    {
        switch (modifier)
        {
            case "Alt":
                modifierValue = WindowsApiWrapper.MOD_ALT;
                return true;
            case "Ctrl":
                modifierValue = WindowsApiWrapper.MOD_CONTROL;
                return true;
            case "Shift":
                modifierValue = WindowsApiWrapper.MOD_SHIFT;
                return true;
            case "Win":
                modifierValue = WindowsApiWrapper.MOD_WIN;
                return true;
            default:
                modifierValue = 0;
                return false;
        }
    }

    public static KeyOption GetKeyOption(uint virtualKey)
    {
        return Keys.FirstOrDefault(item => item.VirtualKey == virtualKey) ?? Keys[0];
    }

    private static IReadOnlyList<KeyOption> BuildKeys()
    {
        var options = new List<KeyOption>
        {
            new("`", 0xC0),
            new("-", 0xBD),
            new("=", 0xBB),
            new("0", 0x30),
            new("1", 0x31),
            new("2", 0x32),
            new("3", 0x33),
            new("4", 0x34),
            new("5", 0x35),
            new("6", 0x36),
            new("7", 0x37),
            new("8", 0x38),
            new("9", 0x39)
        };

        for (uint key = 0x41; key <= 0x5A; key++)
        {
            options.Add(new(((char)key).ToString(), key));
        }

        for (uint key = 0x70, index = 1; key <= 0x7B; key++, index++)
        {
            options.Add(new($"F{index}", key));
        }

        return options;
    }
}
