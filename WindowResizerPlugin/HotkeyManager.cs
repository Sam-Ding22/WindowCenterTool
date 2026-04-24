using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace WindowResizerPlugin;

public sealed class HotkeyManager : IDisposable
{
    private int _nextHotkeyId = 1;
    private readonly List<(int Id, uint Modifier, uint Key)> _registeredHotkeys = new();
    private readonly ManualResetEventSlim _readyEvent = new(false);

    private Thread? _messageLoopThread;
    private IntPtr _windowHandle = IntPtr.Zero;
    private bool _registerSuccess;
    private bool _disposed;

    public event EventHandler<HotKeyEventArgs>? HotkeyPressed;

    public bool RegisterHotkeys(params (uint Modifiers, uint VirtualKey)[] hotkeys)
    {
        ThrowIfDisposed();

        if (hotkeys.Length == 0)
        {
            return false;
        }

        UnregisterHotkey();
        _readyEvent.Reset();
        _registerSuccess = false;
        _registeredHotkeys.Clear();

        foreach (var hotkey in hotkeys)
        {
            _registeredHotkeys.Add((_nextHotkeyId++, hotkey.Modifiers, hotkey.VirtualKey));
        }

        _messageLoopThread = new Thread(MessageLoop)
        {
            IsBackground = true
        };
        _messageLoopThread.SetApartmentState(ApartmentState.STA);
        _messageLoopThread.Start();

        _readyEvent.Wait(TimeSpan.FromSeconds(3));
        return _registerSuccess;
    }

    [Obsolete("Use RegisterHotkeys instead.")]
    public bool RegisterHotkey(IntPtr _)
    {
        return RegisterHotkeys((WindowsApiWrapper.MOD_ALT, 0xBD));
    }

    public void UnregisterHotkey()
    {
        if (_windowHandle != IntPtr.Zero)
        {
            WindowsApiWrapper.PostMessage(_windowHandle, WindowsApiWrapper.WM_APP_QUIT, IntPtr.Zero, IntPtr.Zero);
        }

        if (_messageLoopThread is { IsAlive: true })
        {
            _messageLoopThread.Join(TimeSpan.FromSeconds(2));
        }

        _windowHandle = IntPtr.Zero;
        _messageLoopThread = null;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        UnregisterHotkey();
        _readyEvent.Dispose();
        _disposed = true;
    }

    private void MessageLoop()
    {
        _windowHandle = CreateHiddenWindow();
        if (_windowHandle == IntPtr.Zero)
        {
            _readyEvent.Set();
            return;
        }

        _registerSuccess = true;
        foreach (var (id, modifier, key) in _registeredHotkeys)
        {
            if (!WindowsApiWrapper.RegisterHotKey(_windowHandle, id, modifier, key))
            {
                _registerSuccess = false;
            }
        }

        _readyEvent.Set();

        if (!_registerSuccess)
        {
            CleanupRegisteredHotkeys();
            return;
        }

        var registeredIds = new HashSet<int>();
        foreach (var (id, _, _) in _registeredHotkeys)
        {
            registeredIds.Add(id);
        }

        while (WindowsApiWrapper.GetMessage(out WindowsApiWrapper.MSG msg, IntPtr.Zero, 0, 0) > 0)
        {
            if (msg.message == WindowsApiWrapper.WM_APP_QUIT)
            {
                break;
            }

            if (msg.message == WindowsApiWrapper.WM_HOTKEY && registeredIds.Contains(msg.wParam.ToInt32()))
            {
                var hotkeyId = msg.wParam.ToInt32();
                var entry = _registeredHotkeys.Find(item => item.Id == hotkeyId);
                HotkeyPressed?.Invoke(this, new HotKeyEventArgs
                {
                    HotkeyId = hotkeyId,
                    Modifier = entry.Modifier,
                    VirtualKey = entry.Key
                });
            }

            WindowsApiWrapper.TranslateMessage(ref msg);
            WindowsApiWrapper.DispatchMessage(ref msg);
        }

        CleanupRegisteredHotkeys();
    }

    private void CleanupRegisteredHotkeys()
    {
        foreach (var (id, _, _) in _registeredHotkeys)
        {
            if (_windowHandle != IntPtr.Zero)
            {
                WindowsApiWrapper.UnregisterHotKey(_windowHandle, id);
            }
        }
    }

    private static IntPtr CreateHiddenWindow()
    {
        var className = "HotkeyMsgWnd_" + Guid.NewGuid().ToString("N")[..8];
        WindowsApiWrapper.WNDPROC wndProc = WindowsApiWrapper.DefWindowProc;

        var wc = new WindowsApiWrapper.WNDCLASS
        {
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(wndProc),
            hInstance = WindowsApiWrapper.GetModuleHandle(null),
            lpszClassName = className
        };

        var atom = WindowsApiWrapper.RegisterClass(ref wc);
        if (atom == 0)
        {
            return IntPtr.Zero;
        }

        return WindowsApiWrapper.CreateWindowEx(
            0,
            className,
            "Hotkey Message Window",
            0,
            0,
            0,
            0,
            0,
            IntPtr.Zero,
            IntPtr.Zero,
            wc.hInstance,
            IntPtr.Zero);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(HotkeyManager));
        }
    }
}

public sealed class HotKeyEventArgs : EventArgs
{
    public int HotkeyId { get; set; }

    public uint Modifier { get; set; }

    public uint VirtualKey { get; set; }
}
