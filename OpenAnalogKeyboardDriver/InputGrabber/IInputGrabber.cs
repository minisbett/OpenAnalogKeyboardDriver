namespace OpenAnalogKeyboardDriver.InputGrabber;

/// <summary>
/// Represents a platform-agnostic handle for holding process-exclusivity ("grabbing") over HID input.
/// </summary>
public interface IInputGrabber : IDisposable
{
    /// <summary>
    /// Starts grabbing the HID input.
    /// </summary>
    public void Grab();
}