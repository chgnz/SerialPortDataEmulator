using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace SerialPortDataEmulatorConsole.SerialProtocols
{
    class DTRPulseTrigger : ISerialEmulator
    {
        private SerialPort Port;

        public string GetMenuString()
        {
            return "Generate DTR ~500ms Pulse on data receive ";
        }

        public void Init(SerialPort port)
        {
            this.Port = port;
            this.Port.StopBits = StopBits.One;
            this.Port.BaudRate = 9600;
            this.Port.Open();
            this.Port.DtrEnable = false;
        }

        public void Trigger()
        {
            if (Port.BytesToRead == 0)
            {
                return;
            }

            // wait additional 5 ms to receive all request.
            Thread.Sleep(25);

            if (Port.BytesToRead >= 1)
            {
                var data_received_count = this.Port.BytesToRead;
                byte[] buffer = new byte[data_received_count];
                int bytes_read = Port.Read(buffer, 0, data_received_count);

                string rawAsciiMsg = System.Text.Encoding.ASCII.GetString(buffer);

                Console.WriteLine($"{DateTime.Now.ToString("hh:mm:ss fff")} - Request Received. '{rawAsciiMsg}'");

                Console.Beep(1111, 111);

                this.Port.DtrEnable = true;
                Thread.Sleep(25);
                this.Port.DtrEnable = false;
            }
        }
    }
}
