using HidSharp;

namespace OpenAnalogKeyboardDriver.Readers;

public class WootingAnalogReader_FF53(HidDevice device) : AnalogReader(device)
{
    private readonly byte[] _buffer = new byte[65];
    
    public override AnalogKeyReading[] Read()
    {
        if (Stream is null)
            throw new InvalidOperationException("The reader is not opened.");

        Stream.ReadExactly(_buffer);
        
        // Strip the 1-byte HID report ID. This is okay because the logical device sending the FF53 data only has one input report.
        Span<byte> data = _buffer.AsSpan(1);

        // https://github.com/WootingKb/wooting-analog-sdk/blob/be67cbf479eb1e10e2859e71dbdcc12fff7ba266/wooting-analog-sdk/src/plugin.rs#L276
        //   Byte 1   |   Byte 2   |   Byte 3   |   Byte 4
        //  RRRCCCCC     KKKKKKKK     VVVVVVVV     VVNNNN?A
        //  ||||||||     ||||||||     ||||||||     ||||||||
        //  ||||||||     ||||||||     ||||||||     ||||||||
        //  ||||||||     ||||||||     ||||||||     |||||||+-- actuation state (1 bit)
        //  ||||||||     ||||||||     ||||||||     ||||||+--- reserved (1 bit)
        //  ||||||||     ||||||||     ||||||||     ||++++---- key namespace (4 bits)
        //  ||||||||     ||||||||     ++++++++-----++-------- analog value (10 bits)
        //  ||||||||     ++++++++---------------------------- HID key code (8 bits)
        //  |||+++++----------------------------------------- matrix column (5 bits)
        //  +++---------------------------------------------- matrix row (3 bits)
        List<AnalogKeyReading> readings = [];
        for (int i = 0; i < data.Length; i += 4)
        {
            byte matrixPosition = data[i];
            byte hidKeyCode = data[i + 1];
            byte packed = data[i + 2];
            byte valueHigh = data[i + 3];

            // We can confidently tell whether the analog report ended by checking if the next 4 bytes are all 0.
            if (matrixPosition is 0 && hidKeyCode is 0 && packed is 0 && valueHigh is 0)
                break;

            byte matrixRow = (byte)((matrixPosition >> 5) & 0x07);
            byte matrixColumn = (byte)(matrixPosition & 0x1F);
            byte keyNamespace = (byte)((packed >> 2) & 0x0F);
            byte valueLow = (byte)((packed >> 6) & 0x03);
            ushort analogValue = (ushort)((valueHigh << 2) | valueLow);
            ushort scaledAnalogValue = (ushort)(analogValue / 1023f * ushort.MaxValue);
            
            Logger.Debug($"Reading: hidKeyCode={hidKeyCode} analogValue={analogValue} scaledAnalogValue={scaledAnalogValue} matrixRow={matrixRow} matrixColumn={matrixColumn} keyNamespace={keyNamespace}", moduleName: GetType().Name);
            readings.Add(new(hidKeyCode, scaledAnalogValue));
        }

        return [..readings];
    }
}