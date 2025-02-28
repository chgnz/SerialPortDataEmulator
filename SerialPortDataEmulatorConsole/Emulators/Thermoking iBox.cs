using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SerialPortDataEmulatorConsole.SerialProtocols
{
    class iBoxEmulator : ISerialEmulator
    {
        private SerialPort Port;

        public void Init(SerialPort port)
        {
            this.Port = port;
            this.Port.StopBits = StopBits.One;
            this.Port.BaudRate = GetBaudrate();
            this.Port.Open();

            Console.WriteLine($"iBoxEmulator Emulator Initialized. {port.PortName}, Baudrate: {GetBaudrate()}");
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

            return HandleRequest(buffer);
        }

        private bool HandleRequest(byte[] RAWCommand)
        {
            byte[] response = GenerateResponse(RAWCommand);

            if (response == null)
            {
                return false;
            }

            return SendResponse(response);
        }

        protected virtual byte[] GenerateResponse(byte[] command)
        {
            // Multiple predefined command buffe    rsa]
            byte[] command_pid_243_id =                 { 0x93, 0x00, 0xf3, 0x7a };
            byte[] command_pid_203_cargowatch =         { 0x93, 0x00, 0xcb, 0xa2 };
            byte[] command_pid_208_rpm =                { 0x93, 0xd0, 0x80, 0xbc, 0x65, 0xc9, 0x33 }; 
            byte[] command_pid_208_controller_type =    { 0x93, 0xd0, 0x00, 0x1f, 0x15, 0xf6, 0x73 }; // controller type  0x1F15F6

            byte[] command_pid_96_fuel =                { 0x93, 0x00, 0x60, 0x0d };
            byte[] command_pid_200_zone1 =              { 0x93, 0x00, 0xc8, 0xa5 };

            Console.WriteLine($"GenerateResponse {BitConverter.ToString(command)} ");

            Console.WriteLine($"");
            // reefer id request
            if (command.SequenceEqual(command_pid_243_id))
            {
                Console.WriteLine($"command_pid_243_id");
                Console.WriteLine($"");
                return new byte[] { 0x93, 0xf3, 0x1c, 0x93, 0x54, 0x4b, 0x49, 0x4e, 0x47, 0x2a, 0x69, 0x42, 0x4f, 0x58, 0x2a, 0x41, 0x36, 0x31, 0x32, 0x44, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x2a };
            }

            if (command.SequenceEqual(command_pid_203_cargowatch))
            {
                Console.WriteLine($"command_pid_203_cargowatch");
                Console.WriteLine($"");
                return new byte[] { 0x93, 0xcb, 0xff, 0xc0, 0xff, 0x8f, 0xff, 0x6f, 0xe7 };
            }

            if (command.SequenceEqual(command_pid_208_rpm))
            {
                Console.WriteLine($"command_pid_208_rpm");
                return new byte[] { 0x93, 0xd0, 0x05, 0xbb, 0xdd };
            }

            if (command.SequenceEqual(command_pid_208_controller_type))
            {
                Console.WriteLine($"command_pid_208_controller_type");
                return new byte[] { 0x93, 0xd0, 0x00, 0x0e, 0x8f };
            }

            if (command.SequenceEqual(command_pid_200_zone1))
            {
                Console.WriteLine($"command_pid_200_zone1");
                return new byte[] { 0x93, 0xc8, 0x11, 0x91, 0x02, 0x00, 0x01 };
            }
            
            return new byte[] { 0x05 } ;
        }

        bool SendResponse(byte[] response)
        {
            if (!Port.IsOpen)
            {
                return false;
            }

            //byte[] data = ASCIIEncoding.ASCII.GetBytes(response);

            Port.Write(response, 0, response.Count());

            Console.WriteLine($"data sent: '{BitConverter.ToString(response)}', size: {response.Count()}");

            return true;
        }

        private int GetBaudrate()
        {
            return 9600;
        }

        public string GetMenuString()
        {
            return "iBox (Request-Response ASCII protocol @ baudrate 9600)";
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