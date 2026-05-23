namespace OpenAnalogKeyboardDriver.Readers;

/// <summary>
/// Represents the analog reading of a singular physical key.
/// </summary>
/// <param name="HidKeyCode">The HID key code.</param>
/// <param name="AnalogValue">The analog value, ranging from <see cref="ushort.MinValue"/> to <see cref="ushort.MaxValue"/>.</param>
public record AnalogKeyReading(byte HidKeyCode, ushort AnalogValue);