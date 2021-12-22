using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace SerialPortDataEmulatorConsole.SerialProtocols
{
    class TrueloadEmulator : ISerialEmulator
    {
        private long Timestamp;

        private SerialPort Port;
        private UInt32 TxInterval;
        private UInt32 Counter = 0;

        private readonly byte[][] Messages = new byte[][] {
            new byte[] { 0x01, 0x00, 0x0A, 0x81, 0x7C, 0x01, 0x01, 0x01, 0x01, 0x00, 0x00, 0x4F, 0x24, 0x7E }, // Gross
            new byte[] { 0x01, 0x00, 0x0A, 0x81, 0x7D, 0x01, 0x01, 0x02, 0x00, 0x00, 0x00, 0x14, 0xD8, 0xF8 }, // Axle 1
            new byte[] { 0x01, 0x00, 0x0A, 0x81, 0x7E, 0x01, 0x01, 0x03, 0x00, 0x00, 0x00, 0x14, 0xBB, 0xDD }, // Axle 2
            new byte[] { 0x01, 0x00, 0x0A, 0x81, 0x7B, 0x01, 0x01, 0x00, 0x01, 0x00, 0x00, 0x01, 0x04, 0x0E }, // Net 260kg
            new byte[] { 0x01, 0x00, 0x0A, 0x81, 0x7F, 0x01, 0x01, 0x04, 0x00, 0x00, 0x00, 0x12, 0xD0, 0xF2 }, // Axle 3
            new byte[] { 0x01, 0x00, 0x0A, 0x81, 0x80, 0x01, 0x01, 0x05, 0x00, 0x00, 0x00, 0x12, 0xD0, 0xF4 }, // Axle 4
            new byte[] { 0x01, 0x00, 0x0A, 0x81, 0x4F, 0x01, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0xC8, 0xA5 }, // Net 200kg

        };

        public void Init(SerialPort port)
        {
            this.Port = port;
            this.Port.StopBits = StopBits.One;

            this.Port.BaudRate = GetBaudrate();
            this.TxInterval = GetTxInterval();

            this.Port.Open();

            Console.WriteLine($"Trueload emulator. {port.PortName}, Baudrate: {GetBaudrate()}, Tx interval: {GetTxInterval()} ms");

            Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
        }
        public void Trigger()
        {
            if (this.TxInterval == 0 || GetTimestamp() - this.Timestamp < this.TxInterval)
            {
                return;
            }

            Send();

            Counter++;

            if (Counter >= Messages.Length)
            {
                Counter = 0;
            }

            this.Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
        }
        public string GetMenuString()
        {
            return $"Trueload (automatically transmits every {this.GetTxInterval()} ms @ {this.GetBaudrate()} baud)";
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
            return 10000;
        }
        bool Send()
        {
            if (!Port.IsOpen)
            {
                return false;
            }

            Console.WriteLine($"Trueload emulator sent: {BitConverter.ToString(Messages[Counter])}");

            Port.Write(Messages[Counter],0 , Messages[Counter].Length);

            return true;
        }

    }
}
