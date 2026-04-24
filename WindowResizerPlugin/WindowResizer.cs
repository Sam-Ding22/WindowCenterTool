using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WindowResizerPlugin;

public class WindowResizer : IWindowResizerPlugin
{
    private sealed class ManagedWindowState
    {
        public int PreferredWidth { get; set; }
        public bool HasManagedBefore { get; set; }
    }

    [DllImport("user32.dll")]
    private static extern IntPtr WindowFromPoint(WindowsApiWrapper.POINT point);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out WindowsApiWrapper.POINT point);

    private readonly Dictionary<nint, ManagedWindowState> _windowStates = new();
    private double _centerWidthPercent = 0.5;

    public void ResizeWindow(IntPtr windowHandle, int width, int height)
    {
        if (!TryGetWindowRect(windowHandle, out var rect))
        {
            return;
        }

        WindowsApiWrapper.MoveWindow(windowHandle, rect.Left, rect.Top, width, height, true);
    }

    public void MoveWindow(IntPtr windowHandle, int x, int y)
    {
        if (!TryGetWindowRect(windowHandle, out var rect))
        {
            return;
        }

        WindowsApiWrapper.MoveWindow(windowHandle, x, y, rect.Width, rect.Height, true);
    }

    public void ResizeAndMoveWindow(IntPtr windowHandle, int x, int y, int width, int height)
    {
        WindowsApiWrapper.MoveWindow(windowHandle, x, y, width, height, true);
    }

    public void MaximizeWindow(IntPtr windowHandle)
    {
        WindowsApiWrapper.ShowWindow(windowHandle, WindowsApiWrapper.SW_MAXIMIZE);
    }

    public void MinimizeWindow(IntPtr windowHandle)
    {
        WindowsApiWrapper.ShowWindow(windowHandle, WindowsApiWrapper.SW_MINIMIZE);
    }

    public void CenterWindow(IntPtr windowHandle)
    {
        if (!TryGetWindowRect(windowHandle, out var rect))
        {
            return;
        }

        var workArea = GetWorkArea(windowHandle);
        var x = workArea.Left + (workArea.Width - rect.Width) / 2;
        var y = workArea.Top + (workArea.Height - rect.Height) / 2;
        WindowsApiWrapper.MoveWindow(windowHandle, x, y, rect.Width, rect.Height, true);
    }

    public IntPtr GetForegroundWindow()
    {
        return WindowsApiWrapper.GetForegroundWindow();
    }

    public IntPtr GetWindowAtCursor()
    {
        return GetCursorPos(out var point) ? WindowFromPoint(point) : IntPtr.Zero;
    }

    public void ResizeToFullHeightAndCenter(IntPtr windowHandle)
    {
        CenterFullHeight(windowHandle);
    }

    public void CenterFullHeight(IntPtr windowHandle)
    {
        if (!TryGetWindowRect(windowHandle, out var rect))
        {
            return;
        }

        var workArea = GetWorkArea(windowHandle);
        var state = GetOrCreateState(windowHandle, workArea);

        int targetWidth;
        if (state.HasManagedBefore)
        {
            // 已管理过：按窗口当前宽度居中，尊重手动调整
            targetWidth = ClampWidth(rect.Width, workArea);
        }
        else
        {
            // 首次管理：使用全局居中比例
            targetWidth = (int)(workArea.Width * _centerWidthPercent);
        }

        MoveToCenteredFullHeight(windowHandle, workArea, targetWidth);

        // 重新获取实际宽度，更新 _centerWidthPercent 供左右窗口使用
        if (TryGetWindowRect(windowHandle, out var actualRect))
        {
            _centerWidthPercent = Math.Max(0.1, Math.Min(0.9, actualRect.Width / (double)workArea.Width));
        }

        state.PreferredWidth = targetWidth;
        state.HasManagedBefore = true;
    }

    public void ResizeTo1x1Ratio(IntPtr windowHandle)
    {
        if (!TryGetWindowRect(windowHandle, out _))
        {
            return;
        }

        var workArea = GetWorkArea(windowHandle);
        var size = (int)(Math.Min(workArea.Width, workArea.Height) * 0.8);
        var x = workArea.Left + (workArea.Width - size) / 2;
        var y = workArea.Top + (workArea.Height - size) / 2;
        WindowsApiWrapper.MoveWindow(windowHandle, x, y, size, size, true);
        UpdateState(windowHandle, size, workArea);
    }

    public void ResizeToLeftHalf(IntPtr windowHandle)
    {
        ResizeToLeftQuarter(windowHandle);
    }

    public void ResizeToRightHalf(IntPtr windowHandle)
    {
        ResizeToRightQuarter(windowHandle);
    }

    public void ResizeToLeftQuarter(IntPtr windowHandle)
    {
        if (!TryGetWindowRect(windowHandle, out var rect))
        {
            return;
        }

        var workArea = GetWorkArea(windowHandle);
        var width = (int)(workArea.Width * (1 - _centerWidthPercent) / 2);
        WindowsApiWrapper.MoveWindow(windowHandle, workArea.Left, workArea.Top, width, workArea.Height, true);
        UpdateState(windowHandle, width, workArea);
    }

    public void ResizeToRightQuarter(IntPtr windowHandle)
    {
        if (!TryGetWindowRect(windowHandle, out var rect))
        {
            return;
        }

        var workArea = GetWorkArea(windowHandle);
        var width = (int)(workArea.Width * (1 - _centerWidthPercent) / 2);
        var x = workArea.Right - width;
        WindowsApiWrapper.MoveWindow(windowHandle, x, workArea.Top, width, workArea.Height, true);
        UpdateState(windowHandle, width, workArea);
    }

    public void ToggleCenteredFullHeight(IntPtr windowHandle)
    {
        CenterFullHeight(windowHandle);
    }

    public bool SetStartup()
    {
        return StartupManager.SetStartup(System.Reflection.Assembly.GetEntryAssembly()?.Location ?? string.Empty);
    }

    public bool RemoveStartup()
    {
        return StartupManager.RemoveStartup();
    }

    public bool IsStartupEnabled()
    {
        return StartupManager.IsStartupEnabled();
    }

    public bool IsWindowFullHeight(IntPtr windowHandle)
    {
        if (!TryGetWindowRect(windowHandle, out var rect))
        {
            return false;
        }

        var workArea = GetWorkArea(windowHandle);
        return rect.Height >= workArea.Height * 0.95;
    }

    private ManagedWindowState GetOrCreateState(IntPtr windowHandle, Rectangle workArea)
    {
        if (!_windowStates.TryGetValue(windowHandle, out var state))
        {
            state = new ManagedWindowState
            {
                PreferredWidth = workArea.Width / 2,
                HasManagedBefore = false
            };
            _windowStates[windowHandle] = state;
        }

        return state;
    }

    private void UpdateState(IntPtr windowHandle, int width, Rectangle workArea)
    {
        var state = GetOrCreateState(windowHandle, workArea);
        state.PreferredWidth = ClampWidth(width, workArea);
        state.HasManagedBefore = true;
    }

    private static void MoveToCenteredFullHeight(IntPtr windowHandle, Rectangle workArea, int width)
    {
        var clampedWidth = ClampWidth(width, workArea);
        var x = workArea.Left + (workArea.Width - clampedWidth) / 2;
        WindowsApiWrapper.MoveWindow(windowHandle, x, workArea.Top, clampedWidth, workArea.Height, true);
    }

    private static int ClampWidth(int width, Rectangle workArea)
    {
        var minWidth = Math.Min(240, workArea.Width);
        return Math.Max(minWidth, Math.Min(width, workArea.Width));
    }

    private static bool TryGetWindowRect(IntPtr windowHandle, out Rectangle rectangle)
    {
        rectangle = Rectangle.Empty;

        if (windowHandle == IntPtr.Zero ||
            !WindowsApiWrapper.GetWindowRect(windowHandle, out var rect))
        {
            return false;
        }

        rectangle = Rectangle.FromLTRB(rect.left, rect.top, rect.right, rect.bottom);
        return true;
    }

    private static Rectangle GetWorkArea(IntPtr windowHandle)
    {
        var screen = Screen.FromHandle(windowHandle);
        return screen.WorkingArea;
    }
}
