using HidSharp;

namespace OpenAnalogKeyboardDriver.Readers;

public abstract class AnalogReader(HidDevice hidDevice) : IDisposable
{
    private HidStream? _stream;
    
    protected HidStream? Stream => _stream;

    public void Open()
    {
        if (_stream is not null)
            throw new InvalidOperationException("The reader is already open.");
        
        if (!hidDevice.TryOpen(out _stream))
            throw new InvalidOperationException("Failed to open HID device.");

        _stream.ReadTimeout = -1;
        Logger.Info("Reader opened", moduleName: GetType().Name);
    }

    public abstract AnalogKeyReading[] Read();

    public void Dispose()
    {
        _stream?.Dispose();
        _stream = null;
        Logger.Info("Reader closed", moduleName: GetType().Name);
    }
}