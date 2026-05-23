namespace OpenAnalogKeyboardDriver.InputProcessing.Handlers;

public interface IInputHandler
{
    void Handle(IReadOnlyList<KeyState> keyStates);
}