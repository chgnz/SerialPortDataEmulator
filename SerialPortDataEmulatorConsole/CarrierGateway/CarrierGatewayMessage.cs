using System;

namespace SerialPortDataEmulatorConsole
{
    //public unsafe class CarrierGatewayMessage
    public class CarrierGatewayMessage
    {
        public struct MsgStatus
        {
            public bool PrefixReceived;
            public bool CommandReceived;
            public bool DataSizeReceived;
            public bool DataReceived;
            public bool CrcReceived;
        }

        public struct MsgData
        {
            public byte Prefix;
            public byte Command;
            public UInt16 DataSize;
            public byte[] Payload;

            public int DataBytesReceived;
            public byte Crc;
        }

        public MsgStatus State;
        public MsgData Data;
        
        public CarrierGatewayMessage()
        {
            this.Data.Payload = new byte [256];
            this.Reset();
        }
               
        public bool Receive(byte[] data)
        {
            if (!this.State.PrefixReceived)
            {
                this.Data.Prefix = data[0];

                if (this.Data.Prefix != 0x99)
                {
                    // gaidam sakumu
                    return false;
                }

                this.State.PrefixReceived = true;
                return false;
            }

            if (!this.State.CommandReceived)
            {
                this.Data.Command = data[0];
                this.State.CommandReceived = true;
                return false;
            }

            if (!this.State.DataSizeReceived)
            {
                if (data.Length != 2)
                {
                    Console.WriteLine($"data size mismatch, must be 2, got {data.Length}");
                    this.Reset();
                    return false;
                }

                this.Data.DataSize = BitConverter.ToUInt16(data, 0);
                this.State.DataSizeReceived = true;

                // pārbaudam vai sekos arī paši dati
                this.State.DataReceived = this.Data.DataSize == 0;        
                
                return false;
            }

            if (!this.State.DataReceived)
            {
                if (data.Length != this.Data.DataSize)
                {
                    Console.WriteLine($"payload size mismatch, must be {this.Data.DataSize}, got {data.Length}");
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
                return 1;

            if (!this.State.CommandReceived)
                return 1;

            if (!this.State.DataSizeReceived)
                return 2;

            if (!this.State.DataReceived)
                return this.Data.DataSize;

            if (!this.State.CrcReceived)
                return 1;

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
            crc += this.Data.Command;

            for (int i = 0; i < BitConverter.GetBytes(this.Data.DataSize).Length; i++)
            {
                crc += BitConverter.GetBytes(this.Data.DataSize)[i];
            }

            for (int i = 0; i < this.Data.DataSize; i++)
            {
                crc += this.Data.Payload[i];
            }

            return crc;
        }
    }
}
