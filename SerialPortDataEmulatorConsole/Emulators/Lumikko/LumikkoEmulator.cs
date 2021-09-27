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
    class LumikkoEmulator : ISerialEmulator
    {
        private SerialPort Port;
        private LumikkoMessage RxMsg = new LumikkoMessage();
        
        public void Init(SerialPort port)
        {
            this.Port = port;
            this.Port.StopBits = StopBits.One;
            this.Port.BaudRate = GetBaudrate();
            this.Port.Open();

            Console.WriteLine($"Lumikko Emulator Initialized. {port.PortName}, Baudrate: {GetBaudrate()}");
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

            while (Port.BytesToRead >= RxMsg.GetRequiredSize() && 
                    sw.Elapsed < maxDuration)
            {
                byte[] buffer = new byte[RxMsg.GetRequiredSize()];
                int bytes_read = Port.Read(buffer, 0, RxMsg.GetRequiredSize());

                if (bytes_read != RxMsg.GetRequiredSize())
                {
                    Console.WriteLine($"ERROR!, bytes_read : {bytes_read}");
                }
                
                if (RxMsg.Receive(buffer))
                {
                    string rawAsciiMsg = System.Text.Encoding.ASCII.GetString(this.RxMsg.Data.Payload, 0, this.RxMsg.Data.Pointer);

                    Console.WriteLine($"{DateTime.Now.ToString("hh:mm:ss fff")} - Request Received. Command: '{this.RxMsg.Data.Command}', bytes left: '{Port.BytesToRead}', '{rawAsciiMsg}'");

                    HandleRequest();
                    RxMsg.Reset();
                    return true;
                }
            }

            return false;
        }

        private void HandleRequest()
        {
            string response = GenerateResponseWithoutCRC(RxMsg.Data.Command);
            if (response == null)
            {
                Console.WriteLine("No response");
                return;
            }

            byte crc = LumikkoMessage.CalculateCrc(ASCIIEncoding.ASCII.GetBytes(response));

            response += $"{crc}\n";

            SendResponse(response);
        }

        private readonly double setpoint_offset = 0.0;
        private readonly double return_temp_offset = 0.0;
        
        // compartment mode -> 0 (off), 1 (cooling). 2 (heating), 3 (defrost), 4 (standby)
        private readonly int compartment_mode = 1;

        protected virtual string GenerateResponseWithoutCRC(string command)
        {
            switch (command)
            {
                case "001":
                    //  model, io-version, ds-version
                    return "!001,300D,0.42,0.10,";

                case "002":
                    //  power_mode: engine,  autostart off (continous?),   no alarms
                    return "!002,1,1,0,";

                    //  power_mode,  autostart on,   with alarms
                    // return "!002,2,1,1,";

                case "003":
                    //  active alarms : 1,2,3,4
                    return $"!003,1,2,3,4,";
                   // return $"!003,1,2,3,{new Random().Next(1, 99).ToString()},";

                case "004":
                    // compartment mode -> 0 (off), 1 (cooling). 2 (heating), 3 (defrost), 4 (standby)
                    // setpoint, ±xx.x | return ±xx.x
                    return $"!004,{compartment_mode}," +
                        $"{(10.1 + setpoint_offset).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)}," +
                        $"{(11.1 + return_temp_offset).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)},";

                case "005":
                    return $"!005,{compartment_mode}," +
                        $"{(10.2 + setpoint_offset).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)}," +
                        $"{(11.2 + return_temp_offset).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)},";

                case "006":
                    return $"!006,{compartment_mode}," +
                        $"{(10.3 + setpoint_offset).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)}," +
                        $"{(11.3 + return_temp_offset).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)},";

                case "010":
                    // active compartments ? bitwise ? integer?
                    return "!010,15,";

                case "012":
                    // compartment amount (1 compartment)
                    return "!012,1,";

                case "023":
                    // Idling  time (23), 
                    return "!023,12,"; // 12 hours?

                case "025":
                    // diesel hours
                    return "!025,123,";    // 123 hours

                case "026":
                    // Electric hours
                    return "!026,36,"; // 36 hours

                case "045":
                    // evaporator coil. +-12.3,+-23.4,+-34.5
                    return "!045,+3.3,-4.4,+5.5,";

                case "046":
                    //Diesel info, coolant temp, rpm, battery voltage
                    return $"!046,+87.3,{new Random().Next(1000, 2000).ToString()},12.{new Random().Next(56, 99).ToString()},";

                default:
                    Console.WriteLine($"unknown request: {command}");
                    return null;
            }
        }

        bool SendResponse(string response)
        {
            if (!Port.IsOpen)
            {
                return false;
            }

            byte[] data = ASCIIEncoding.ASCII.GetBytes(response);

            Port.Write(data, 0, data.Count());

            Console.WriteLine($"data sent: '{response.Replace("\n", "\\n")}', size: {data.Count()}");

            return true;
        }

        private int GetBaudrate()
        {
            return 9600;
        }

        public string GetMenuString()
        {
            return "Lumikko (Request-Response protocol @ baudrate 9600)";
        }
    }
}
