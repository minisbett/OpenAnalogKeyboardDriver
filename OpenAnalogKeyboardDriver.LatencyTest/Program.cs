using System.Buffers.Binary;
using System.Globalization;
using System.Runtime.InteropServices;

Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

object sync = new();
Dictionary<(ushort code, int value), PendingEvent> pending = new();
List<double> deltas = [];

FileStream nativeStream = File.Open("/dev/input/event8", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
FileStream driverStream = File.Open("/dev/input/event27", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

Task nativeTask = Task.Run(() => ReadLoop("Native", nativeStream));
Task driverTask = Task.Run(() => ReadLoop("Driver", driverStream));

await Task.WhenAll(nativeTask, driverTask);

return;

void HandleEvent(string deviceName, ushort code, int value, long timestampNs)
{
    lock (sync)
    {
        if (pending.TryGetValue((code, value), out PendingEvent existing))
        {
            if (existing.DeviceName == deviceName)
                return;

            long nativeNs = deviceName == "Native" ? timestampNs : existing.TimestampNs;
            long driverNs = deviceName == "Driver" ? timestampNs : existing.TimestampNs;
            long deltaNs = driverNs - nativeNs;
            deltas.Add(deltaNs);

            Console.WriteLine($"key={code} value={value} Δ={deltaNs / 1_000_000d,6:F3} ms avg={deltas.Average() / 1_000_000,6:F3} ms driver_faster={deltas.Count(x => x < 0) * 1d / deltas.Count:N3}");
            pending.Remove((code, value));
        }
        else
        {
            pending[(code, value)] = new()
            {
                DeviceName = deviceName,
                TimestampNs = timestampNs
            };
        }
    }
}

void ReadLoop(string deviceName, FileStream stream)
{
    Span<byte> buffer = stackalloc byte[InputEvent.SIZE];

    while (true)
    {
        if (stream.Read(buffer) is not InputEvent.SIZE)
            continue;

        InputEvent e = InputEvent.FromBytes(buffer);

        if (e.Type is not 1)
            continue;

        long timestampNs = e.Seconds * 1_000_000_000 + e.Microseconds * 1_000;
        HandleEvent(deviceName, e.Code, e.Value, timestampNs);
    }
}

internal struct PendingEvent
{
    public string DeviceName;
    public long TimestampNs;
}

[StructLayout(LayoutKind.Sequential)]
internal struct InputEvent
{
    public long Seconds;
    public long Microseconds;
    public ushort Type;
    public ushort Code;
    public int Value;

    public const int SIZE = 24;

    public static InputEvent FromBytes(Span<byte> data)
        => new()
        {
            Seconds = BinaryPrimitives.ReadInt64LittleEndian(data[..8]),
            Microseconds = BinaryPrimitives.ReadInt64LittleEndian(data[8..16]),
            Type = BinaryPrimitives.ReadUInt16LittleEndian(data[16..18]),
            Code = BinaryPrimitives.ReadUInt16LittleEndian(data[18..20]),
            Value = BinaryPrimitives.ReadInt32LittleEndian(data[20..24]),
        };
}