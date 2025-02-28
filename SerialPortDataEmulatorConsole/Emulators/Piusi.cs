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
    class PiusiEmulator : ISerialEmulator
    {
        // Msg to send
        private readonly string[] msg = new string[] {
            "21/07/22 14:26 - 006 NEILANDS   MO4479              0081.02",
            "21/07/22 14:30 - 006 NEILANDS   TRAKT               0299.95",
            "21/07/22 15:26 - 007 KALVINS    1201                0047.06",
            "21/07/22 15:29 - 032 BABANOVS   4320                0113.14",
            "21/07/22 15:34 - 042 FELDMANIS  9851                0205.01",
            "21/07/22 15:47 - 028 MART RIKOV 4318                0130.02",
            "21/07/22 15:53 - 026 PANOVA     2766                0059.07",
            "21/07/22 15:56 - 012 JEGERS     6193                0045.02",
            "21/07/22 16:00 - 034 SMIRNOVS   6354                0065.70",
            "21/07/22 16:03 - 018 BERZINS    3468                0045.02",
            "21/07/22 16:07 - 025 JANISV     9876                0073.06",
            "21/07/22 16:11 - 019 BARTKEVICS 4635                0105.02",
            "21/07/22 16:15 - 038 SKOKOVSKIS 981                 0060.05",
            "21/07/22 16:29 - 009 KASPARS.B  8610                0070.00",
            "21/07/22 16:33 - 033 LAIVENIEKS 4315                0060.00",
            "21/07/22 16:40 - 013 LUIDMANIS  937                 0054.01",
            "21/07/22 17:26 - 011 BRUVERIS   9474                0270.49",
            "22/07/22 05:25 - 006 NEILANDS   TRAKT               0249.91",
            "22/07/22 07:38 - 006 NEILANDS   TRAKT               0399.89",
            "22/07/22 12:15 - 004 Mehanikis  3077                0047.00",
            "22/07/22 13:10 - 018 BERZINS    3468                0032.00",
            "22/07/22 14:28 - 015 JUSKO      9615                0125.19",
            "22/07/22 14:40 - 007 KALVINS    1201                0044.13",
            "22/07/22 14:49 - 014 VEZIS V    GL6031              0138.06",
            "22/07/22 15:11 - 004 Mehanikis  KARCHER             0039.98",
            "22/07/22 15:16 - 019 BARTKEVICS 4635                0080.01",
            "22/07/22 15:20 - 034 SMIRNOVS   6354                0058.94",
            "22/07/22 15:23 - 009 KASPARS.B  8610                0060.00",
            "22/07/22 15:27 - 004 Mehanikis  KARCHER             0039.94",
            "22/07/22 15:29 - 004 Mehanikis  KARCHER             0040.00",
            "22/07/22 15:32 - 033 LAIVENIEKS 4315                0040.07",
            "22/07/22 15:35 - 012 JEGERS     6193                0044.06",
            "22/07/22 15:40 - 038 SKOKOVSKIS 981                 0050.04",
            "22/07/22 15:52 - 026 PANOVA     2766                0047.11",
            "22/07/22 16:15 - 013 LUIDMANIS  937                 0056.09",
            "22/07/22 18:51 - 028 MART RIKOV 4318                0255.04",
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

            Console.WriteLine($"PIUSI Emulator Initialized. {port.PortName}, Baudrate: {GetBaudrate()}, Tx interval: {GetTxInterval()} ms");

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

            Console.WriteLine($"PIUSI Emulator Send: {msg[MsgIndex]}");

            byte[] data_to_send = ASCIIEncoding.ASCII.GetBytes(msg[MsgIndex] + "\r");
            Port.Write(data_to_send, 0, data_to_send.Length);
            MsgIndex++;
            MsgIndex = (uint)(MsgIndex % msg.Count());

            return true;
        }

        private long GetTimestamp()
        {
            return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
        }

        private int GetBaudrate()
        {
            return 2400;
        }

        private UInt32 GetTxInterval()
        {
            return 10000;
        }

        public string GetMenuString()
        {
            return $"Piusi protocol, sending messages with {GetTxInterval()} ms intervals @ {GetBaudrate()} baud";
        }

        public void DeInit()
        {
            this.Port.Close();
        }
    }
}
