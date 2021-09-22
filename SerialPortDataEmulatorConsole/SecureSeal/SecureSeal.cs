using System;
using System.IO.Ports;
using System.Linq;

namespace SerialPortDataEmulatorConsole.SerialProtocols
{
    class SecureSeal : ISerialEmulator
    {
        // Msg to send
        private readonly byte[][] msg = new byte[][] {
            new byte[] {0x88, 0x00, 0x00, 0xa6, 0x16, 0x00, 0x00, 0x2b, 0xb6, 0x8e, 0x48 },
            };

        // interval settings
        private UInt32 TxInterval;
        private SerialPort Port;

        // private vars
        private long Timestamp;
        private uint SecureSealMsgIndex;

        public void Init(SerialPort port)
        {
            this.Port = port;
            this.Port.StopBits = StopBits.One;
            this.Port.BaudRate = GetBaudrate();
            this.TxInterval = GetTxInterval();

            this.Port.Open();

            Console.WriteLine($"Secure Seal. {port.PortName}, Baudrate: {GetBaudrate()}, Tx interval: {GetTxInterval()} ms");

            Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
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

            this.Timestamp = GetTimestamp();
        }

        bool Send()
        {
            if (!Port.IsOpen)
            {
                return false;
            }
            Console.WriteLine($"SecureSeal Emulator Send: {BitConverter.ToString(msg[SecureSealMsgIndex]).Replace("-","")}");

            Port.Write(msg[SecureSealMsgIndex], 0, msg[SecureSealMsgIndex].Length);
            SecureSealMsgIndex++;
            SecureSealMsgIndex = (uint)(SecureSealMsgIndex % msg.Count());

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
            return 1000;
        }
    }
}
