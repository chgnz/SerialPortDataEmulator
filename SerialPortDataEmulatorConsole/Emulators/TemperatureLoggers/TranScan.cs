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
    class TranScanEmulator : ISerialEmulator
    {
        private SerialPort Port;

        public void Init(SerialPort port)
        {
            this.Port = port;
            this.Port.StopBits = StopBits.One;
            this.Port.BaudRate = GetBaudrate();
            this.Port.Open();

            Console.WriteLine($"TranScan 2 ADR Emulator Initialized. {port.PortName}, Baudrate: {GetBaudrate()}");
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
                case "i":
                case "I":
                    return $"◄{command} T19046\r";
                case "a":
                case "A":
                    return $"◄{command} 0000,0,0,0,0,0000\r";
                case "f":
                case "F":
                    return $"{command} 0158\r";
                case "h":
                case "H":
                    // historical temperature values?		
                    return $"{command} 0042\r";
                case "?":
                    return $"TS2-T700\r";
                case "#":
                    return $" 7932\r";
                case "x":
                case "X":
                    return $"{command} 0158\r";
                case "m":
                case "M":
                    return $"{command} 0000,0000\r";

                case "f1":
                    return $"{command} f 0001 2022-01-26,19:55:00,T19046,4,4,2,1,8,T700,0,359\r";
                case "F1":
                    return $"{command} F 0001 2022-01-26,19:55:00,T19046,4,4,2,1,8,T700,0,359\r";
                case "f2":
                    return $"{command} f 0002 2022-01-26,18:45:00,T19046,4,4,2,1,132,T700,0,358\r";
                case "F2":
                    return $"{command} F 0002 2022-01-26,18:45:00,T19046,4,4,2,1,132,T700,0,358\r";

                case "h1":
                    return $"{command} h 0001 2017-09-06,13:17:00,T19046,-18.4,-50.0,0000,00\r";
                case "H1":
                    return $"{command} H 0001 2017-09-06,13:17:00,T19046,-18.4,-50.0,0000,00\r";
                case "h2":
                    return $"{command} h 0002 2017-09-06,13:15:00,T19046,-18.4,-50.0,0000,00\r";
                case "H2":
                    return $"{command} H 0002 2017-09-06,13:15:00,T19046,-18.4,-50.0,0000,00\r";

                case "M0":
                case "m0":
                case "M 0":
                case "m 0":
                    return $"{command} 9.5,-5.4,-50.0,-50.0,0000,0000\r";
                case "m1":
                case "M1":
                    return $"{command} 9.3,0000,0000\r";
                case "m2":
                case "M2":
                    return $"{command} -12.8,0000,0000\r";
                case "m3":
                case "M3":
                    return $"{command} -50.0,0000,0000\r";
                case "m4":
                case "M4":
                    return $"{command} -50.0,0000,0000\r";
                case "m5":
                case "M5":
                    return $"#2\r";
                case "m6":
                case "M6":
                    return $"#2\r";

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
            return "TranScan 2 ADR  (Request-Response ASCII protocol @ baudrate 9600)";
        }

        public void DeInit()
        {
            this.Port.Close();
        }
    }
}
