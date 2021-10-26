using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace SerialPortDataEmulatorConsole.SerialProtocols
{
    class DignitaSerialDemo : ISerialEmulator
    {
        // interval settings
        private UInt32 TxInterval;
        private SerialPort Port;

        // private vars
        private long Timestamp;
        private int UdsMsgIndex;

        public void Init(SerialPort port)
        {
            this.Port = port;
            this.Port.StopBits = StopBits.One;
            this.Port.Parity = Parity.None;
            this.Port.BaudRate = GetBaudrate();
            this.TxInterval = GetTxInterval();

            this.Port.Open();

            Console.WriteLine($"dignita demo test. {port.PortName}, Baudrate: {GetBaudrate()}, Tx interval: {GetTxInterval()} ms");

            Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        }
        bool WaitResponse()
        {
            TimeSpan maxDuration = TimeSpan.FromMilliseconds(1000);
            Stopwatch sw = Stopwatch.StartNew();
            Stopwatch lastDataRx = null;

            byte[] buffer = new byte[256];
            int offset = 0;

            while (sw.Elapsed < maxDuration && (lastDataRx == null || lastDataRx.ElapsedMilliseconds < 50))
            {
                if (Port.BytesToRead > 0)
                {
                    int data_size = Port.BytesToRead;
                    Port.Read(buffer, offset, data_size);
                    offset += data_size;

                    if (lastDataRx == null)
                    {
                        lastDataRx = Stopwatch.StartNew();
                    }

                }

                Thread.Sleep(1);
            }

            if (offset > 0)
            {
                Console.WriteLine($"received: {offset} bytes: {BitConverter.ToString(buffer, 0, offset)}");
            }

            return false;
        }

        public void Trigger()
        {
            if (this.TxInterval == 0 ||
                GetTimestamp() - this.Timestamp < this.TxInterval)
            {
                // nav pienācies laiks sūtīt
                return;
            }

            if (Console.KeyAvailable && Console.ReadKey().Key == ConsoleKey.Enter)
            {
                this.Send();
            }


            this.WaitResponse();

            this.Timestamp = GetTimestamp();
        }

        readonly byte[] cmd = new byte[] { 0x02, 0xff, 0xff, 0xff, 0xff, 0xff, 0x64, 0x00, 0xff };
        private byte counter = 0;

        bool Send()
        {
            if (!Port.IsOpen)
            {
                return false;
            }


            Port.Write(cmd, 0, cmd.Length);
            Console.WriteLine($"Command sent: {BitConverter.ToString(cmd)}");

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
            return 250;
        }

        public string GetMenuString()
        {
            return "Dignita, hack, to test comms";
        }
    }
}
