using OpenAnalogKeyboardDriver;
using OpenAnalogKeyboardDriver.Devices;
using OpenAnalogKeyboardDriver.InputEmulation;
using OpenAnalogKeyboardDriver.InputGrabber;
using OpenAnalogKeyboardDriver.InputProcessing;
using OpenAnalogKeyboardDriver.InputProcessing.Handlers;
using OpenAnalogKeyboardDriver.Readers;

DriverUtils.LogInformation();
DriverUtils.InitializeThread();

Device[] devices = [..DeviceManager.FindDevices()];
Device device = devices[0];

IInputGrabber inputGrabber = device.CreateInputGrabber();
VirtualKeyboard virtualKeyboard = VirtualKeyboard.CreatePlatformInstance();
AnalogReader analogReader = device.CreateAnalogReader();
InputProcessor processor = new(analogReader, virtualKeyboard);
processor.InputHandlers.Add(new DefaultInputHandler());

virtualKeyboard.Initialize();
inputGrabber.Grab();
analogReader.Open();

while (true)
    processor.Tick();