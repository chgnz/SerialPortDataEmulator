using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SerialPortDataEmulatorConsole.SerialProtocols
{
    class FuelSensorReadMode : ISerialEmulator
    {
        private Stopwatch debug_stopwatch;
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

            DutMessage tx_msg = new DutMessage();
            tx_msg.Data.Header = 0x31;
            tx_msg.Data.Command = (byte)DutMessage.DutCommand.filtered_values;
            tx_msg.Data.Address = 0xff;
            tx_msg.Data.Size = 0;

            SendRequest(tx_msg);

            if (WaitResponse())
            {
                // todo
            }

            Thread.Sleep(100);
        }

        private void SendRequest(DutMessage request)
        {
            var data_without_crc = new byte[] { request.Data.Header, request.Data.Address, request.Data.Command };

            var CRC_Calculator = new DallasCRC();
            byte crc = CRC_Calculator.Begin();

            for (int i = 0; i < data_without_crc.Length; i++)
            {
                crc = CRC_Calculator.Update(crc, data_without_crc[i]);
            }

            byte[] data = data_without_crc.Concat(new byte[] { crc }).ToArray();
            SendData(data);
        }

        bool SendData(byte[] data)
        {
            if (!Port.IsOpen)
            {
                return false;
            }

            string s = Port.ReadExisting();

            if (s.Length > 0)
            {
                Console.WriteLine($"have  {s.Length} bytes");
            }

            Port.Write(data, 0, data.Count());

            debug_stopwatch = Stopwatch.StartNew();

            return true;
        }

        private Stack<long> timeouts = new Stack<long>(20);

        bool WaitResponse()
        {
            const int MIN_DUT_PACKET_SIZE = 4;

            DutMessage rx_msg = new DutMessage();

            TimeSpan maxDuration = TimeSpan.FromMilliseconds(3500);
            Stopwatch sw = Stopwatch.StartNew();

            while (sw.Elapsed < maxDuration)
            {
                if (Port.BytesToRead == 0)
                {
                    Thread.Sleep(1);
                    continue;
                }

                byte rx_byte = (byte)Port.ReadByte();

                if (rx_msg.Receive(rx_msg, rx_byte))
                {
                    debug_stopwatch.Stop();
                    timeouts.Push(debug_stopwatch.ElapsedMilliseconds);
                    
                    var Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                    Console.WriteLine($"{DateTime.UtcNow} - Dut Message received (after {debug_stopwatch.ElapsedMilliseconds,6:##} ms, (avg: {timeouts.Average(),6:##.0})): command: 0x{rx_msg.Data.Command:x}, address: 0x{rx_msg.Data.Address:x}," +
                        $" {BitConverter.ToString(rx_msg.Data.Payload, 0, rx_msg.Data.Size)}, {rx_msg.Data.DataBytesReceived}, {rx_msg.Data.Size} ");
                    DutRxMsg.Reset();

                    return true;
                }
            }

            Console.WriteLine($"timeout occured after {debug_stopwatch.ElapsedMilliseconds:d6} ms, ");

            return false;
        }
        
        static float avg;

        virtual protected int GetBaudrate()
        {
            return 19200;
        }


        virtual public string GetMenuString()
        {
            return "DUT Fuel sensor read mode (Read filtred values)";
        }
    }
}