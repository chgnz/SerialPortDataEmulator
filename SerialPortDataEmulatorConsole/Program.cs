using System;
using System.IO.Ports;
using System.Linq;
using System.Threading;

using SerialPortDataEmulatorConsole.SerialProtocols;

namespace SerialPortDataEmulatorConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            ISerialEmulator protocol;

            DisplayMenu();
            
            switch (ReadSelectedProtocol())
            {
                case SerialProtocol.SREProtocol:
                    protocol = new StoneridgeEmulator();
                    break;

                case SerialProtocol.VDOProtocol:
                    protocol = new SiemensVDOEmulator();
                    break;

                case SerialProtocol.DUTProtocol:
                    protocol = new DUTEmulator();
                    break;

                case SerialProtocol.EpsilonESProtocol:
                    protocol = new EpsilonESEmulator();
                    break;

                case SerialProtocol.CarrierGatewayProtocol:
                    protocol = new CarrierGatewayEmulator();
                    break;

                case SerialProtocol.LumikkoProtocol:
                    protocol = new LumikkoEmulator();
                    break;

                case SerialProtocol.CarrierDirectProtocol:
                    protocol = new CarrierDirectEmulator();
                    break;

                case SerialProtocol.EuroscanProtocol:
                    protocol = new EuroscanEmulator();
                    break;

                case SerialProtocol.ThermokingTouchprintASCII:
                    protocol = new ThermokingTouchprintASCIIEmulator();
                    break;

                default:
                    Console.WriteLine("incorect protocol selected, close app");
                    Console.ReadKey();
                    return;
            }

            SerialPort sp = new SerialPort("COM7");

            protocol.Init(sp);

            while (true)
            {
                if (Console.KeyAvailable)
                {
                    if (Console.ReadKey().Key == ConsoleKey.Escape)
                    {
                        Console.WriteLine("Esc pressed, close app");
                        Thread.Sleep(2000);
                        break;
                    }

                    if (Console.ReadKey().Key == ConsoleKey.Spacebar)
                    {
                        Console.WriteLine("Space pressed, send random data");
                        sp.Write("M6   0.0,0000\r");
                        continue;
                    }

                    //ignore other keys
                    Console.ReadKey();

                }

                protocol.Trigger();
                Thread.Sleep(1);
            }
        }

        static public void DisplayMenu()
        {
            Console.WriteLine("Select Serial Protocol");
            Console.WriteLine();
            Console.WriteLine("1. Stoneridge SRE (automatically transmits SRE data each 500ms @ baudrate 1200)");
            Console.WriteLine("2. Siemens VDO (automatically transmits VDO data each 1000ms @ baudrate 10400)");
            Console.WriteLine("3. DUT FuelSensor (Request-Response protocol @ baudrate 19200), implemented commands: 0x02, 0x06, 0x1c, 0x23");
            Console.WriteLine("4. Epsilon ES FuelSensor (Request-Response protocol @ baudrate 19200), implemented commands: 0x06");
            Console.WriteLine("5. Carrier gateway (Request-Response protocol @ baudrate 38400)");
            Console.WriteLine("6. Lumikko (Request-Response protocol @ baudrate 9600)");
            Console.WriteLine("7. Carrier Direct (Request-Response protocol @ baudrate 9600)");
            Console.WriteLine("8. Euroscan MX Series (Request-Response ASCII protocol @ baudrate 9600)");
            Console.WriteLine("9. Thermoking Touchprint (Request-Response ASCII protocol @ baudrate 9600)");
        }

        public enum SerialProtocol
        {
            SREProtocol = 1,
            VDOProtocol,
            DUTProtocol,
            EpsilonESProtocol,
            CarrierGatewayProtocol,
            LumikkoProtocol,
            CarrierDirectProtocol,
            EuroscanProtocol,
            ThermokingTouchprintASCII,

            UnknownProtocol,
        };

        static SerialProtocol ReadSelectedProtocol()
        {
            string selectedIndexString = Console.ReadLine();
            int selectedValue = -1;

            Int32.TryParse(selectedIndexString, out selectedValue);

            if (selectedValue < (int)SerialProtocol.SREProtocol ||
                selectedValue >= (int)SerialProtocol.UnknownProtocol)
            {
                return SerialProtocol.UnknownProtocol;
            }
            else
            {
                return (SerialProtocol)selectedValue;
            }
        }
    }
}
