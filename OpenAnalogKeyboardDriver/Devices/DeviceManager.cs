using HidSharp;
using HidSharp.Reports;

namespace OpenAnalogKeyboardDriver.Devices;

/// <summary>
/// A static class for finding and identifying the logical HID devices on the system.
/// </summary>
public static class DeviceManager
{
    /// <summary>
    /// Returns all logical HID devices connected to the system that could be identified as one of <see cref="DeviceType"/>.
    /// </summary>
    /// <returns>The logical HID devices.</returns>
    public static IEnumerable<Device> FindDevices()
    {
        HidDevice[] hidDevices = [.. DeviceList.Local.GetHidDevices()];

        foreach (HidDevice hidDevice in hidDevices)
        {
            if (DetectDeviceType(hidDevice) is DeviceType type)
                yield return new(type, hidDevice);
        }
    }

    /// <summary>
    /// Returns the identified type of the specified logical HID device. If the device could not be identified, null is returned.
    /// </summary>
    /// <param name="hidDevice">The logical HID device to be identified.</param>
    /// <returns>The identified type of the specified logical HID device, or null if it could not be identified.</returns>
    private static DeviceType? DetectDeviceType(HidDevice hidDevice)
    {
        ReportDescriptor descriptor;
        try
        {
            descriptor = hidDevice.GetReportDescriptor();
        }
        catch (Exception ex)
        {
            Logger.Warn($"Could not get report descriptor of {hidDevice.GetFriendlyName()}: {ex.Message}");
            return null;
        }

        switch (hidDevice.VendorID, hidDevice.ProductID)
        {
            case (0x31e3, _): // Any Wooting Device
                // Every Wooting devices has a logical HID device dedicated to the analog protocol.
                // The logical device has a singular input report with the usage 0002 on usage page FF53.
                Report[] inputs = [.. descriptor.InputReports];
                if (inputs.Length is not 1)
                    return null;

                uint[] usages = [.. inputs.Single().GetAllUsages()];
                if (usages.Length is 1 && usages[0] == 0xFF53_0002)
                    return DeviceType.Wooting_FF53;
                
                break;
        }

        return null;
    }
}