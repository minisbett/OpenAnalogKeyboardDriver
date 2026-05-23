using System.Runtime.InteropServices;
using OpenAnalogKeyboardDriver.Native;

namespace OpenAnalogKeyboardDriver.InputEmulation;

/// <summary>
/// A <see cref="VirtualKeyboard"/> for the Linux operating system.
/// </summary>
public class LinuxVirtualKeyboard : VirtualKeyboard
{
    private const string NAME = "OpenAnalogKeyboardDriver Virtual Keyboard";
    
    /// <summary>
    /// The handle of the virtual keyboard.
    /// </summary>
    private int? _deviceHandle;

    /// <inheritdoc />
    public override void Initialize()
    {
        if(_deviceHandle is not null)
            throw new InvalidOperationException($"The device already exists.");
        
        _deviceHandle = LinuxNative.open("/dev/uinput", LinuxNative.O_WRONLY | LinuxNative.O_NONBLOCK);
        _deviceHandle = _deviceHandle < 0 ? null : _deviceHandle;
        if (_deviceHandle is null)
            throw new InvalidOperationException($"Failed to open /dev/uinput (errno={Marshal.GetLastWin32Error()})");
        
        LinuxNative.ioctl(_deviceHandle.Value, LinuxNative.UI_SET_EVBIT, LinuxNative.EV_KEY);
        LinuxNative.ioctl(_deviceHandle.Value, LinuxNative.UI_SET_EVBIT, LinuxNative.EV_SYN);

        for (int i = 0; i <= 255; i++)
            LinuxNative.ioctl(_deviceHandle.Value, LinuxNative.UI_SET_KEYBIT, i);

        LinuxNative.UInputSetup setup = new()
        {
            Id = new()
            {
                BusType = 0x03,
                Vendor = 0x727,
                Product = 0x727,
                Version = 1
            },
            Name = NAME,
            FfEffectsMax = 0
        };

        if (LinuxNative.ioctl(_deviceHandle.Value, LinuxNative.UI_DEV_SETUP, ref setup) < 0)
            throw new InvalidOperationException($"Failed UI_DEV_SETUP (errno={Marshal.GetLastWin32Error()})");

        if (LinuxNative.ioctl(_deviceHandle.Value, LinuxNative.UI_DEV_CREATE, 0) < 0)
            throw new InvalidOperationException($"Failed UI_DEV_CREATE (errno={Marshal.GetLastWin32Error()})");
        
        Logger.Info($"Created \"{NAME}\"");
    }

    /// <inheritdoc />
    public override void SetPressedState(byte hidKeyCode, bool state)
    {
        if(_deviceHandle is null)
            throw new InvalidOperationException($"The device has not been created.");
        
        Send(LinuxNative.EV_KEY, ToLinuxKey(hidKeyCode), state ? 1 : 0);
    }
    
    /// <inheritdoc />
    public override void Sync()
    {
        if(_deviceHandle is null)
            throw new InvalidOperationException($"The device has not been created.");
        
        Send(LinuxNative.EV_SYN, LinuxNative.SYN_REPORT, 0);
    }
    
    public override void Dispose()
    {
        if (_deviceHandle is null)
            return;

        LinuxNative.ioctl(_deviceHandle.Value, LinuxNative.UI_DEV_DESTROY, 0);
        LinuxNative.close(_deviceHandle.Value);
        _deviceHandle = null;
        Logger.Info($"Destroyed \"{NAME}\"");
    }
    
    #region Helper Methods
    
    private void Send(ushort type, ushort code, int value)
    {
        LinuxNative.InputEvent ev = new()
        {
            Type = type,
            Code = code,
            Value = value
        };

        int size = Marshal.SizeOf<LinuxNative.InputEvent>();

        IntPtr ptr = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(ev, ptr, false);
            LinuxNative.write(_deviceHandle!.Value, ptr, size);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }
    
    /// <summary>
    /// Translates the HID key code into an EV key code.
    /// </summary>
    /// <param name="hidKeyCode">The HID key code.</param>
    /// <returns>The EV key code.</returns>
    private static byte ToLinuxKey(ushort hidKeyCode) => hidKeyCode switch
    {
        // Letters
        0x04 => 30, // A
        0x05 => 48, // B
        0x06 => 46, // C
        0x07 => 32, // D
        0x08 => 18, // E
        0x09 => 33, // F
        0x0A => 34, // G
        0x0B => 35, // H
        0x0C => 23, // I
        0x0D => 36, // J
        0x0E => 37, // K
        0x0F => 38, // L
        0x10 => 50, // M
        0x11 => 49, // N
        0x12 => 24, // O
        0x13 => 25, // P
        0x14 => 16, // Q
        0x15 => 19, // R
        0x16 => 31, // S
        0x17 => 20, // T
        0x18 => 22, // U
        0x19 => 47, // V
        0x1A => 17, // W
        0x1B => 45, // X
        0x1C => 21, // Y
        0x1D => 44, // Z

        // Numbers
        0x1E => 2, // 1
        0x1F => 3, // 2
        0x20 => 4, // 3
        0x21 => 5, // 4
        0x22 => 6, // 5
        0x23 => 7, // 6
        0x24 => 8, // 7
        0x25 => 9, // 8
        0x26 => 10, // 9
        0x27 => 11, // 0

        // Basic controls
        0x28 => 28, // Enter
        0x29 => 1, // Escape
        0x2A => 14, // Backspace
        0x2B => 15, // Tab
        0x2C => 57, // Space
        0x2D => 12, // -
        0x2E => 13, // =
        0x2F => 26, // [
        0x30 => 27, // ]
        0x31 => 43, // \
        0x32 => 43, // Non-US #
        0x33 => 39, // ;
        0x34 => 40, // '
        0x35 => 41, // `
        0x36 => 51, // ,
        0x37 => 52, // .
        0x38 => 53, // /
        0x39 => 58, // CapsLock

        // Function keys
        0x3A => 59, // F1
        0x3B => 60, // F2
        0x3C => 61, // F3
        0x3D => 62, // F4
        0x3E => 63, // F5
        0x3F => 64, // F6
        0x40 => 65, // F7
        0x41 => 66, // F8
        0x42 => 67, // F9
        0x43 => 68, // F10
        0x44 => 87, // F11
        0x45 => 88, // F12

        // Navigation
        0x46 => 99, // PrintScreen
        0x47 => 70, // ScrollLock
        0x48 => 119, // Pause
        0x49 => 110, // Insert
        0x4A => 102, // Home
        0x4B => 104, // PageUp
        0x4C => 111, // Delete
        0x4D => 107, // End
        0x4E => 109, // PageDown
        0x4F => 106, // Right
        0x50 => 105, // Left
        0x51 => 108, // Down
        0x52 => 103, // Up

        // Numpad
        0x53 => 69, // NumLock
        0x54 => 98, // KP /
        0x55 => 55, // KP *
        0x56 => 74, // KP -
        0x57 => 78, // KP +
        0x58 => 96, // KP Enter
        0x59 => 79, // KP1
        0x5A => 80, // KP2
        0x5B => 81, // KP3
        0x5C => 75, // KP4
        0x5D => 76, // KP5
        0x5E => 77, // KP6
        0x5F => 71, // KP7
        0x60 => 72, // KP8
        0x61 => 73, // KP9
        0x62 => 82, // KP0
        0x63 => 83, // KP .

        // Extra
        0x64 => 86, // Non-US \
        0x65 => 127, // Application/Menu
        0x66 => 116, // Power
        0x67 => 117, // KP =
        0x68 => 183, // F13
        0x69 => 184, // F14
        0x6A => 185, // F15
        0x6B => 186, // F16
        0x6C => 187, // F17
        0x6D => 188, // F18
        0x6E => 189, // F19
        0x6F => 190, // F20
        0x70 => 191, // F21
        0x71 => 192, // F22
        0x72 => 193, // F23
        0x73 => 194, // F24

        // Modifiers
        0xE0 => 29, // Left Ctrl
        0xE1 => 42, // Left Shift
        0xE2 => 56, // Left Alt
        0xE3 => 125, // Left Meta
        0xE4 => 97, // Right Ctrl
        0xE5 => 54, // Right Shift
        0xE6 => 100, // Right Alt
        0xE7 => 126, // Right Meta

        _ => 0
    };
    
    #endregion
}