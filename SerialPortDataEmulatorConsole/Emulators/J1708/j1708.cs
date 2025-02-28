using SerialPortDataEmulatorConsole.Emulators.J1708;
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
    public class J1708Emulator : ISerialEmulator
    {
        private SerialPort Port;

        private int TxInterval = 100;

        private long Timestamp;

        public void Init(SerialPort port)
        {
            this.Port = port;
            this.Port.StopBits = StopBits.One;
            this.Port.BaudRate = GetBaudrate();
            this.Port.Open();
            Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

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

            if (this.TxInterval == 0 ||
                GetTimestamp() - this.Timestamp < this.TxInterval)
            {
                // nav pienācies laiks sūtīt
                return;
            }

            SendData();

            this.Timestamp = GetTimestamp();

        }
        private long GetTimestamp()
        {
            return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
        }

        static byte i = 209;

        enum ECU_PID
        {
            speed = 0x54,
            accelerator = 0x5b,
            engine_load = 0x5c,
            fuel_level = 0x60,
            engine_temperature = 0x6e,
            cruise = 85,

            // uint16_t
            //fuel_rate = 183, //Fuel Rate (Instantaneous)
            instantaneous_fuel_economy = 184, //instantaneous_fuel_economy,  km/l??
            engine_speed_rpm = 190,  //

            total_distance = 245,  //
            total_engine_hours = 247,  //
        }
        private byte[] AddByteToArray(byte[] bArray, byte[] appendArray)
        {
            byte[] newArray = new byte[bArray.Length + appendArray.Length];
            bArray.CopyTo(newArray, 0);
            appendArray.CopyTo(newArray, bArray.Length);
            return newArray;
        }

        static byte offset = 0;
        byte MID = 128;
        void SendData()
        {
            Factory f = new Factory();

            //const byte MID = 0x80;  // (128) engine ECU
            //const byte MID = 144; // cruise control
            //byte[] msg;
            //msg = new byte[] { MID };
            //msg = AddByteToArray(msg, f.createAcceleratorPedalPosition().SetValue(55).GetRawData());
            //msg = AddByteToArray(msg, f.createEngineLoad().SetValue(7).GetRawData());
            //msg = AddByteToArray(msg, f.createFuelLevel().SetValue(8).GetRawData());
            //msg = AddByteToArray(msg, f.createRoadSpeed().SetValue(25).GetRawData());
            //msg = AddByteToArray(msg, f.createRPM().SetValue(1029).GetRawData());
            //msg = AddByteToArray(msg, f.createCruiseStatus().SetValue(0xff).GetRawData());

            //byte crc = this.j1708_checksum(msg);
            //this.Port.Write(msg, 0, msg.Length);
            //this.Port.Write(new byte[] { crc }, 0, 1);

            //Console.WriteLine($"tx: {BitConverter.ToString(msg)}-{BitConverter.ToString(new byte[] { crc })}"); ;

            //offset++;

            //if (offset > 5)
            //{ offset = 0; }

            byte PID = (byte)(247);

            //var param_msg = new byte[] { MID, offset, 0xff };
            var param_msg = new byte[] { MID, 86, 0xff };
            byte crc = this.j1708_checksum(param_msg);

            this.Port.Write(param_msg, 0, param_msg.Length);
            this.Port.Write(new byte[] { crc }, 0, 1);

            Console.WriteLine($"tx: {BitConverter.ToString(param_msg)}-{BitConverter.ToString(new byte[] { crc })}"); ;

            this.Timestamp = GetTimestamp();
            MID++;
            //offset++;
            //if (offset++ >= 128)
            //{
            //    offset = 0;
            //}
        }

        private bool HandleRequest(string RAWCommand)
        {
            // no requests
            return true;
        }

        private int GetBaudrate()
        {
            return 9600;
        }

        public string GetMenuString()
        {
            return "J1708 emulator (binary protocol @ baudrate 9600)";
        }

        public byte j1708_checksum(byte[] message)
        {
            int checksum = 0;
            for (int i = 0; i < message.Length; i++)
            {
                checksum += message[i];
            }

            checksum = checksum & 0xff;
            checksum = 0x100 - checksum;

            return (byte)(checksum & 0xff);
        }

        public void DeInit()
        {
            this.Port.Close();
        }
    }
}
