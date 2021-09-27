using System;
using System.IO.Ports;
using System.Linq;

namespace SerialPortDataEmulatorConsole.SerialProtocols
{
    class TemplateEmulator : ISerialEmulator
    {
        // Set message(s), which will be transfered to the serial port
        private readonly byte[][] Messages = new byte[][] {
            new byte[] {0x55, 0x44, 0x54, 0x32, 0x30, 0x30, 0xe9 },
            new byte[] {0x55, 0x44, 0x54, 0x32, 0x30, 0x30, 0xed },
         };

        // interval settings
        private UInt32 TxInterval;
        private SerialPort Port;

        // private vars
        private long Timestamp;
        private uint MsgIndex;

        // Initialize serial port settings and Send intervals here
        public void Init(SerialPort port)
        {
            this.Port = port;

            this.Port.StopBits = StopBits.One;
            this.Port.BaudRate = GetBaudrate();
            this.TxInterval = GetTxInterval();
            this.Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

            this.Port.Open();

            Console.WriteLine($"Emulator Initialized. {port.PortName}, Baudrate: {GetBaudrate()}, Tx interval: {GetTxInterval()} ms");

            // Additional stuff required by the implementation goes here
        }

        public void Trigger()
        {
            if (this.TxInterval == 0 ||
                GetTimestamp() - this.Timestamp < this.TxInterval)
            {
                return;
            }

            // send data
            this.Send();

            // update timestamp
            this.Timestamp = GetTimestamp();
        }

        // Send each message form the Message array
        bool Send()
        {
            if (!Port.IsOpen)
            {
                return false;
            }

            Console.WriteLine($"VDO D8 Emulator Send: {BitConverter.ToString(Messages[MsgIndex]).Replace("-","")}");

            Port.Write(Messages[MsgIndex], 0, Messages[MsgIndex].Length);
            MsgIndex++;
            MsgIndex = (uint)(MsgIndex % Messages.Count());

            return true;
        }

        private long GetTimestamp()
        {
            return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
        }

        private int GetBaudrate()
        {
            return 9600;
        }

        private UInt32 GetTxInterval()
        {
            return 1000;
        }

        public string GetMenuString()
        {
            return "set Menu entry which will be displayed on app startup";
        }
    }
}
