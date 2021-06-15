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
    class EuroscanEmulator : ISerialEmulator
    {
        private SerialPort Port;

        public void Init(SerialPort port)
        {
            this.Port = port;
            this.Port.StopBits = StopBits.One;
            this.Port.BaudRate = GetBaudrate();
            this.Port.Open();

            Console.WriteLine($"Euroscan Emulator Initialized. {port.PortName}, Baudrate: {GetBaudrate()}");
        }

        public void Trigger()
        {
            if (!Port.IsOpen)
            {
                Console.WriteLine("PORT Closed");
                Thread.Sleep(500);
                return;
            }

            WaitRequest();

        }

        bool WaitRequest()
        {
            TimeSpan maxDuration = TimeSpan.FromMilliseconds(100);
            Stopwatch sw = Stopwatch.StartNew();

            if (Port.BytesToRead == 0)
            {
                return false;
            }

            // ja ir dati tad pagaidam 100 ms, lai iesūta visus paku
            while (sw.Elapsed < maxDuration) ;

            int bytes = Port.BytesToRead;
            byte[] buffer = new byte[bytes];
            int bytes_read = Port.Read(buffer, 0, bytes);
                                
            string rawAsciiMsg = System.Text.Encoding.ASCII.GetString(buffer, 0, bytes_read);

            return HandleRequest(rawAsciiMsg);
        }

        private bool HandleRequest(string RAWCommand)
        {
            string response = GenerateResponse(RAWCommand);

            if (response == null) {
                return false;
            }

            return SendResponse(response);
        }

        protected virtual string GenerateResponse(string command)
        {
            switch (command)
            {
                case "?\r":
                    //  fw version
                    return "TMS V1.63\r";
                case "i\r":
                    // serial nr on tk
                    return "serial-nr\r";

                case "M1\r":
                    return "M1 -1.8,1111\r";
                case "M2\r":
                    return "M2 -2.8,1111\r";
                case "M3\r":
                    return "M3 -3.8,1111\r";
                case "M4\r":
                    return "M4 -4.8,1111\r";
                case "M5\r":
                    return "M5 -5.8,1111\r";
                case "M6\r":
                    // input not enabled (temp = 0.00)
                    return "M6 0.00,1111\r";

                default:
                    Console.WriteLine($"unknown request: {command}");
                    return null;
            }
        }

        bool SendResponse(string response)
        {
            if (!Port.IsOpen)
            {
                return false;
            }

            byte[] data = ASCIIEncoding.ASCII.GetBytes(response);

            Port.Write(data, 0, data.Count());

            Console.WriteLine($"data sent: '{response.Replace("\n", "\\n")}', size: {data.Count()}");

            return true;
        }

        private int GetBaudrate()
        {
            return 9600;
        }
    }
}
