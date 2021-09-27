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
    class ThermokingTouchprintASCIIEmulator : ISerialEmulator
    {
        private SerialPort Port;

        public void Init(SerialPort port)
        {
            this.Port = port;
            this.Port.StopBits = StopBits.One;
            this.Port.BaudRate = GetBaudrate();
            this.Port.Open();

            Console.WriteLine($"Thermoking Touchprint ASCII Emulator Initialized. {port.PortName}, Baudrate: {GetBaudrate()}");
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
                case "i\r":
                    // serial nr on tk
                    return "serial-nr\r";

                case "m1\r":
                    return "m1 -1.1,1111\r";
                case "m2\r":
                    return "m2 -2.2,1111\r";
                case "m3\r":
                    return "m3 -3.3,1111\r";
                case "m4\r":
                    return "m4 -4.4,1111\r";
                case "m5\r":
                    return "m5 -5.5,1111\r";
                case "m6\r":
                    return "m6 -6.6,1111\r";

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

        public string GetMenuString()
        {
            return "Thermoking Touchprint (Request-Response ASCII protocol @ baudrate 9600)";
        }
    }
}
