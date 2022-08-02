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

            if (!ModbusCRC.IsValidCRC(command))
            {
                return false; 
            }

            var msg = new byte[] { 0x01, 0x03, 0x0c, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            ushort value;
            switch (first_register_address)
            {
                default:
                    value = (ushort)(new Random().Next(0, 0xffff) & 0xffff);
                    break;
            }

            msg[3] = (byte)((value << 8) & 0xff);
            msg[4] = (byte)(value & 0xff);
            msg[5] = (byte)((value << 8) & 0xff);
            msg[6] = (byte)(value & 0xff);
            msg[7] = (byte)((value << 8) & 0xff);
            msg[8] = (byte)(value & 0xff);
            msg[9] = (byte)((value << 8) & 0xff);
            msg[10] = (byte)(value & 0xff);
            msg[11] = (byte)((value << 8) & 0xff);
            msg[12] = (byte)(value & 0xff);
            msg[13] = (byte)((value << 8) & 0xff);
            msg[14] = (byte)(value & 0xff);

            int crc_response = ModbusCRC.CalculateCRC(msg);
            var crc_modbus = new byte[] { (byte)(crc_response & 0xff), (byte)((crc_response >> 8) & 0xff) };

            this.SendResponse(msg);
            this.SendResponse(crc_modbus);

            return true;
        }

        bool SendResponse(byte[] data)
        {
            if (!Port.IsOpen)
            {
                return false;
            }

            Port.Write(data, 0, data.Count());

            Console.WriteLine($"data sent: '{BitConverter.ToString(data).Replace("-", "")}', size: {data.Count()}");

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

        private bool IsCRCValid(byte[] data_packet)
        {
            return ModbusCRC.IsValidCRC(data_packet);
        }
    }
}
