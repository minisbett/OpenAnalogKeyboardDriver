using System.Runtime.InteropServices;
using HidSharp;
using OpenAnalogKeyboardDriver.Native;

namespace OpenAnalogKeyboardDriver.InputGrabber;

/// <summary>
/// An <see cref="IInputGrabber"/> for the Linux operating system, grabbing all input events on the USB port of the specified logical HID device.
/// </summary>
/// <param name="logicalDevice">A logical HID device on the targetted USB port.</param>
public class LinuxInputGrabber(HidDevice logicalDevice) : IInputGrabber
{
    /// <summary>
    /// The file streams to the input events. If null, no inputs are being grabbed.
    /// </summary>
    private FileStream[]? _streams;

    /// <inheritdoc />
    public void Grab()
    {
        if (_streams is not null)
            throw new InvalidOperationException("The input is already grabbed.");

        List<FileStream> streams = [];
        string[] inputEventNames = [.. FindInputEvents()];
        foreach (string inputEventName in inputEventNames)
        {
            FileStream stream = File.Open($"/dev/input/{inputEventName}", FileMode.Open, FileAccess.Read,
                FileShare.ReadWrite);
            streams.Add(stream);

            int handle = stream.SafeFileHandle.DangerousGetHandle().ToInt32();
            if (LinuxNative.ioctl(handle, LinuxNative.EVIOCGRAB, 1) is not 0)
                throw new($"Failed EVIOCGRAB (err={Marshal.GetLastWin32Error()})");
        }

        Logger.Info($"Grabbed {inputEventNames.Length} inputs: {string.Join(", ", inputEventNames)}");
        _streams = [..streams];
    }

    public void Dispose()
    {
        if (_streams is null)
            return;

        foreach (FileStream stream in _streams)
        {
            int handle = stream.SafeFileHandle.DangerousGetHandle().ToInt32();
            LinuxNative.ioctl(handle, LinuxNative.EVIOCGRAB, 0);
            stream.Dispose();
        }

        Logger.Info($"Released {_streams.Length} inputs: {string.Join(", ", _streams.Select(x => Path.GetFileName(x.Name)))}");
        _streams = null;
    }

    /// <summary>
    /// Returns all input event names belonging to the physical device of the passed logical device.
    /// </summary>
    /// <returns>The input event names (eg. "event8").</returns>
    private IEnumerable<string> FindInputEvents()
    {
        // /dev/hidraw11
        string hidrawPath = logicalDevice.GetFileSystemName();

        // /sys/classes/hidraw/hidraw11
        hidrawPath = $"/sys/class/hidraw/{hidrawPath[5..]}";

        // /sys/devices/pci0000:00/0000:00:01.2/0000:02:00.0/usb1/1-2/1-2:1.4/0003:31E3:1312.000C/hidraw/hidraw11
        hidrawPath = new DirectoryInfo(hidrawPath).ResolveLinkTarget(true)!.FullName;

        // /sys/devices/pci0000:00/0000:00:01.2/0000:02:00.0/usb1/1-2/firmware_node
        string firmwareNodesPath = Path.Combine(hidrawPath, "..", "..", "..", "..", "firmware_node");

        // /sys/devices/pci0000:00/0000:00:01.2/0000:02:00.0/usb1/1-2/firmware_node/physical_node<...>
        foreach (string physicalNodePath in Directory.GetDirectories(firmwareNodesPath))
        {
            // /sys/devices/pci0000:00/0000:00:01.2/0000:02:00.0/usb1/1-2/firmware_node/physical_node<...>/input
            string inputsPath = Path.Combine(physicalNodePath, "input");

            if (!Directory.Exists(inputsPath))
                continue;

            // /sys/devices/pci0000:00/0000:00:01.2/0000:02:00.0/usb1/1-2/firmware_node/physical_node<...>/input/input<...>
            foreach (string inputPath in Directory.GetDirectories(inputsPath, "input*"))
                // /sys/devices/pci0000:00/0000:00:01.2/0000:02:00.0/usb1/1-2/firmware_node/physical_node<...>/input/input<...>/event<...>
            foreach (string eventPath in Directory.GetDirectories(inputPath, "event*"))
                yield return Path.GetFileName(eventPath);
        }
    }
}