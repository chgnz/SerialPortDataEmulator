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
    class APM303ModbusEmulator : ISerialEmulator
    {
        private SerialPort Port;

        public void Init(SerialPort port)
        {
            this.Port = port;
            this.Port.StopBits = StopBits.One;
            this.Port.BaudRate = GetBaudrate();
            this.Port.Open();

            Console.WriteLine($"APM303 Modbus Emulator Initialized. {port.PortName}, Baudrate: {GetBaudrate()}");
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
                Thread.Sleep(100);
                return false;
            }

            // ja ir dati tad pagaidam 100 ms, lai iesūta visus paku
            while (sw.Elapsed < maxDuration)
            {
                Thread.Sleep(1);
            };

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

            if (address != 0x05)
            {
                Console.WriteLine($"unknown device address: 0x{address:x}");
                return false;
            }

            byte function = command[1];

            if (function != 0x04)
            {
                Console.WriteLine($"Error, Only read register function suported(0x03), Received function: 0x{function:x}");
                return false;
            }

            int first_register_address = command[2] << 8 | command[3];
            int register_count = command[4] << 8 | command[5];

            int crc = command[7] << 8 | command[6];

            Console.WriteLine($"first_register_address: {first_register_address:X}, register count {register_count}");

            
            if (!ModbusCRC.IsValidCRC(command))
            {
                Console.WriteLine($"invalid crc");
                return false; 
            }

            var msg = new byte[] { 0x05, 0x04, 0x02, 0x00, 0x00 };

            const bool high_prcision = true;

            const int apm303_registers_live_1_neutral_voltage = 0x0000;
            const int apm303_registers_live_2_neutral_voltage = 0x0001;
            const int apm303_registers_live_3_neutral_voltage = 0x0002;
            const int apm303_registers_engine_speed_rpm = 0x0016;

            const int apm303_registers_live_1_current = 0x0006;
            const int apm303_registers_live_2_current = 0x0007;
            const int apm303_registers_live_3_current = 0x0008;
            const int apm303_registers_reading_precision = 0x0018;

            ushort value;
            switch (first_register_address)
            {
                case apm303_registers_live_1_neutral_voltage:
                    value = 200;
                    break;
                case apm303_registers_live_2_neutral_voltage:
                    value = 202;
                    break;
                case apm303_registers_live_3_neutral_voltage:
                    value = 204;
                    break;
                case apm303_registers_engine_speed_rpm:
                    value = 239;
                    break;

                case apm303_registers_live_1_current:
                    value = 10;
                    if (high_prcision) value *= 10;
                    break;
                case apm303_registers_live_2_current:
                    value = 13;
                    if (high_prcision) value *= 10;
                    break;
                case apm303_registers_live_3_current:
                    value = 17;
                    if (high_prcision) value *= 10;
                    break;

                case apm303_registers_reading_precision:
                    value = high_prcision ? 1 : 0; 
                    break;

                default:
                    value = (ushort)(new Random().Next(0, 0xffff) & 0xffff);
                    break;
            }

            msg[3] = (byte)((value << 8) & 0xff);
            msg[4] = (byte)(value & 0xff);

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

            Console.WriteLine($"data sent: '{BitConverter.ToString(data).Replace("-","")}', size: {data.Count()}");

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
            return "APM303 Generator (Request-Response MODBUS protocol @ baudrate 9600)";
        }

        public void DeInit()
        {
            this.Port.Close();
        }
    }
}
