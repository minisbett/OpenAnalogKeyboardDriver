using System.Runtime.InteropServices;

namespace OpenAnalogKeyboardDriver.Native;

/// <summary>
/// Provides interaction with native functions (libc) on Linux.
/// </summary>
public static class LinuxNative
{
    public const int O_WRONLY = 0x1;
    public const int O_NONBLOCK = 0x800;

    public const uint UI_SET_EVBIT = 0x40045564;
    public const uint UI_SET_KEYBIT = 0x40045565;
    public const uint UI_DEV_SETUP = 0x405C5503;
    public const uint UI_DEV_CREATE = 0x5501;
    public const uint UI_DEV_DESTROY = 0x5502;

    public const ushort EV_SYN = 0x0;
    public const ushort EV_KEY = 0x1;
    
    public const ushort SYN_REPORT = 0x0;
    
    public const uint EVIOCGRAB = 0x40044590;
    
    [DllImport("libc")]
    public static extern int open(string path, int flags);

    [DllImport("libc")]
    public static extern int ioctl(int fd, uint request, int arg);

    [DllImport("libc")]
    public static extern int ioctl(int fd, uint request, ref UInputSetup arg);

    [DllImport("libc")]
    public static extern int write(int fd, IntPtr ptr, int count);

    [DllImport("libc")]
    public static extern int close(int fd);

    public struct InputEvent
    {
        public long Sec;
        public long USec;
        public ushort Type;
        public ushort Code;
        public int Value;
    };

    public struct InputId
    {
        public ushort BusType;
        public ushort Vendor;
        public ushort Product;
        public ushort Version;
    }

    public struct UInputSetup
    {
        public InputId Id;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string Name;

        public uint FfEffectsMax;
    }
}