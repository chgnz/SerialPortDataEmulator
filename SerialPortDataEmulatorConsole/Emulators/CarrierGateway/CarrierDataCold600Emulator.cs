using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialPortDataEmulatorConsole.SerialProtocols
{
    class CarrierDataCold600Emulator : ISerialEmulator
    {
        private SerialPort Port;
        private CarrierGatewayMessage RxMsg = new CarrierGatewayMessage();
        
        public void Init(SerialPort port)
        {
            this.Port = port;
            this.Port.StopBits = StopBits.One;
            this.Port.BaudRate = GetBaudrate();
            this.Port.Open();

            Console.WriteLine($"CarrierDataCold600Emulator Emulator Initialized. {port.PortName}, Baudrate: {GetBaudrate()}");
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
            const int MIN_PACKET_SIZE = 5;

            if (Port.BytesToRead < MIN_PACKET_SIZE)
            {
                return false;
            }

            TimeSpan maxDuration = TimeSpan.FromMilliseconds(100);
            Stopwatch sw = Stopwatch.StartNew();

            while (RxMsg.GetRequiredSize() > 0 && 
                    Port.BytesToRead >= RxMsg.GetRequiredSize() && 
                    sw.Elapsed < maxDuration)
            {
                byte[] buffer = new byte[RxMsg.GetRequiredSize()];
                Port.Read(buffer, 0, RxMsg.GetRequiredSize());
                
                if (RxMsg.Receive(buffer))
                {
                    Console.WriteLine($"Request Received: 0x{RxMsg.Data.Command:x}");
                    HandleRequest();
                    RxMsg.Reset();
                    return true;
                }
            }

            Console.WriteLine($"timeout, received {Port.BytesToRead} bytes ");

            return false;
        }

        private void HandleRequest()
        {
            var data_without_crc = GenerateResponseWithoutCRC(RxMsg.Data.Command);

            if (data_without_crc == null)
            {
                return;
            }

            // calculate crc
            byte crc = 0;

            for (int i = 0; i < data_without_crc.Length; i++)
            {
                crc += data_without_crc[i];
            }

            Console.Write("Sending Response: ");
            SendResponse(data_without_crc);
            SendResponse(new byte[] { crc });
            Console.WriteLine();
        }

        protected virtual byte[] GenerateResponseWithoutCRC(byte command)
        {
            switch (command)
            {
                case 0x13:
                    //return new byte[] {
                    // 2 sensors
                    //   0x99, 0x93 , 0x0a , 0x00 , 0x01, 0x01, 0x03 , 0xaa, 0x00 , 0x02, 0x01, 0x03 , 0xaa, 0x00 // crc 0x95
                    //};
                    //return new byte[] {
                    // 4 sensors 
                    //    0x99,0x93,0x14,0x00,0x01,0x01,0x03,0xaa,0x01,0x02,0x01,0x03,0xaa,0x00,0x03,0x01,0x03,0x33,0x00,0x04,0x01,0x03,0x55,0x01 // crc 0x38
                    //};
                    return new byte[] {
                        // 4 sensors with single negative temperature
                        // Number of sensors: 4, Analog 1 42.6, Analog 2 17, Analog 3 5.1, Analog 4 -17.1

                    0x99,0x93,0x14,0x00,0x01,0x00,0x03,0xaa,0x01,0x02,0x00,0x03,0xaa,0x00,0x03,0x00,0x03,0x33,0x00,0x04,0x00,0x03,0x55,0xff // crc 0x36
                    };
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
            Console.Write(BitConverter.ToString(data).Replace("-", ""));

            return true;
        }

        private int GetBaudrate()
        {
            return 38400;
        }

        public string GetMenuString()
        {
            return "Carrier DataCold600 (Request-Response protocol @ baudrate 38400)";
        }
    }
}
