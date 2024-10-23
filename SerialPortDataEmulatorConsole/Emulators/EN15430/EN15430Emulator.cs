using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SerialPortDataEmulatorConsole.SerialProtocols
{
    /// <summary>
    ///  !!!!!!!!
    ///  ONLY TIMESYNC PACKET TRANSMIT IS ADDED.
    ///  MODE DOENST WORK!
    /// </summary>
    class EN15430Emulator : ISerialEmulator
    {
        private bool isSynchonized = false;
        private SerialPort Port;

        // interval settings
        private UInt32 TxInterval;

        // private vars
        private long Timestamp;

        public void Init(SerialPort port)
        {
            this.Port = port;
            this.Port.StopBits = StopBits.One;
            this.Port.BaudRate = GetBaudrate();
            this.Port.Open();

            this.TxInterval = GetTxInterval();
            Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

            Console.WriteLine($"EN15430Emulator. {port.PortName}, Baudrate: {GetBaudrate()}");
        }

        public void Trigger()
        {
            if (!Port.IsOpen)
            {
                Console.WriteLine("PORT Closed");
                Thread.Sleep(500);
                return;
            }

            SendTimeSync();
            HandleIncomingData();

        }

        bool SendTimeSync()
        {
            if (!Port.IsOpen)
            {
                // kļūdains stāvoklis
                return false;
            }

            if (isSynchonized == true)
            {
                // nav nepieciešams
                return true;
            }

            if (this.TxInterval == 0 ||
                this.GetTimestamp() - this.Timestamp < this.TxInterval)
            {
                // nav pienācies laiks sūtīt
                return true;
            }
            byte[] timesyncpacket = new byte[] {
                0x01, 0x31, 0x3b, 0x31, 0x30, 0x3b, 0x31, 0x36, 0x30, 0x32, 0x30, 0x34, 0x38, 0x3b, 0x30, 0x34, 0x36, 0x31, 0x30, 0x32, 0x31, 0x3b, 0x35, 0x3b, 0x41, 0x62, 0x63, 0x3b, 0x45, 0x71, 0x75, 0x69, 0x70, 0x31, 0x3b, 0x3b, 0x3b, 0x0d, 0x0a, 0x36, 0x36, 0x44, 0x39, 0x04
            };

            Port.Write(timesyncpacket, 0, timesyncpacket.Count());

            Console.WriteLine($"data sent: '{BitConverter.ToString(timesyncpacket).Replace("-", "")}', size: {timesyncpacket.Count()}");

            this.Timestamp = GetTimestamp();
            return true;
        }

        private int GetBaudrate()
        {
            return 9600;
        }

        public string GetMenuString()
        {
            return "EN15430 (NOT IMPLEMENTED!)";
        }

        private long GetTimestamp()
        {
            return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
        }

        private UInt32 GetTxInterval()
        {
            return 5000;
        }

        private bool HandleIncomingData()
        {
            const int MIN_PACKET_SIZE = 1;

            if (Port.BytesToRead < MIN_PACKET_SIZE)
            {
                return false;
            }

            TimeSpan maxDuration = TimeSpan.FromMilliseconds(100);
            Stopwatch sw = Stopwatch.StartNew();

            while (Port.BytesToRead > 0 && sw.Elapsed < maxDuration)
            {
                byte[] buffer = new byte[Port.BytesToRead];
                Port.Read(buffer, 0, buffer.Length);
                Console.WriteLine($"Data Received: 0x{BitConverter.ToString(buffer).Replace("-", "")}");
            }

            return false;
        }

    }
}
