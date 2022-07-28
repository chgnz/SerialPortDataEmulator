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
    class ThermokingTouchprintModbusEmulator : ISerialEmulator
    {
        private SerialPort Port;

        public void Init(SerialPort port)
        {
            this.Port = port;
            this.Port.StopBits = StopBits.One;
            this.Port.BaudRate = GetBaudrate();
            this.Port.Open();

            Console.WriteLine($"Thermoking Touchprint Modbus Emulator Initialized. {port.PortName}, Baudrate: {GetBaudrate()}");
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

            Console.WriteLine(BitConverter.ToString(buffer));
            return HandleRequest(buffer);
        }

        private bool HandleRequest(byte[] command)
        {
            const int MIN_MODBUS_MSG_SIZE = 6;

            if (command.Length < MIN_MODBUS_MSG_SIZE)
            {
                Console.WriteLine($"invalid message size: {command.Length}");
                return false;
            }

            byte address = command[0];

            if (address != 0x01 && address != 0x2b)
            {
                Console.WriteLine($"unknown device address: 0x{address:x}");
                return false;
            }

            byte function = command[1];

            if (function != 0x03)
            {
                Console.WriteLine($"Error, Only read register function suported(0x03), Received function: 0x{function:x}");
                return false;
            }

            //int payload_size = command[3] << 8 | command[2];
            //int required_data_size = payload_size + 4;
            //if (required_data_size > command.Length)
            //{
            //    Console.WriteLine($"Not enough data received: {command.Length}, required {required_data_size}");
            //    return false;
            //}

            int first_register_address = command[2] << 8 | command[3];
            int register_count = command[4] << 8 | command[5];

            int crc = command[6] << 8 | command[7];

            Console.WriteLine($"first_register_address: {first_register_address}, register count {register_count}");

            if (!validateCrc(command))
            {
                return false; 
            }


            return true;
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

        protected virtual string GenerateResponse(byte[] command)
        {
            return null;
        }

        private int GetBaudrate()
        {
            return 9600;
        }

        public string GetMenuString()
        {
            return "Thermoking Touchprint (Request-Response MODBUS protocol @ baudrate 9600)";
        }

        private bool validateCrc(byte[] data_packet)
        {
            int crc_received = data_packet[7] << 8 | data_packet[6];
            int crc = 0xffff;

            for (int i = 0; i < 6; i++)
            {
                crc ^= data_packet[i];
                for (byte bit_num = 0; bit_num < 8; ++bit_num)
                {
                    crc = (crc & 1) > 0 ? (crc >> 1) ^ 0xa001 : (crc >> 1);
                }
            }

            //Console.WriteLine($"crc: {crc:x}, crc_received: {crc_received:x}");

            return crc_received == crc;

        }
    }
}
