using System.IO.Ports;

namespace SerialPortDataEmulatorConsole.SerialProtocols
{
    public interface ISerialEmulator
    {
        void Init(SerialPort port);
        void Trigger();
        void DeInit();
        string GetMenuString();
    }

    public interface IFuelSensorEmulator
    {
        void SetFixedFuelValue(int value);
        void EnableFixedValueMode();
        void EnableRandomValueMode();
    }
}
