using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialPortDataEmulatorConsole.SerialProtocols
{
    public class DUTEmulator : ISerialEmulator
    {
        protected int value;
        protected bool enableValueOverride;

        private SerialPort Port;
        protected DutMessage DutRxMsg = new DutMessage();

        public void Init(SerialPort port)
        {
            this.Port = port;
            this.Port.StopBits = StopBits.One;
            this.Port.BaudRate = GetBaudrate();
            this.Port.Open();

            Console.WriteLine($"DUT FuelSensor Emulator Initialized. {port.PortName}, Baudrate: {GetBaudrate()}");
        }
        public void Trigger()
        {
            if (!Port.IsOpen)
            {
                return;
            }

            WaitRequest();

        }

        bool WaitRequest()
        {
            const int MIN_DUT_PACKET_SIZE = 4;

            if (Port.BytesToRead < MIN_DUT_PACKET_SIZE)
            {
                return false;
            }

            TimeSpan maxDuration = TimeSpan.FromMilliseconds(100);
            Stopwatch sw = Stopwatch.StartNew();

            while (Port.BytesToRead > 0 && sw.Elapsed < maxDuration)
            {
                byte rx_byte = (byte)Port.ReadByte();

                if (ReceiveDutMessage(rx_byte))
                {
                    var Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                    Console.WriteLine($"{Timestamp} - Dut Message received: command: 0x{DutRxMsg.Data.Command:x}, address: 0x{DutRxMsg.Data.Address:x}");
                    DutRxMsg.Reset();
					HandleRequest(DutRxMsg);

                    return true;
                }
            }

            return false;
        }

        private void HandleRequest(DutMessage request)
        {
            var data_without_crc = GenerateResponseWithoutCRC(request.Data.Command, request.Data.Address);

            if (request.Data.Address > 101)
            {
                //return;
            }

            if (data_without_crc == null)
            {
                return;
            }

            // calculate crc
            var CRC_Calculator = new DallasCRC();
            byte crc = CRC_Calculator.Begin();

            for (int i = 0; i < data_without_crc.Length; i++)
            {
                crc = CRC_Calculator.Update(crc, data_without_crc[i]);
            }
            
            byte[] response = data_without_crc.Concat(new byte[] { crc }).ToArray();
            SendResponse(response);

            Console.WriteLine($"sent {response.Length} bytes {BitConverter.ToString(response)}");
        }

        static byte counter = 0;
        protected virtual byte[] GenerateResponseWithoutCRC(byte command, byte address)
        {
            switch (command)
            {
                case 0x02:
                    // serial number (DUT-E)
                    return new byte[] { 0x3e, address, 0x02, 0x01, 0x02, 0x03, 0x04 };

                case 0x06:
                    // filtered params
                    return new byte[] { 0x3e, address, 0x06, 0x01, 0x01, 0x01, counter, 0x01 };

                case 0x1c:
                    // FW Version (DUT-E)
                    return new byte[] { 0x3e, address, 0x1c, 0x01, 0x02, 0x03 };

                case 0x23:
                    // work parames (DUT-E)
                    return new byte[] { 0x3e, address, 0x23, counter++, 0x00, 0x03, 0x04, 0x01, 0x02, 0x03, 0x04, 0x01, 0x02, 0x03, 0x04, 0x01, 0x02, 0x03, 0x04, 0x01, 0x02, 0x03, 0x04, 0x01, 0x02, 0x03, 0x04, 0x01, 0x02, 0x03, 0x04, 0x01, 0x02, 0x03, 0x04, 0x01, 0x02, 0x03, 0x04, 0x01, 0x02, 0x03, 0x04, 0x01, 0x02, 0x03, 0x04, 0x01, 0x02, 0x03, 0x04 };
           
                default:
                    return null;
            }
        }

        bool SendResponse(byte[] data)
        {
            if (!Port.IsOpen)
            {
                return false;
            }

            Port.Write(data, 0, data.Count());

            return true;
        }

        virtual protected int GetBaudrate()
        {
            return 19200;
        }

        private bool ReceiveDutMessage(byte rx_byte)
        {
            if (!DutRxMsg.State.HeaderReceived)
            {
                DutRxMsg.Data.Header = rx_byte;

                if (DutRxMsg.Data.Header != 0x31 &&
                    DutRxMsg.Data.Header != 0x3e)
                {
                    return false;
                }

                DutRxMsg.State.HeaderReceived = true;
                return false;
            }

            if (!DutRxMsg.State.AddressReceived)
            {
                DutRxMsg.Data.Address = rx_byte;
                DutRxMsg.State.AddressReceived = true;
                return false;
            }

            if (!DutRxMsg.State.CommandReceived)
            {
                DutRxMsg.Data.Command = rx_byte;
                DutRxMsg.Data.Size = DutRxMsg.GetPayloadSize();

                DutRxMsg.State.CommandReceived = true;
                return false;
            }

            if (!DutRxMsg.State.DataReceived && DutRxMsg.Data.Size > 0)
            {
                if (DutRxMsg.Data.DataBytesReceived > DutRxMsg.Data.Payload.Length)
                {
                    Console.WriteLine("pa daudz datu!!");
                    DutRxMsg.Reset();
                    return false;
                }

                // vācam datus
                DutRxMsg.Data.Payload[DutRxMsg.Data.DataBytesReceived++] = rx_byte;

                if (DutRxMsg.Data.DataBytesReceived == DutRxMsg.Data.Size)
                {
                    DutRxMsg.State.DataReceived = true;
                }

                return false;
            }

            if (!DutRxMsg.State.CrcReceived)
            {
                DutRxMsg.Data.Crc = rx_byte;
                DutRxMsg.State.CrcReceived = true;

                var CRC_Calculator = new DallasCRC();
                byte crc = CRC_Calculator.Begin();
                crc = CRC_Calculator.Update(crc, DutRxMsg.Data.Header);
                crc = CRC_Calculator.Update(crc, DutRxMsg.Data.Address);
                crc = CRC_Calculator.Update(crc, DutRxMsg.Data.Command);

                for (int i = 0; i < DutRxMsg.Data.Size; i++)
                {
                    crc = CRC_Calculator.Update(crc, DutRxMsg.Data.Payload[i]);
                }

                if (crc != DutRxMsg.Data.Crc)
                {
                    DutRxMsg.Reset();
                }
                return true;
            }

            return false;
        }

        virtual public string GetMenuString()
        {
            return "DUT FuelSensor (Request-Response protocol @ baudrate 19200), implemented commands: 0x02, 0x06, 0x1c, 0x23";
        }

        public void DeInit()
        {
            this.Port.Close();
        }
    }

    public class EpsilonESEmulator : DUTEmulator, IFuelSensorEmulator
    {

        override protected byte[] GenerateResponseWithoutCRC(byte command, byte address)
        {
            switch (command)
            {
                case 0x06:
                    // filtered params

                    short test_value = this.enableValueOverride ? (short)(this.value & 0xffff) : (short)new Random().Next(1, 4096);
                    Console.WriteLine($"sending value: {(short)(test_value)}, {this.enableValueOverride}");

                    return new byte[] { 0x3e, address, 0x06, 0x08, (byte)(test_value & 0xff), (byte)(test_value >> 8), 0x01, 0x01 };

                default:
                    return null;
            }
        }

        override public string GetMenuString()
        {
            return "Epsilon ES FuelSensor (Request-Response protocol @ baudrate 19200), implemented commands: 0x06";
        }

        public void SetFixedFuelValue(int value)
        {
            this.value = value;
        }

        public void EnableFixedValueMode()
        {
            this.enableValueOverride = true;
        }

        public void EnableRandomValueMode()
        {
            this.enableValueOverride = false;
        }
    }

    public class TechnotonFlowMeter : DUTEmulator
    {
        override protected int GetBaudrate()
        {
            return 9600;
        }

        override public string GetMenuString()
        {
            return "Technoton flow meter (Request-Response protocol @ baudrate 57600)";
        }
    }
}