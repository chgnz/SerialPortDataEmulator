using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace SerialPortDataEmulatorConsole.SerialProtocols
{
    class TrailoaderEmulator : ISerialEmulator
    {
        private long Timestamp;

        private SerialPort Port;
        private UInt32 TxInterval;
        private UInt32 Counter = 0;

        private readonly string[] NetWeight = new string[] { "000000", "000010", "000250", "000400",
            "001000", "025000", "500550", "800000", "999912", "999999"};
        private readonly string GrossWeight = "999999";
        private readonly string SOF = "$P01T1006";


        private byte[] Msg = new byte[26];

        public void Init(SerialPort port)
        {
            this.Port = port;
            this.Port.StopBits = StopBits.One;

            this.Port.BaudRate = GetBaudrate();
            this.TxInterval = GetTxInterval();

            this.Port.Open();

            Console.WriteLine($"Trailoader emulator. {port.PortName}, Baudrate: {GetBaudrate()}, Tx interval: {GetTxInterval()} ms");

            //Msg = Encoding.ASCII.GetBytes(SOF);
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(SOF), 0, Msg, 0, 9);

            Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        }
        public void Trigger()
        {
            if (this.TxInterval == 0 || GetTimestamp() - this.Timestamp < this.TxInterval)
            {
                return;
            }

            byte[] tempByte1 = new byte[6];
            byte[] tempByte2 = new byte[6];
            byte[] tempByte3 = new byte[4];
            tempByte1 = Encoding.ASCII.GetBytes(NetWeight[Counter]);
            tempByte2 = Encoding.ASCII.GetBytes(GrossWeight);

            Buffer.BlockCopy(tempByte1, 0, Msg, 9, 6);
            Buffer.BlockCopy(tempByte2, 0, Msg, 15, 6);

            UInt16 checksum = 0;
            for (int i = 1; i < 21; i++)
            {
                checksum += Msg[i];
            }

            // convert checksum to hex string 
            string crc = checksum.ToString("X4");
            tempByte3 = Encoding.ASCII.GetBytes(crc);
            Buffer.BlockCopy(tempByte3, 0, Msg, 21, 4);

            // EOF
            Msg[25] = 0x7D;

            Send();

            this.Timestamp = GetTimestamp();
            Counter++;
            if (Counter > 9)
                Counter = 0;
        }
        public string GetMenuString()
        {
            return "Trailoader (automatically transmits every 10s @ 9600baud)";
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

            Console.WriteLine($"Trailoader emulator sent: {Encoding.Default.GetString(Msg)}");

            Port.Write(Msg, 0, 26);

            return true;
        }

        public void DeInit()
        {
            this.Port.Close();
        }

    }
}
