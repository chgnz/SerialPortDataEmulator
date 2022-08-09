using System.IO.Ports;

namespace SerialPortDataEmulatorConsole.SerialProtocols
{
    class EmulatorFactory
    {
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
            SecureSeal,
            UDSRequest,
            VEIMillennium,
            TVGGritter,
            Trailoader,
            TrueLoad,
            Test,
            TranScan,
            DataCold600,
            ThermokingTouchprintModbus,
            J1708,
            APM303Modbus,
            Dignita,
            Drager,
            IntellicEFAS,
            UnknownProtocol,
        };


        public static ISerialEmulator Build(int protocol)
        {
            switch ((SerialProtocol)protocol)
            {
                case SerialProtocol.SREProtocol:
                    return new StoneridgeEmulator();

                case SerialProtocol.VDOProtocol:
                    return new SiemensVDOEmulator();

                case SerialProtocol.DUTProtocol:
                    return new DUTEmulator();

                case SerialProtocol.EpsilonESProtocol:
                    return new EpsilonESEmulator();

                case SerialProtocol.CarrierGatewayProtocol:
                    return new CarrierGatewayEmulator();

                case SerialProtocol.LumikkoProtocol:
                    return new LumikkoEmulator();

                case SerialProtocol.CarrierDirectProtocol:
                    return new CarrierDirectEmulator();

                case SerialProtocol.EuroscanProtocol:
                    return new EuroscanEmulator();

                case SerialProtocol.ThermokingTouchprintASCII:
                    return new ThermokingTouchprintASCIIEmulator();

                case SerialProtocol.SecureSeal:
                    return new SecureSeal();

                case SerialProtocol.UDSRequest:
                    return new UDSEmulator();

                case SerialProtocol.VEIMillennium:
                    return new Millennium5Emulator();

                case SerialProtocol.TVGGritter:
                    return new TvgGritterEmulator();

                case SerialProtocol.Trailoader:
                    return new TrailoaderEmulator();

                case SerialProtocol.TrueLoad:
                    return new TrueloadEmulator();

                case SerialProtocol.Test:
                    return new Test();

                case SerialProtocol.TranScan:
                    return new TranScanEmulator();

                case SerialProtocol.DataCold600:
                    return new CarrierDataCold600Emulator();

                case SerialProtocol.ThermokingTouchprintModbus:
                    return new ThermokingTouchprintModbusEmulator();

                case SerialProtocol.J1708:
                    return new J1708Emulator();

                case SerialProtocol.APM303Modbus:
                    return new APM303ModbusEmulator();

                case SerialProtocol.Dignita:
                    return new DignitaSerialDemo();

                case SerialProtocol.Drager:
                    return new DragerSerialDemo();

                case SerialProtocol.IntellicEFAS:
                    return new IntellicEFASEmulatory();

                default:
                    throw new System.Exception("unknown protocol");
            }
        }

        public static string BuildInfoMenu()
        {
            string Menu = "";

            // start with index 1, for user friendly counting
            for (int i = 1; i < (int)SerialProtocol.UnknownProtocol; i++)
            {
                ISerialEmulator emulator = Build(i);

                Menu += $"{i}. {emulator.GetMenuString()}\n";
            }

            return Menu;
        }

    }
}
