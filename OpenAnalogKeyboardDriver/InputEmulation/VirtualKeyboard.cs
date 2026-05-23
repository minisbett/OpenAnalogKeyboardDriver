namespace OpenAnalogKeyboardDriver.InputEmulation;

/// <summary>
/// The platform-agnostic base class for sending virtual keyboards inputs to the operating system.
/// </summary>
public abstract class VirtualKeyboard : IDisposable
{
    /// <summary>
    /// Initializes the keyboard (eg. registering the device with the operating system to become available).
    /// </summary>
    public virtual void Initialize()
    {
    }

    /// <summary>
    /// Sets the pressed state of the specified HID key code.
    /// </summary>
    /// <param name="hidKeyCode">The HID key code.</param>
    /// <param name="state">The pressed state (true = pressed).</param>
    public abstract void SetPressedState(byte hidKeyCode, bool state);

    /// <summary>
    /// Confirms the new keyboard state with the operating system. This may not be applicable on all operating systems.
    /// </summary>
    public virtual void Sync()
    {
    }

    public virtual void Dispose()
    {
    }

    /// <summary>
    /// Creates a platform-specific <see cref="VirtualKeyboard"/> instance.
    /// </summary>
    /// <returns>The virtual keyboard.</returns>
    public static VirtualKeyboard CreatePlatformInstance()
    {
        if (OperatingSystem.IsLinux())
            return new LinuxVirtualKeyboard();

        throw new PlatformNotSupportedException();
    }
}