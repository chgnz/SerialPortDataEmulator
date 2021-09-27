using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace SerialPortDataEmulatorConsole
{
    public class TvgGritterMessage
    {
        public byte[] stdMessageInit = { 0x3c, 0x00, 0x00, 0x00, 0x2c, 0x00, 0x00, 0x2c, 0x00, 0x2c, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                  0x00, 0x00, 0x2c, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x2c, 0x00, 0x00, 0x00, 0x2c,
                                  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3e, 0x0d, 0x0a};

        public byte[] extdMessageInit = { 0x3c, 0x00, 0x00, 0x00, 0x2c, 0x00, 0x00, 0x2c, 0x00, 0x2c, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                  0x00, 0x00, 0x2c, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x2c, 0x00, 0x00, 0x00, 0x2c,
                                  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x2c, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                  0x2c, 0x30, 0x30, 0x2c, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x2c, 0x30, 0x30, 0x30, 0x30, 0x3e,
                                  0x0d, 0x0a};
        public struct MessageStd
        {
            public byte StartOfMsg;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] SpreadingRate;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] SpreadingWidth;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] Pattern;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
            public byte[] Flags1;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
            public byte[] Flags2;
            public byte VehicleType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] SaltType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] Flags3;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] EndOfMsg;
        }
        public struct MessageExtnd
        {
            public byte StartOfMsg;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] SpreadingRate;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] SpreadingWidth;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] Pattern;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
            public byte[] Flags1;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
            public byte[] Flags2;
            public byte VehicleType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] SaltType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
            public byte[] Flags3;
            public byte SensorType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
            public byte[] SurfaceTemp;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] NotUsed1;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
            public byte[] AirTemp;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] NotUsed2;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] EndOfMsg;
        }

        public MessageStd DataStd;
        public MessageExtnd DataExtnd;
        public TvgGritterMessage()
        {
            this.DataStd.SpreadingRate = new byte[4];
            this.DataStd.SpreadingWidth = new byte[3];
            this.DataStd.Pattern = new byte[2];
            this.DataStd.Flags1 = new byte[9];
            this.DataStd.Flags2 = new byte[9];
            this.DataStd.SaltType = new byte[3];
            this.DataStd.Flags3 = new byte[8];
            this.DataStd.EndOfMsg = new byte[3];


            this.DataExtnd.SpreadingRate = new byte[4];
            this.DataExtnd.SpreadingWidth = new byte[3];
            this.DataExtnd.Pattern = new byte[2];
            this.DataExtnd.Flags1 = new byte[9];
            this.DataExtnd.Flags2 = new byte[9];
            this.DataExtnd.SaltType = new byte[2];
            this.DataExtnd.Flags3 = new byte[9];
            this.DataExtnd.SurfaceTemp = new byte[7];
            this.DataExtnd.NotUsed1 = new byte[3];
            this.DataExtnd.AirTemp = new byte[7];
            this.DataExtnd.NotUsed2 = new byte[4];
            this.DataExtnd.EndOfMsg = new byte[3];

            this.DataStd = convertByteToStruct(stdMessageInit);
            this.DataExtnd = convertByteToStructEx(extdMessageInit);
        }

        public TvgGritterMessage.MessageStd convertByteToStruct(byte[] arr)
        {
            MessageStd dataStd = new MessageStd();

            int size = Marshal.SizeOf(dataStd);
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(arr, 0, ptr, size);

            dataStd = (MessageStd)Marshal.PtrToStructure(ptr, dataStd.GetType());
            Marshal.FreeHGlobal(ptr);

            return dataStd;
        }

        public TvgGritterMessage.MessageExtnd convertByteToStructEx(byte[] arr)
        {
            MessageExtnd dataStd = new MessageExtnd();

            int size = Marshal.SizeOf(dataStd);
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(arr, 0, ptr, size);

            dataStd = (MessageExtnd)Marshal.PtrToStructure(ptr, dataStd.GetType());
            Marshal.FreeHGlobal(ptr);

            return dataStd;
        }
    }
}
