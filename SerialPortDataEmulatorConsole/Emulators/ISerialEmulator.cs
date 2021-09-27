using System.IO.Ports;

namespace SerialPortDataEmulatorConsole.SerialProtocols
{
    interface ISerialEmulator
    {
        void Init(SerialPort port);
        void Trigger();
        string GetMenuString();
    }
}
