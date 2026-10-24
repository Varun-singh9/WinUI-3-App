using System.Runtime.InteropServices;

namespace Launcher.Integrations.Hotkey;

/// <summary>
/// The few user32 and kernel32 functions the hotkey service needs, declared by hand (ADR-0024: no new packages).
/// </summary>
internal static class NativeMethods
{
    public const uint WmQuit = 0x0012;
    public const uint WmUser = 0x0400;
    public const uint WmHotkey = 0x0312;
    public const uint WmApp = 0x8000;
    public const uint PmNoRemove = 0x0000;

    public const uint ModAlt = 0x0001;
    public const uint ModControl = 0x0002;
    public const uint ModShift = 0x0004;
    public const uint ModWin = 0x0008;
    public const uint ModNoRepeat = 0x4000;

    public const int ErrorInvalidParameter = 87;
    public const int ErrorHotkeyAlreadyRegistered = 1409;

    /// <summary>The Win32 <c>MSG</c> structure. Fields are public so the compiler does not report them as never assigned.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Msg
    {
        public nint Hwnd;
        public uint Message;
        public nuint WParam;
        public nint LParam;
        public uint Time;
        public int PointX;
        public int PointY;
        public uint Private;
    }

    [DllImport("user32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UnregisterHotKey(nint hWnd, int id);

    [DllImport("user32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static extern int GetMessageW(out Msg message, nint hWnd, uint filterMin, uint filterMax);

    [DllImport("user32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool PeekMessageW(out Msg message, nint hWnd, uint filterMin, uint filterMax, uint removeMessage);

    [DllImport("user32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool PostThreadMessageW(uint threadId, uint message, nuint wParam, nint lParam);

    [DllImport("kernel32.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static extern uint GetCurrentThreadId();
}
