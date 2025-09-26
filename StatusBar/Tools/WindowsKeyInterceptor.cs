// WindowsKeyInterceptor.cs
// Eine eigenständige Klasse, die die Windows-Taste (LWIN/RWIN) global abfängt,
// das Startmenü unterdrückt und Events feuert, auch wenn die App im Hintergrund ist.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace StatusBar {
  /// <summary>
  /// Interceptiert global die Windows-Tasten (links/rechts) via Low-Level-Keyboard-Hook.
  /// Unterdrückt optional die Systemverarbeitung (Startmenü) und feuert Events.
  /// </summary>
  public sealed class WindowsKeyInterceptor : IDisposable {
    // Öffentliches Event: feuert bei LWIN/RWIN KeyDown und KeyUp.
    public event EventHandler<WindowsKeyEventArgs> WindowsKeyEvent;

    /// <summary>
    /// Wenn true (Standard), wird die Windows-Taste für das System blockiert (Startmenü öffnet nicht).
    /// Wenn false, wird nur benachrichtigt, aber nicht blockiert.
    /// </summary>
    public bool SuppressSystemHandling { get; set; } = true;

    private IntPtr _hookHandle = IntPtr.Zero;
    private LowLevelKeyboardProc _proc; // Referenz halten, damit der Delegate nicht vom GC eingesammelt wird.
    private bool _isDisposed;

    // WinAPI Konstanten
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    private const int WM_SYSKEYDOWN = 0x0104;
    private const int WM_SYSKEYUP = 0x0105;

    private const int VK_LWIN = 0x5B;
    private const int VK_RWIN = 0x5C;

    // Delegate für SetWindowsHookEx
    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    // Struktur von KBDLLHOOKSTRUCT
    [StructLayout(LayoutKind.Sequential)]
    private struct KBDLLHOOKSTRUCT {
      public int vkCode;
      public int scanCode;
      public int flags;
      public int time;
      public IntPtr dwExtraInfo;
    }

    // P/Invoke Deklarationen
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    /// <summary>
    /// Installiert den globalen Low-Level-Keyboard-Hook.
    /// </summary>
    public void Start() {
      ThrowIfDisposed();
      if (_hookHandle != IntPtr.Zero)
        return;

      _proc = HookCallback;

      // Modulhandle des aktuellen Prozesses besorgen.
      Process current = Process.GetCurrentProcess();
      ProcessModule module = current.MainModule;
      IntPtr hModule = GetModuleHandle(module.ModuleName);

      _hookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, hModule, 0);
      if (_hookHandle == IntPtr.Zero) {
        int error = Marshal.GetLastWin32Error();
        throw new InvalidOperationException("SetWindowsHookEx(WH_KEYBOARD_LL) fehlgeschlagen. Win32Error=" + error);
      }
    }

    /// <summary>
    /// Entfernt den Hook.
    /// </summary>
    public void Stop() {
      if (_hookHandle == IntPtr.Zero)
        return;

      bool ok = UnhookWindowsHookEx(_hookHandle);
      _hookHandle = IntPtr.Zero;
      _proc = null;

      if (!ok) {
        int error = Marshal.GetLastWin32Error();
        throw new InvalidOperationException("UnhookWindowsHookEx fehlgeschlagen. Win32Error=" + error);
      }
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
      // nCode < 0 -> immer weiterreichen
      if (nCode >= 0) {
        int msg = wParam.ToInt32();

        if (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN || msg == WM_KEYUP || msg == WM_SYSKEYUP) {
          KBDLLHOOKSTRUCT data = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
          bool isWinKey = (data.vkCode == VK_LWIN) || (data.vkCode == VK_RWIN);

          if (isWinKey) {
            bool isDown = (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN);
            WindowsKeySide side = data.vkCode == VK_LWIN ? WindowsKeySide.Left : WindowsKeySide.Right;

            // Event feuern (try/catch, damit Hook stabil bleibt)
            try {
              EventHandler<WindowsKeyEventArgs> handler = WindowsKeyEvent;
              if (handler != null) {
                handler(this, new WindowsKeyEventArgs(side, isDown));
              }
            }
            catch {
              // Fehler im Nutzer-Handler dürfen den Hook nicht töten.
            }

            if (SuppressSystemHandling) {
              // Nicht weiterreichen -> Startmenü etc. wird nicht ausgelöst.
              return new IntPtr(1);
            }
          }
        }
      }

      return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
    }

    private void ThrowIfDisposed() {
      if (_isDisposed) {
        throw new ObjectDisposedException(nameof(WindowsKeyInterceptor));
      }
    }

    public void Dispose() {
      if (_isDisposed)
        return;
      try {
        Stop();
      }
      catch {
        // Beim Dispose keine Exceptions nach außen eskalieren.
      }
      _isDisposed = true;
      GC.SuppressFinalize(this);
    }
  }

  /// <summary>
  /// EventArgs mit Seite (links/rechts) und Zustand (Down/Up).
  /// </summary>
  public sealed class WindowsKeyEventArgs : EventArgs {
    public WindowsKeySide Side { get; }
    public bool IsKeyDown { get; }
    public bool IsKeyUp { get { return !IsKeyDown; } }

    public WindowsKeyEventArgs(WindowsKeySide side, bool isKeyDown) {
      Side = side;
      IsKeyDown = isKeyDown;
    }
  }

  public enum WindowsKeySide {
    Left,
    Right
  }
}