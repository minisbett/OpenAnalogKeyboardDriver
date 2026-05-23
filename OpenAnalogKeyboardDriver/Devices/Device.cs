using HidSharp;
using OpenAnalogKeyboardDriver.InputGrabber;
using OpenAnalogKeyboardDriver.Readers;

namespace OpenAnalogKeyboardDriver.Devices;

/// <summary>
/// Represents an identified device in the driver.
/// </summary>
/// <param name="Type">The type of the device.</param>
/// <param name="AnalogHidDevice">The logical HID device used for analog reading.</param>
public record Device(DeviceType Type, HidDevice AnalogHidDevice)
{
    /// <summary>
    /// Creates the corresponding <see cref="AnalogReader"/> by the type of this device.
    /// </summary>
    /// <returns>An analog reader for this device.</returns>
    public AnalogReader CreateAnalogReader() => Type switch
    {
        DeviceType.Wooting_FF53 => new WootingAnalogReader_FF53(AnalogHidDevice),
        _ => throw new NotImplementedException($"No analog reader for {Type} implemented.")
    };

    /// <summary>
    /// Creates a platform-specific <see cref="IInputGrabber"/> for this device.
    /// </summary>
    /// <returns>The input grabber for this device.</returns>
    public IInputGrabber CreateInputGrabber()
    {
        if (OperatingSystem.IsLinux())
            return new LinuxInputGrabber(AnalogHidDevice);

        throw new PlatformNotSupportedException();
    }

    public override string ToString() => $"{AnalogHidDevice.GetManufacturer()} {AnalogHidDevice.GetFriendlyName()} ({Type})";
}