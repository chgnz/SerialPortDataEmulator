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
        protected 
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
            command = command.TrimEnd();

            switch (command)
            {
                case "?":
                    //  fw version
                    return "TMS V1.63\r";

                case "M0":
                    // all inputs
                    return $"M0  22.{GetDigit()}, 22.{GetDigit()},  5.{GetDigit()}, 20.{GetDigit()},  0.0,  0.0,0000\r";

                case "M1":
                    return "M1  19.4,0000\r";
                case "M2":
                    return "M2   3.5,0000\r";
                case "M3":
                    return "M3   1.0,0000\r";
                case "M4":
                    return "M4   3.0,0000\r";

                // input not enabled (temp = 0.00)
                case "M5":
                    return "M5   0.0,0000\r";
                case "M6":
                    return "M6   0.0,0000\r";

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

            Console.WriteLine($"data sent: '{response.Trim()}', size: {data.Count()}");

            return true;
        }

        private int GetBaudrate()
        {
            return 9600;
        }

        public string GetMenuString()
        {
            return "Euroscan MX Series (Request-Response ASCII protocol @ baudrate 9600)";
        }

        private static string GetDigit()
        {
            return new Random().Next(0, 9).ToString();
        }

        public void DeInit()
        {
            this.Port.Close();
        }
    }
}
