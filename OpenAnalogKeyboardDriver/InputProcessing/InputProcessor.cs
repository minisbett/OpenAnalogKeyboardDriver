using System.Diagnostics;
using OpenAnalogKeyboardDriver.InputProcessing.Handlers;
using OpenAnalogKeyboardDriver.Readers;
using OpenAnalogKeyboardDriver.InputEmulation;

namespace OpenAnalogKeyboardDriver.InputProcessing;

/// <summary>
/// The core input processing unit of the driver:<br/>
/// 1. Receives analog input via the specified <see cref="AnalogReader"/><br/>
/// 2. Processes the input through the registered <see cref="IInputHandler"/>s<br/>
/// 3. Outputs the result to the specified <see cref="VirtualKeyboard"/>
/// </summary>
/// <param name="analogReader">The analog reader to read analog input from.</param>
/// <param name="virtualKeyboard">The virtual keyboard to output the key presses to.</param>
public class InputProcessor(AnalogReader analogReader, VirtualKeyboard virtualKeyboard)
{
    /// <summary>
    /// The input handlers used by this input processor.
    /// </summary>
    public List<IInputHandler> InputHandlers { get; } = [];

    /// <summary>
    /// A list of the HID key codes of the currently pressed keys, used to determine whether an input handler changed the pressed state compared to the last tick.
    /// </summary>
    private readonly List<byte> _pressedKeyCodes = [];

    /// <summary>
    /// The current state of all keys an analog value was read for at least once.
    /// </summary>
    private readonly List<KeyState> _keyStates = [];

    /// <summary>
    /// Performs a tick on the input processor, consisting of reading the current analog input, processing it and sending the result to the virtual keyboard.
    /// </summary>
    public void Tick()
    {
        Stopwatch realWatch = Stopwatch.StartNew();

        AnalogKeyReading[] readings = analogReader.Read();

        Stopwatch watch = Stopwatch.StartNew();

        foreach (AnalogKeyReading reading in readings)
        {
            if (_keyStates.FirstOrDefault(x => x.HidKeyCode == reading.HidKeyCode) is not KeyState state)
                _keyStates.Add(new(reading.HidKeyCode, reading.AnalogValue));
            else
                state.AnalogValue = reading.AnalogValue;
        }

        foreach (IInputHandler inputHandler in InputHandlers)
            inputHandler.Handle(_keyStates.AsReadOnly());

        bool dirty = false;
        foreach (KeyState state in _keyStates)
            switch (state.IsPressed)
            {
                case true when !_pressedKeyCodes.Contains(state.HidKeyCode):
                    Logger.Info($"Pressed HidKeyCode={state.HidKeyCode} AnalogValue={state.AnalogValue}");
                    virtualKeyboard.SetPressedState(state.HidKeyCode, true);
                    _pressedKeyCodes.Add(state.HidKeyCode);
                    dirty = true;
                    break;
                case false when _pressedKeyCodes.Contains(state.HidKeyCode):
                    Logger.Info($"Released HidKeyCode={state.HidKeyCode} AnalogValue={state.AnalogValue}");
                    virtualKeyboard.SetPressedState(state.HidKeyCode, false);
                    _pressedKeyCodes.Remove(state.HidKeyCode);
                    dirty = true;
                    break;
            }

        if (dirty)
            virtualKeyboard.Sync();

        watch.Stop();
        realWatch.Stop();
        Logger.Debug($"Process Time: {watch.Elapsed.TotalMilliseconds:N3}ms, Scan Time: {realWatch.Elapsed.TotalMilliseconds:N3}ms");
    }
}