using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialPortDataEmulatorConsole.SerialProtocols
{
    class CarrierGatewayEmulator : ISerialEmulator
    {
        private SerialPort Port;
        private CarrierGatewayMessage RxMsg = new CarrierGatewayMessage();
        
        public void Init(SerialPort port)
        {
            this.Port = port;
            this.Port.StopBits = StopBits.One;
            this.Port.BaudRate = GetBaudrate();
            this.Port.Open();

            Console.WriteLine($"CarrierGatewayProtocol Emulator Initialized. {port.PortName}, Baudrate: {GetBaudrate()}");
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
            
            SendResponse(data_without_crc);
            SendResponse(new byte[] { crc });
        }

        protected virtual byte[] GenerateResponseWithoutCRC(byte command)
        {
            switch (command)
            {
                case 0x20:
                    return new byte[] {

                        0x99, 0xa0, 0x42, 0x00,
                        0x2f, 0x01, 0x05, 0x07, 0x5a, 0x43, 0x38, 0x34, 0x38, 0x30, 0x30, 0x35,
                        0x20, 0x20, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x35, 0x2e, 0x31, 0x36, 0x2e, 0x30, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x4d, 0x4e, 0x34, 0x33, 0x35, 0x20, 0x20, 0x20,
                        0x20, 0x20, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0b0001, /* Compartments active (selected bits)*/
                        1,      /* Run mode*/   //(0)start-stop, (1)continous, (2)cycle-sentry
                        0,      /* Power mode*/ //(0)engine, 1(electric)
                        0,      /* Speed mode*/ //(0)normal, 1(high)
                        0x07, 0xe1, 0x00, 0x00,
                        0xe0, 0x00, 0x00, 0x03, 0x00, 0x01 // wihtout crc

                        // original msg
                        //0x99, 0xa0, 0x42, 0x00, 0x2f, 0x01, 0x05, 0x07, 0x5a, 0x43, 0x38, 0x34, 0x38, 0x30, 0x30, 0x35,
                        //0x20, 0x20, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x35, 0x2e, 0x31, 0x36, 0x2e, 0x30, 0x00, 0x00,
                        //0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x4d, 0x4e, 0x34, 0x33, 0x35, 0x20, 0x20, 0x20,
                        //0x20, 0x20, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x07, 0xe1, 0x00, 0x00,
                        //0xe0, 0x00, 0x00, 0x03, 0x00, 0x01 // crc d9
                    };

                case 0x22:
                
                    Console.WriteLine($"receive compartment request. {BitConverter.ToString(RxMsg.Data.Payload, 0, RxMsg.Data.DataSize)}");
                    return new byte[] {
                        0x99, 0xa2, 0x21, 0x00,
                        RxMsg.Data.Payload[0], /* compartment id*/

                        // on
                        0b11, // state flags
                        0x02, // compartment mode

                        // off
                        //0b0, // state flags
                        //0x00, // compartment mode
                        /*
                        reefer_compartment_mode_off =		0,			///< izslēgts
                        reefer_compartment_mode_heating =	1,			///< sildīšana
                        reefer_compartment_mode_cooling =	2,			///< dzesēšana
                        reefer_compartment_mode_idle =		3,			///< miera stāvoklis
                        reefer_compartment_mode_defrost =	4,			///< atkausēšana
                        reefer_compartment_mode_pre_trip =	5,			///< sagatavošana pirms ceļa
                        reefer_compartment_mode_unknown =	15,			///< nezināms
                        */

                        0x00, 0x00, 0x03, 0xab, 0x00,   /* setpoint */  /*id(u8),type(u8),flags(u8),value(u16)*/
                        0x01, 0x00, 0x03, (byte)new Random().Next(190, 200), 0x00,   /* supply0 */   /*id(u8),type(u8),flags(u8),value(u16)*/
                        0x02, 0x00, 0x00, 0x00, 0x00,   /* supply1 */   /*id(u8),type(u8),flags(u8),value(u16)*/
                        0x01, 0x00, 0x03, (byte)new Random().Next(130, 140), 0x00,   /* return0 */   /*id(u8),type(u8),flags(u8),value(u16)*/
                        0x02, 0x00, 0x00, 0x00, 0x00,   /* return1 */   /*id(u8),type(u8),flags(u8),value(u16)*/
                        0x00, 0x00, 0x00, 0x00, 0x00    /* evap */      /*id(u8),type(u8),flags(u8),value(u16)*/
                    };
                    
                case 0x23:
                    return new byte[] {
                        0x99, 0xa3, 0x11, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

                case 0x24:
                    return new byte[] {
                        0x99, 0xa4, 0x28, 0x00, 0x05, 0x16, 0x00, 0x00, 0x00, 0x05, 0x8b, 0x04, 0x00, 0x00, 0x05, 0xf9,
                        0x02, 0x00, 0x00, 0x00, 0x57, 0x17, 0x04, 0x00, 0x04, 0x34, 0xcf, 0x3f, 0x00, 0x03, 0x75, 0x77,
                        0x42, 0x00, 0x01, 0xb5, 0x77, 0x42, 0x00, 0x07, 0x8a, 0xb1, 0x06, 0x00, // crc 0xb4
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

            return true;
        }

        private int GetBaudrate()
        {
            return 38400;
        }

        public string GetMenuString()
        {
            return "Carrier gateway (Request-Response protocol @ baudrate 38400)";
        }
    }
}
