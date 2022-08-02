using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;

namespace SerialPortDataEmulatorConsole.SerialProtocols
{
    class Test : ISerialEmulator
    {
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

        static char c = 'h';
        static char c2 = '1';
        static int counter = 0;

        // Send each message form the Message array
        bool Send()
        {
            if (!Port.IsOpen)
            {
                return false;
            }

            byte[] line = { (byte)c, (byte)c2, (byte)'\r' };

            Port.Write($"u{counter}\r");
            Console.Write($"u{counter++}");
            waitResponse();

            return true;
        }
        private bool waitResponse()
        {
            TimeSpan maxDuration = TimeSpan.FromMilliseconds(2200);
            Stopwatch sw = Stopwatch.StartNew();

            byte[] buffer = new byte[256];
            int offset = 0;

            while (sw.Elapsed < maxDuration)
            {
                if (Port.BytesToRead == 0)
                {
                    continue;
                }

                int dataAvailable = Port.BytesToRead;
                Port.Read(buffer, offset, dataAvailable);
                offset += dataAvailable;

                if (buffer[offset-1] == (byte)'\r')
                {
                    string msg = System.Text.Encoding.ASCII.GetString(buffer, 0, offset - 1);
                    Console.WriteLine($"Message received in {sw.Elapsed.Milliseconds} ms: '{msg}'");

                    return true;
                }
            }

            if (offset > 0)
            {
                string msg = System.Text.Encoding.ASCII.GetString(buffer, 0, offset);
                Console.WriteLine($"Timeout occured Received {offset} bytes: '{msg}'");
            }

            return false;
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
            return 110;
        }

        public string GetMenuString()
        {
            return "sandbox mode";
        }
    }
}
