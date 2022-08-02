using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace SerialPortDataEmulatorConsole.SerialProtocols
{
    class DragerSerialDemo : ISerialEmulator
    {
        private const int TX_INTERVAL_MS = 1500;
        private byte[] packet_read_device_data = new byte[] { 0x11, 0x4c, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00, 0x78, 0xe9 };
        private byte[] packet_heartbeat = new byte[] { 0x11, 0x4c, 0x00, 0x00, 0x00, 0x07, 0x00, 0x30, 0x00, 0x01, 0x1a, 0x10, 0x0b, 0x04, 0x12, 0x31,
                                                    0x31, 0x30, 0x35, 0x32, 0x30, 0x31, 0x38, 0x2d, 0x2d, 0x2d, 0x2d, 0x2d, 0x2d, 0x00, 0x00, 0x00,
                                                    0x32, 0x39, 0x30, 0x37, 0x32, 0x30, 0x31, 0x37, 0x2d, 0x2d, 0x2d, 0x2d, 0x2d, 0x2d, 0x00, 0x00,
                                                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xc1, 0xcf};
        private byte[] packet_event = new byte[] { 0x11, 0x4c, 0x00, 0x00, 0x00, 0x08, 0x00, 0x30, 0x00, 0x28, 0x05, 0x08, 0x0c, 0x04, 0x12, 0x01,
                                                    0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x09, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x37, 0x83};

        private SerialPort Port;

        // interval settings
        private UInt32 TxInterval;

        // private vars
        private long Timestamp;

        public void Init(SerialPort port)
        {
            this.Port = port;
            this.Port.StopBits = StopBits.One;
            this.Port.Parity = Parity.None;
            this.Port.BaudRate = GetBaudrate();
            this.TxInterval = GetTxInterval();

            this.Port.Open();

            Console.WriteLine($"Drager. {port.PortName}, Baudrate: {GetBaudrate()}, Tx interval: {GetTxInterval()} ms");

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

            this.Send();

            this.WaitResponse();

            this.Timestamp = GetTimestamp();
        }

        static int msg_counter = 0;
        bool Send()
        {
            if (!Port.IsOpen)
            {
                return false;
            }

            msg_counter++;
            
            byte[] cmd;
            if (msg_counter % 3 == 0)
            {
                cmd = packet_read_device_data;
            }
            else if (msg_counter % 5 == 0)
            {
                cmd = packet_heartbeat;
            }
            else if (msg_counter % 7 == 0)
            {
                cmd = packet_event;
            }
            else
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
            return 115200;
        }

        private UInt32 GetTxInterval()
        {
            return TX_INTERVAL_MS;
        }

        public string GetMenuString()
        {
            return $"Drager, Transmit single Dignita event message with {TX_INTERVAL_MS} ms intervals";
        }
    }
}
