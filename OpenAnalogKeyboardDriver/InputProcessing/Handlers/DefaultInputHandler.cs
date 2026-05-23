namespace OpenAnalogKeyboardDriver.InputProcessing.Handlers;

public class DefaultInputHandler : IInputHandler
{
    public void Handle(IReadOnlyList<KeyState> keyStates)
    {
        foreach (KeyState state in keyStates)
        {
            if (state.AnalogValue >= ushort.MaxValue * 0.45)
                state.IsPressed = true;
            else if (state.AnalogValue < ushort.MaxValue * 0.45)
                state.IsPressed = false;
        }
    }
}