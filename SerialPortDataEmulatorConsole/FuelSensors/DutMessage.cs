using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialPortDataEmulatorConsole
{
    public class DutMessage
    {
        enum DutCommand
        {
            // kopējie
            full_config = 0x05,     ///< pilna konfigurācija
            filtered_values = 0x06,     ///< filtrētās vērtības
            filter_interval = 0x14,     ///< filtrācijas intervāls
            passwd = 0x15,      ///< uzstādītāja parole
            compile_date = 0x1a,        ///< firmvāres datums
            compile_time = 0x1b,        ///< firmvāres laiks
            ext_config = 0x1e,      ///< papildus konfigurācija
            unfiltered_values = 0x1f,       ///< nefiltrētās vērtības
            work_values = 0x23,     ///< darba parametri
            bounds = 0x24,      ///< vērtību diapazoni
            calibration = 0x26,     ///< kalibrācijas tabula

            // Technoton DUT specifiskie
            dute_serial_number = 0x02,      ///< sērijas numurs
            fw_version = 0x1c,      ///< firmvāres versija

            // Epsilon ES specifiskie
            es_serial_number = 0x42,        ///< sērijas numurs
        };

        public struct MsgStatus
        {
            public bool HeaderReceived;
            public bool AddressReceived;
            public bool CommandReceived;
            public bool CrcReceived;
            public bool DataReceived;
        }

        public struct MsgData
        {
            public byte Header;
            public byte Address;
            public byte Command;
            public byte[] Payload;
            public int DataBytesReceived;
            public byte Crc;
            public int Size;
        }

        public MsgStatus State;
        public MsgData Data;

        public DutMessage()
        {
            this.Reset();
            this.Data.Payload = new byte[256];
        }

        public void Reset()
        {
            State.HeaderReceived = false;
            State.AddressReceived = false;
            State.CommandReceived = false;
            State.DataReceived = false;
            State.CrcReceived = false;
        }

        public int GetPayloadSize()
        {
            return 0;

            //switch ((DutCommand)this.Data.Command)
            //{
            //    // kopējie formāti
            //    case DutCommand.filtered_values: return 5;
            //    case DutCommand.unfiltered_values: return 5;
            //    case DutCommand.full_config: return 18;
            //    case DutCommand.filter_interval: return 1;
            //    case DutCommand.calibration: return 124;
            //    case DutCommand.compile_time: return 16;
            //    case DutCommand.compile_date: return 16;
            //    case DutCommand.fw_version: return 3;
            //    case DutCommand.work_values: return 48;
            //    case DutCommand.ext_config: return 6;
            //    case DutCommand.bounds: return 16;
            //    case DutCommand.passwd: return 8;

            //    // Technoton DUT specifiskie
            //    case DutCommand.dute_serial_number: return 4;

            //    // Epsilon ES specifiskie
            //    case DutCommand.es_serial_number: return 7;

            //    // nezināms formāts
            //    default: return 0;
            //};
        }
    }
}
