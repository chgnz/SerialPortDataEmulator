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
    class Millennium5Emulator : ISerialEmulator
    {
        // Msg to send
        private readonly string[] VEIMillennium5_MSG = new string[] {
            "$P01|M5G|0020|170321121509|||||65535|0767083655|0C8B}",
            "$P01|M5G|0018|170321121617||00000380|00001|TEST|MONEY||||A7||65535|0767083655|1536}",
            "$P01|M5G|0018|170321121631||00000380|00001|TEST|MONEY||||A7||65535|0767083655|1532}",
            "$P01|M5G|0018|170321121647||00000380|00001|TEST|MONEY||||A7||65535|0767083655|1539}"
          };

        // interval settings
        private UInt32 TxInterval;
        private SerialPort Port;

        // private vars
        private long Timestamp;
        private uint MsgIndex;

        public void Init(SerialPort port)
        {
            this.Port = port;
            this.Port.StopBits = StopBits.One;
            this.Port.BaudRate = GetBaudrate();
            this.TxInterval = GetTxInterval();

            this.Port.Open();

            Console.WriteLine($"VEI Millennium Emulator Initialized. {port.PortName}, Baudrate: {GetBaudrate()}, Tx interval: {GetTxInterval()} ms");

            Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
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

            this.Timestamp = GetTimestamp();
        }

        bool Send()
        {
            if (!Port.IsOpen)
            {
                return false;
            }

            Console.WriteLine($"VEI MIllennium5 Emulator Send: {VEIMillennium5_MSG[MsgIndex]}");

            Port.Write(ASCIIEncoding.ASCII.GetBytes(VEIMillennium5_MSG[MsgIndex]), 0, VEIMillennium5_MSG[MsgIndex].Length);
            MsgIndex++;
            MsgIndex = (uint)(MsgIndex % VEIMillennium5_MSG.Count());

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
            return 10000;
        }

        public string GetMenuString()
        {
            return $"Emulate VEI protocol by sending messages, with {GetTxInterval()} ms intervals @ {GetBaudrate()} baud";
        }

        public void DeInit()
        {
            this.Port.Close();
        }
    }
}
