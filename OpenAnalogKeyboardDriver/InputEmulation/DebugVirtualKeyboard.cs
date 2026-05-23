namespace OpenAnalogKeyboardDriver.InputEmulation;

/// <summary>
/// A dummy implementation of a <see cref="VirtualKeyboard"/> for debugging purposes.
/// </summary>
public class DebugVirtualKeyboard : VirtualKeyboard
{
    public override void SetPressedState(byte hidKeyCode, bool state)
    {
        Logger.Debug($"SetPressedState({hidKeyCode}, {state})");
    }

    public override void Sync()
    {
        Logger.Debug($"Sync");
    }
}