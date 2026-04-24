using System;

namespace WindowResizerPlugin
{
    public interface IWindowResizerPlugin
    {
        void ResizeWindow(IntPtr windowHandle, int width, int height);
        void MoveWindow(IntPtr windowHandle, int x, int y);
        void ResizeAndMoveWindow(IntPtr windowHandle, int x, int y, int width, int height);
        void MaximizeWindow(IntPtr windowHandle);
        void MinimizeWindow(IntPtr windowHandle);
        void CenterWindow(IntPtr windowHandle);
        IntPtr GetForegroundWindow();
        IntPtr GetWindowAtCursor();
        void ResizeToFullHeightAndCenter(IntPtr windowHandle);
        void CenterFullHeight(IntPtr windowHandle);
        void ResizeTo1x1Ratio(IntPtr windowHandle);
        void ResizeToLeftQuarter(IntPtr windowHandle);
        void ResizeToRightQuarter(IntPtr windowHandle);
        void ToggleCenteredFullHeight(IntPtr windowHandle);
        bool SetStartup();
        bool RemoveStartup();
        bool IsStartupEnabled();
        bool IsWindowFullHeight(IntPtr windowHandle);
    }
}
