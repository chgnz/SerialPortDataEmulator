using System;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SerialPortDataEmulatorConsole.SerialProtocols
{
    class TvgGritterEmulator : ISerialEmulator
    {
        private SerialPort Port;
        private UInt32 TxInterval;

        private TvgGritterMessage TvgRxMsg = new TvgGritterMessage();

        private long Timestamp;
        private int counter = 0;

        private byte[] outArrEx;
        private byte[] outArr;

        public void Init(SerialPort port)
        {
            this.Port = port;
            this.Port.StopBits = StopBits.One;

            this.Port.BaudRate = GetBaudrate();
            this.TxInterval = GetTxInterval();

            this.Port.Open();

            Console.WriteLine($"TVG Gritter emulator. {port.PortName}, Baudrate: {GetBaudrate()}, Tx interval: {GetTxInterval()} ms");

            Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        }

        public void Trigger()
        {
            if (this.TxInterval == 0 || GetTimestamp() - this.Timestamp < this.TxInterval)
            {
                return;
            }

            // first 100 standard messages and after that 100 extended messages

            if ((counter >= 100) && (counter < 200))
            {
                ComposeMessage(true);
                outArrEx = convertToByteStreamEx(TvgRxMsg.DataExtnd);
                string outStrEx = Encoding.Default.GetString(outArrEx);
                Console.WriteLine("OUT ex: " + outStrEx);
                this.Port.Write(outStrEx);
            }
            else if (counter < 100)
            {
                ComposeMessage(false);
                outArr = convertToByteStream(TvgRxMsg.DataStd);
                string outStr = Encoding.Default.GetString(outArr);
                Console.WriteLine("OUT: " + outStr);
                this.Port.Write(outStr);
            }
            else
            {
                counter = 0;
            }

            counter++;

            this.Timestamp = GetTimestamp();
        }


        public void ComposeMessage(bool isExtnd)
        {

            var rand = new Random();

            // spreader rate type random generation
            byte[] spreadRateByte = Encoding.ASCII.GetBytes(rand.Next(200).ToString("000"));


            // spreader width type random generation
            byte[] spreadWidthByte = Encoding.ASCII.GetBytes(rand.Next(20).ToString("00"));

            // pattern type random generation
            byte[] patternByte = Encoding.ASCII.GetBytes(rand.Next(9).ToString("0"));

            // flags1 type random generation
            int flags1 = rand.Next(255);
            byte[] flags1Byte = Encoding.ASCII.GetBytes(Convert.ToString(flags1, 2).PadLeft(8, '0'));

            // flags2 type random generation
            int flags2 = rand.Next(255);
            byte[] flags2Byte = Encoding.ASCII.GetBytes(Convert.ToString(flags2, 2).PadLeft(8, '0'));

            // vehicle type generation
            byte[] vehicleTypeArr = { 0x50, 0x55, 0x4e, 0x53, 0x4d, 0x3f };
            byte vehicleType = vehicleTypeArr[rand.Next(vehicleTypeArr.Length)];

            // salt type random generation
            byte[] saltTypeByte = Encoding.ASCII.GetBytes(rand.Next(20).ToString("00"));


            // flags3 type random generation
            int flags3 = rand.Next(255);
            byte[] flags3Byte = Encoding.ASCII.GetBytes(Convert.ToString(flags3, 2).PadLeft(8, '0'));

            // sensor type generation
            byte[] sensorTypeArr = { 0x56, 0x52 };
            byte sensorType = sensorTypeArr[rand.Next(sensorTypeArr.Length)];

            // Surface temperature
            double resSurfTemp = Math.Round((rand.NextDouble() - rand.NextDouble()) * 200, 1);
            byte[] temporarySurfTemp = Encoding.ASCII.GetBytes(Math.Abs(resSurfTemp).ToString("000.0"));

            byte[] surfTempByte = new byte[6];
            surfTempByte[1] = temporarySurfTemp[0];
            surfTempByte[2] = temporarySurfTemp[1];
            surfTempByte[3] = temporarySurfTemp[2];
            surfTempByte[4] = (byte)'.';
            surfTempByte[5] = temporarySurfTemp[4];

            if (resSurfTemp > 0.0)
            {
                surfTempByte[0] = (byte)'+';
            }
            else
            {
                surfTempByte[0] = (byte)'-';
            }


            // Air temperature
            double resAirTemp = Math.Round((rand.NextDouble() - rand.NextDouble()) * 200, 1);
            byte[] temporaryAirTemp = Encoding.ASCII.GetBytes(Math.Abs(resAirTemp).ToString("000.0"));

            byte[] airTempByte = new byte[6];
            airTempByte[1] = temporaryAirTemp[0];
            airTempByte[2] = temporaryAirTemp[1];
            airTempByte[3] = temporaryAirTemp[2];
            airTempByte[4] = (byte)'.';
            airTempByte[5] = temporaryAirTemp[4];

            if (resAirTemp > 0.0)
            {
                airTempByte[0] = (byte)'+';
            }
            else
            {
                airTempByte[0] = (byte)'-';
            }


            if (!isExtnd)
            {

                Array.Copy(spreadRateByte, TvgRxMsg.DataStd.SpreadingRate, spreadRateByte.Length);
                Array.Copy(spreadWidthByte, TvgRxMsg.DataStd.SpreadingWidth, spreadWidthByte.Length);
                Array.Copy(patternByte, TvgRxMsg.DataStd.Pattern, patternByte.Length);
                Array.Copy(flags1Byte, TvgRxMsg.DataStd.Flags1, flags1Byte.Length);
                Array.Copy(flags2Byte, TvgRxMsg.DataStd.Flags2, flags2Byte.Length);
                TvgRxMsg.DataStd.VehicleType = vehicleType;
                Array.Copy(saltTypeByte, TvgRxMsg.DataStd.SaltType, saltTypeByte.Length);
                Array.Copy(flags3Byte, TvgRxMsg.DataStd.Flags3, flags3Byte.Length);
            }
            else
            {
                Array.Copy(spreadRateByte, TvgRxMsg.DataExtnd.SpreadingRate, spreadRateByte.Length);
                Array.Copy(spreadWidthByte, TvgRxMsg.DataExtnd.SpreadingWidth, spreadWidthByte.Length);
                Array.Copy(patternByte, TvgRxMsg.DataExtnd.Pattern, patternByte.Length);
                Array.Copy(flags1Byte, TvgRxMsg.DataExtnd.Flags1, flags1Byte.Length);
                Array.Copy(flags2Byte, TvgRxMsg.DataExtnd.Flags2, flags2Byte.Length);
                TvgRxMsg.DataExtnd.VehicleType = vehicleType;
                Array.Copy(saltTypeByte, TvgRxMsg.DataExtnd.SaltType, saltTypeByte.Length);
                TvgRxMsg.DataExtnd.SensorType = sensorType;
                Array.Copy(flags3Byte, TvgRxMsg.DataExtnd.Flags3, flags3Byte.Length);
                Array.Copy(surfTempByte, TvgRxMsg.DataExtnd.SurfaceTemp, surfTempByte.Length);
                Array.Copy(airTempByte, TvgRxMsg.DataExtnd.AirTemp, airTempByte.Length);
            }
        }

        public void SendMessageToPort(bool isExtnd)
        {
        }


        public byte[] convertToByteStream(TvgGritterMessage.MessageStd messageStruct)
        {
            int size = Marshal.SizeOf(messageStruct);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(messageStruct, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            return arr;
        }

        public byte[] convertToByteStreamEx(TvgGritterMessage.MessageExtnd messageStruct)
        {
            int size = Marshal.SizeOf(messageStruct);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(messageStruct, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            return arr;
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

        public string GetMenuString()
        {
            return "TVG gritter(automatically transmits each 1000ms @ baudrate 9600)";
        }

        public void DeInit()
        {
            this.Port.Close();
        }
    }
}
