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
    class AxtecEmulator : ISerialEmulator
    {
        private SerialPort Port;
        private long Timestamp;
        private UInt32 TxInterval = 2000 ;

        public void Init(SerialPort port)
        {
            this.Port = port;
            this.Port.StopBits = StopBits.Two;
            this.Port.BaudRate = GetBaudrate();
            this.Port.Open();

            Console.WriteLine($"Axtec Emulator Initialized. {port.PortName}, Baudrate: {GetBaudrate()}");
        }

        public void Trigger()
        {
            if (!Port.IsOpen)
            {
                Console.WriteLine("PORT Closed");
                Thread.Sleep(500);
                return;
            }

            if (this.TxInterval == 0 ||
                GetTimestamp() - this.Timestamp < this.TxInterval)
            {
                // nav pienācies laiks sūtīt
                return;
            }

            this.Send();

            this.Timestamp = GetTimestamp();
        }

        static int netw = 1;

        bool Send()
        {
            int net_weigth = (ushort)(new Random().Next(0, 20000) & 0xffff);
            int gross_weigth = net_weigth + 2;
            int axle1_weight = -1111;
            int axle2_weight = 2222;
            int axle3_weight = -3333;
            int axle4_weight = 4444;

            net_weigth = netw++;

            string format = "+00000;-00000";
            string net_weigth_str = net_weigth.ToString(format);
            string gross_weigth_str = gross_weigth.ToString(format);
            string axle1_weigth_str = axle1_weight.ToString(format);
            string axle2_weigth_str = axle2_weight.ToString(format);
            string axle3_weigth_str = axle3_weight.ToString(format);
            string axle4_weigth_str = axle4_weight.ToString(format);
            //string alarm_flags = "G-123-";
            string alarm_flags = "------";

            string data = $"3={net_weigth_str},4={gross_weigth_str},5={axle1_weigth_str},{axle2_weigth_str},{axle3_weigth_str},{axle4_weigth_str},6=01250,00880,00000,00000,7=01750,02150,00000,00000,03500,8= ,10={alarm_flags},";

            // add cheksum
            int cheksum = calcuate_checksum(data);
            data = $"{data}{cheksum.ToString("X4")}";

            byte[] tx_data = ASCIIEncoding.ASCII.GetBytes($"{data}\n\r");

            Port.Write(tx_data, 0, tx_data.Count());

            Console.WriteLine($"data sent: '{data.Trim()}', size: {data.Count()}");

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

        public string GetMenuString()
        {
            return "Axtec (Autosending mode with 10sec period @ baudrate 115200 (Stopbits=2))";
        }

        public int calcuate_checksum(string message)
        {
            int checksum = 0;
            for (int i = 0; i < message.Length; i++)
            {
                int v = message[i];
                checksum += v;
            }

            checksum = checksum & 0xffff;
            return checksum;
        }

        public void DeInit()
        {
            this.Port.Close();
        }
    }
}
