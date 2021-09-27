using System;

namespace SerialPortDataEmulatorConsole
{
    public class CarrierDirectMessage
    {
        /*	
         *	uint8_t protocol_id;							///< konstante 0x86
         *	uint8_t header_length;							///< headera garums - nemainīgs 0x03
         *	uint8_t command_id;								///< komandas id
         *	uint8_t source;									///< ja tas ir datu pieprasījums, tad "iekārtas id",ja datus saņem - saldētavas id
         *	uint8_t destination;							///< saņēmēja identifikātors, saldētavām hardcoded  0xAA, trešo pušu iekārtām 0x01 - 0x99
         *	uint8_t data_length;							///< datu bloka izmērs
         *	uint8_t data[100];								///< ziņojuma dati
         *	uint8_t checksum;								///< ziņojuma crc
         */

        public struct MsgStatus
        {
            public bool PrefixReceived;
            public bool HeaderReceived;
            public bool DataReceived;
            public bool CrcReceived;
        }

        public struct Header
        {
            public byte HeaderLength;
            public byte CommandId;
            public byte SourceAddr;
            public byte DestinationAddr;
            public byte DataLength;
        }

        public struct MsgData
        {
            public byte Prefix;
            public Header Header; // 5 bytes: header_len, cmd, src_add, dst_addr, data_len
            public int HeaderBytesReceived;
            public byte[] Payload;
            public int DataBytesReceived;
            public byte Crc;
        }

        public MsgStatus State;
        public MsgData Data;
        
        public CarrierDirectMessage()
        {
            this.Data.Payload = new byte [256];
            this.Reset();
        }
               
        public bool Receive(byte[] data)
        {
            if (!this.State.PrefixReceived)
            {
                this.Data.Prefix = data[0];

                if (this.Data.Prefix != 0x86)
                {
                    Console.WriteLine($"incorrect prefix {this.Data.Prefix.ToString("x")}");
                    return false;
                }

                Console.WriteLine($"got prefix {this.Data.Prefix.ToString("x")}");
                this.State.PrefixReceived = true;
                return false;
            }

            if (!this.State.HeaderReceived)
            {
                this.Data.Header.HeaderLength = data[0];
                this.Data.Header.CommandId = data[1];
                this.Data.Header.SourceAddr = data[2];
                this.Data.Header.DestinationAddr = data[3];
                this.Data.Header.DataLength = data[4];

                if (this.Data.Header.HeaderLength != 3)
                {
                    Console.WriteLine($"HeaderLength != 3, got {this.Data.Header.HeaderLength}");
                    this.Reset();
                    return false;
                }

                //Console.WriteLine($"got header");
                
                this.State.HeaderReceived = true;
                this.State.DataReceived = (this.Data.Header.DataLength == 0);
                return false;
            }

            if (!this.State.DataReceived)
            {
                if (data.Length != this.Data.Header.DataLength)
                {
                    Console.WriteLine($"payload size mismatch, must be {this.Data.Header.DataLength}, got {data.Length}");
                    this.Reset();
                    return false;
                }

                if (data.Length > this.Data.Payload.Length)
                {
                    Console.WriteLine($"payload size too big, max allowed {this.Data.Payload.Length}, got {data.Length}");
                    this.Reset();
                    return false;
                }

                Buffer.BlockCopy(data, 0, this.Data.Payload, 0, data.Length);
                this.State.DataReceived = true;
                Console.WriteLine($"got data");
                return false;
            }

            if (!this.State.CrcReceived)
            {
                this.Data.Crc = data[0];
                this.State.CrcReceived = true;

                byte crc = this.CalculateCrc();

                if (crc != this.Data.Crc)
                {
                    Console.WriteLine("CRC error");
                    this.Reset();
                    return false;
                }

                return true;
            }

            return false;
        }

        public int GetRequiredSize()
        {
            if (!this.State.PrefixReceived)
            {
                //Console.WriteLine($"PREFIX GetRequiredSize: {1}");
                return 1;
            }

            if (!this.State.HeaderReceived)
            {
                //Console.WriteLine($"HEADER: GetRequiredSize: {5}");
                return 5;
            }

            if (!this.State.DataReceived)
            {
                //Console.WriteLine($"DATA: GetRequiredSize: {this.Data.Header.DataLength}");
                return this.Data.Header.DataLength;
            }

            if (!this.State.CrcReceived)
            {
               // Console.WriteLine($"CRC: GetRequiredSize: {1}");
                return 1;
            }

            return -1;
        }

        public void Reset()
        {
            this.State = default(MsgStatus);
        }

        private byte CalculateCrc()
        {
            byte crc = 0;

            crc += this.Data.Prefix;
            crc += this.Data.Header.HeaderLength;
            crc += this.Data.Header.CommandId;
            crc += this.Data.Header.SourceAddr;
            crc += this.Data.Header.DestinationAddr;
            crc += this.Data.Header.DataLength;
            
            for (int i = 0; i < this.Data.Header.DataLength; i++)
            {
                crc += this.Data.Payload[i];
            }

            return crc;
        }
    }
}
