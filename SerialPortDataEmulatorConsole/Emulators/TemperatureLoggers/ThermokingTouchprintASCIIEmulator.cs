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
            command = command.TrimEnd();

            switch (command)
            {
                /*
                    203031  00:27:04.455    SER(serial2): ascii message sent: 'm0 '    2022 - 03 - 04 15:50:06
                    203031  00:27:04.725    SER(serial2): ascii message received: 'm0 10.4,10.3,0.0,0.0,0000,00 '  2022 - 03 - 04 15:50:06
                    203031  00:27:04.735    SER(serial2): ascii message sent: 'M0 '    2022 - 03 - 04 15:50:06
                    203031  00:27:05.15     SER(serial2): ascii message received: 'M0 10.4,10.3,0.0,0.0,0000,00 '  2022 - 03 - 04 15:50:07
                    203031  00:27:05.25     SER(serial2): ascii message sent: '? '     2022 - 03 - 04 15:50:07
                    203031  00:27:05.654    SER(serial2): ascii message received: 'TS2-T410 '  2022 - 03 - 04 15:50:07
                    203031  00:27:05.664    SER(serial2): ascii message sent: 'i '     2022 - 03 - 04 15:50:07
                    203031  00:27:06.312    SER(serial2): ascii message received: 'i T06315 '  2022 - 03 - 04 15:50:08
                */
                case "i":
                    return $"{command} T06315\r";

                case "?":
                    return "TS2-T410\r";

                case "m0":
                case "M0":
                    //return $"{command} 10.4,10.3,0.0,0.0,0000,00\r";
                    return $"{command} 10.{new Random().Next(0, 9)},10.{new Random().Next(0, 9)},0.0,0.0,0000,00\r";

                case "m1":
                case "M1":
                    // return $"{command} 14.0,0000,00\r";
                    return $"{command} 14.{new Random().Next(0,9)},0000,00\r";
                case "m2":
                case "M2":
                    return $"{command} 13.8,0000,00\r";
                case "m3":
                case "M3":
                    return $"{command} 2.0,0000,00\r";
                case "m4":
                case "M4":
                    return $"{command} 0.0,0000,00\r";
                case "m5":
                case "M5":
                    return "#2\r";
                case "m6":
                case "M6":
                    return "#2\r";

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
            return "Thermoking Touchprint (Request-Response ASCII protocol @ baudrate 9600)";
        }
    }
}
