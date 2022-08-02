using System;

namespace SerialPortDataEmulatorConsole
{
    public class ModbusCRC
    {
        public static int CalculateCRC(byte[] data)
        {
            UInt16 crc = 0xffff;

            for (int i = 0; i < data.Length; i++)
            {
                crc ^= data[i];
                for (byte bit_num = 0; bit_num < 8; ++bit_num)
                {
                    crc = (UInt16)((crc & 1) > 0 ? (crc >> 1) ^ 0xa001 : (crc >> 1));
                }
            }

            return crc;
        }

        public static bool IsValidCRC(byte[] data)
        {
            if (data.Length <= 2)
            { 
                // packet can not contain only crc
                return false;
            }

            int packet_crc = data[data.Length - 1] << 8 | data[data.Length - 2];

            UInt16 crc = 0xffff;

            for (int i = 0; i < data.Length - 2; i++)
            {
                crc ^= data[i];
                for (byte bit_num = 0; bit_num < 8; ++bit_num)
                {
                    crc = (UInt16)((crc & 1) > 0 ? (crc >> 1) ^ 0xa001 : (crc >> 1));
                }
            }

            return packet_crc == crc;
        }

        /// <summary>
        /// Return initial Dallas CRC value (0)
        /// </summary>
        /// <returns>Return initial Dallas CRC value (0)</returns>
        public static UInt16 Begin()
        {
            return 0xffff;
        }

        /// <summary>
        /// Update CRC by entering next data byte from the packet
        /// </summary>
        /// <param name="crc">Current CRC value</param>
        /// <param name="data_byte">Next data byte</param>
        /// <returns>CRC</returns>
        public static UInt16 Update(UInt16 crc, byte data_byte)
        {
            crc ^= data_byte;

            for (byte bit_num = 0; bit_num < 8; ++bit_num)
            {
                crc = (ushort)((crc & 1) > 0 ? (crc >> 1) ^ 0xa001 : (crc >> 1));
            }

            return crc;
        }

        /// <summary>
        /// Prepare resulting CRC. (return the same value as entering)
        /// </summary>
        /// <returns>CRC</returns>
        public static UInt16 End(UInt16 crc)
        {
            return crc;
        }
    }
}
