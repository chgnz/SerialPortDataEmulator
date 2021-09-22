using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace SerialPortDataEmulatorConsole.SerialProtocols
{
    class UDSEmulator : ISerialEmulator
    {
        // start communication5

        readonly byte[] start_communication_cmd = new byte[] {0x81, 0xee, 0xf0, 0x81, 0xe0 };
        readonly byte[] start_diagnostic__session_cmd = new byte[] { 0x80, 0xee, 0xf0, 0x02, 0x10, 0x85, 0xf5 }; // on EFES tacho session id  is 0x85
        //readonly byte[] start_diagnostic__session_cmd = new byte[] { 0x80, 0xee, 0xf0, 0x02, 0x10, 0x81, 0xf1 }; // on stoneridge session id is 0x81
        readonly byte[] request_param_cmd = new byte[] { 0x80, 0xee, 0xf0, 0x03, 0x22, 0xe0 };

        // interval settings
        private UInt32 TxInterval;
        private SerialPort Port;

        // private vars
        private long Timestamp;
        private int UdsMsgIndex;

        private byte[] GenerateReadByIdRequest(UInt16 id)
        {
            // last byte : 0x00 is CRC, leave as zero as crc caluclates total sum of all bytes
            byte[] request = new byte[] { 0x80, 0xee, 0xf0, 0x03, 0x22, (byte)(id >> 8), (byte)(id & 0xff), 0x00 };

            byte crc = 0;

            foreach (byte b in request) 
            {
                crc += b;
            }

            request[request.Length - 1] = crc;

            return request;
        }

        public void Init(SerialPort port)
        {
            this.Port = port;
            this.Port.StopBits = StopBits.One;
            this.Port.Parity = Parity.Even;
            this.Port.BaudRate = GetBaudrate();
            this.TxInterval = GetTxInterval();

            this.Port.Open();

            Console.WriteLine($"UDS data request on RS232. {port.PortName}, Baudrate: {GetBaudrate()}, Tx interval: {GetTxInterval()} ms");

            Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        }
        bool WaitResponse()
        {
            TimeSpan maxDuration = TimeSpan.FromMilliseconds(1000);
            Stopwatch sw = Stopwatch.StartNew();
            Stopwatch lastDataRx = null;

            byte[] buffer = new byte[256];
            int offset = 0;

            while (sw.Elapsed < maxDuration && (lastDataRx == null || lastDataRx.ElapsedMilliseconds < 50))
            {
                if (Port.BytesToRead > 0)
                {
                    int data_size = Port.BytesToRead;
                    Port.Read(buffer, offset, data_size);
                    offset += data_size;

                    if (lastDataRx == null)
                    {
                        lastDataRx = Stopwatch.StartNew();
                    }

                }

                Thread.Sleep(1);
            }

            if (offset != 8 || (buffer[5] != 0x22 && buffer[6] != 0x32))
            {
                Console.WriteLine($"received: {offset} bytes: {BitConverter.ToString(buffer, 0, offset)}");
            }

            return false;
        }

        public void Trigger()
        {
            if (this.TxInterval == 0 ||
                GetTimestamp() - this.Timestamp < this.TxInterval)
            {
                // nav pienācies laiks sūtīt
                return;
            }

            this.Send();
            this.WaitResponse();

            this.Timestamp = GetTimestamp();
        }

        static UInt16 scan_start_id = 0xfd00;
        bool Send()
        {
            if (!Port.IsOpen)
            {
                return false;
            }


            switch (UdsMsgIndex++)
            {
                // at the start of message send the start comms and start diagnstic commands
                case 0:
                    Port.Write(start_communication_cmd, 0, start_communication_cmd.Length);
                    //Console.WriteLine($"start_communication_cmd: {BitConverter.ToString(start_communication_cmd).Replace("-", "")}");
                    break;
                case 1:
                    Port.Write(start_diagnostic__session_cmd, 0, start_diagnostic__session_cmd.Length);
                    //Console.WriteLine($"start_diagnostic__session_cmd: {BitConverter.ToString(start_diagnostic__session_cmd).Replace("-", "")}");
                    break;

                default:
                    byte[] request = GenerateReadByIdRequest(scan_start_id);
                    Port.Write(request, 0, request.Length);
                    //Console.WriteLine($"request_param_cmd ({scan_start_id:x}): {BitConverter.ToString(request).Replace("-", "")}");
                    //Console.Write(".");
                    scan_start_id++;
                    break;
            }


            return true;
        }

        private long GetTimestamp()
        {
            return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
        }

        private int GetBaudrate()
        {
            return 9600;
        }

        private UInt32 GetTxInterval()
        {
            return 250;
        }
    }
}
