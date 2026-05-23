namespace OpenAnalogKeyboardDriver.InputProcessing;

/// <summary>
/// The state of a key in the <see cref="InputProcessor"/>.
/// </summary>
/// <param name="hidKeyCode">The HID key code.</param>
/// <param name="analogValue">The current analog value.</param>
public class KeyState(byte hidKeyCode, ushort analogValue)
{
    /// <summary>
    /// The HID key code.
    /// </summary>
    public byte HidKeyCode => hidKeyCode;

    /// <summary>
    /// The current analog value.
    /// </summary>
    public ushort AnalogValue { get; set; } = analogValue;

    /// <summary>
    /// Bool whether the key is currently pressed.
    /// </summary>
    public bool IsPressed { get; set; } = false;
}