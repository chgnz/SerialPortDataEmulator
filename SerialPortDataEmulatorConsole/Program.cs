using System;
using System.IO.Ports;
using System.Linq;
using System.Threading;

using SerialPortDataEmulatorConsole.SerialProtocols;
using static SerialPortDataEmulatorConsole.SerialProtocols.EmulatorFactory;

namespace SerialPortDataEmulatorConsole
{
    class Program
    {
        static ISerialEmulator emulator;

        static void Main(string[] args)
        {
            // show menu with available serial emulators
            DisplayMenu(EmulatorFactory.BuildInfoMenu());

            //todo remove this section
            emulator = EmulatorFactory.Build((int)SerialProtocol.J1708);
            SerialPort serialport = new SerialPort("COM40");
            emulator.Init(serialport);
            // todo till here

            //try
            //{
            //    // read selected index and set emulator. throws exception if invalid emulator selected
            //    emulator = SelectedEmulator();

            //    string[] ports = (string[])SerialPort.GetPortNames().Clone();

            //    // show all available Serial ports. throws exception if no serial ports are found
            //    ShowAvailableComportMenu(ports);
            //    // select serial port based on user entry, throws exception if invalid port selected
            //    SerialPort serialport = SelectedSerialPort(ports);

            //    emulator.Init(serialport);
            //}
            //catch (Exception ex)
            //{
            //    CloseApp($"{ex.Message} Press any key to close application");
            //}

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
                        continue;
                    }

                    //ignore other keys
                    Console.ReadKey();

                }

                emulator.Trigger();
                Thread.Sleep(1);
            }

        }

        static public void DisplayMenu(string MenuText)
        {
            Console.Write(MenuText);
        }

        static ISerialEmulator SelectedEmulator()
        {
            string selectedIndexString = Console.ReadLine();
            int selectedValue = -1;

            Int32.TryParse(selectedIndexString, out selectedValue);

            return EmulatorFactory.Build(selectedValue);
        }

        static void ShowAvailableComportMenu(string[] ports) 
        {

            if (ports.Length == 0)
            {
                throw new SystemException("Can't find any serial port");
            }

            Console.WriteLine($"Select serial port! (Use index instead of full portname)");

            for (int i = 0; i < ports.Length; i++)
            {
                Console.WriteLine($"{i+1}. {ports[i]}");
            }
        }
        static SerialPort SelectedSerialPort(string[] ports)
        {
            string selectedIndexString = Console.ReadLine();
            int selectedValue = -1;

            Int32.TryParse(selectedIndexString, out selectedValue);

            if (selectedValue < 1 || selectedValue > ports.Length) {
                throw new Exception("Error: Invalid serial port index used");
            }

            // NB! Indexes shown in user menu, are different with ports array indexes!.
            // UI shows indexes started from 1 instead of 0; thus using '-1';
            return new SerialPort(ports[selectedValue - 1]);
        }


        static void CloseApp(string exit_info)
        {
            Console.WriteLine(exit_info);
            Console.ReadKey();
            Environment.Exit(1);// exit
        }
    }
}
