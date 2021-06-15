using System;

namespace SerialPortDataEmulatorConsole
{
    public class LumikkoMessage
    {
        public struct MsgData
        {
            public byte[] Payload;
            public int Pointer;
            public string Command;
        }

        public MsgData Data;

        public byte[] RawMessage = new byte[256];

        public LumikkoMessage()
        {
            this.Data.Payload = new byte[256];
            this.Reset();
        }

        public bool Receive(byte[] data)
        {
            // vajadzētu būt visu laiku vienam baitam;
            if (data.Length != 1)
            {
                Console.WriteLine($"Receive msg error, too many data: {data.Length}");
            }

            if (Data.Pointer > Data.Payload.Length)
            {
                Console.WriteLine($"no enough free space, reset msg {Data.Pointer}/{data.Length}");
                this.Reset();
            }

            byte rx_byte = data[0];

            if (rx_byte > 0x7f)
            {
                Console.WriteLine($"non ascii char received, ignore {rx_byte:x}");
                return false;
            }

            if (rx_byte == '@')
            {
                // sācies jauns ziņojums 
               // Console.WriteLine($"new msg started");
                this.Reset();
            }

            if (rx_byte == '\n')
            {
               // Console.WriteLine($"end of msg");

                string crc_calculated = CalculateCrc(GetMsgWihtoutCrc()).ToString("000");
                string crc_received = GetCrcFromMsg();

                if (crc_calculated != crc_received)
                {
                    Console.WriteLine($"crc error (, msg '{System.Text.Encoding.ASCII.GetString(this.Data.Payload, 0, this.Data.Pointer)}'): rx: '{crc_received}', calc: '{crc_calculated}'");
                    this.Reset();
                    return false;
                }

                this.Data.Command = GetCommandFromMsg();
                
                return true;
            }

            this.Data.Payload[Data.Pointer++] = rx_byte;

            return false;
        }

        public int GetRequiredSize()
        {
            return 1;
        }

        public void Reset()
        {
            this.Data.Pointer = 0;
        }

        string GetCrcFromMsg()
        {
            string rawAsciiMsg = System.Text.Encoding.ASCII.GetString(this.Data.Payload, 0, this.Data.Pointer);
            int crc_delimiter = rawAsciiMsg.LastIndexOf(',') + 1;
            return rawAsciiMsg.Substring(crc_delimiter);
        }

        byte[] GetMsgWihtoutCrc()
        {
            string rawAsciiMsg = System.Text.Encoding.ASCII.GetString(this.Data.Payload, 0, this.Data.Pointer);
            int crc_delimiter_pos = rawAsciiMsg.LastIndexOf(',') + 1;

            byte[] result = new byte[crc_delimiter_pos];

            Buffer.BlockCopy(this.Data.Payload, 0, result, 0, crc_delimiter_pos);
            return result;
        }

        string GetCommandFromMsg()
        {
            string rawAsciiMsg = System.Text.Encoding.ASCII.GetString(this.Data.Payload, 0, this.Data.Pointer);
            int first_index = rawAsciiMsg.IndexOf(',');
            
            // uzreiz arī atmetam ziņojuma prefix'u: '@', tāpēc sākums ir 1 nevis 0, un garums ir par vienu mazāks 
            return rawAsciiMsg.Substring(1, first_index - 1);
        }

        public static byte CalculateCrc(byte[] data)
        {
            byte crc_calculated = 0;

            foreach (byte b in data)
            {
                crc_calculated += b;
            }

            return crc_calculated;
        }
    }
}
