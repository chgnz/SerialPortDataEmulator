using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialPortDataEmulatorConsole.Emulators.J1708
{
    interface Ij1708Param
    {
        byte[] GetRawData();
    }

    class j1708_parameter: Ij1708Param
    {
        private UInt16 PID;
        private double multiplier;
        private double offset;
        private string Name;
        private string MeasurmentUnits;
        private double value;

        public j1708_parameter(ushort pID, double multiplier, double offset, string name, string measurmentUnits)
        {
            this.PID = pID;
            this.multiplier = multiplier;
            this.offset = offset;
            this.Name = name;
            this.MeasurmentUnits = measurmentUnits;
        }

        public j1708_parameter SetValue(double value)
        {
            this.value = value;
            return this;
        }

        public byte[] GetRawData()
        {
            int dataSize = 0;

            if (this.PID > 0 && this.PID < 128)
            {
                byte raw_value = (byte)((this.value - this.offset) / (this.multiplier));
                Debug.Assert(raw_value <= 0xff);

                dataSize = 1;
                byte[] result = new byte[1 + dataSize];

                result[0] = (byte)this.PID;
                result[1] = raw_value;

                return result;
            }
            else if (this.PID >= 128 && this.PID < 192)
            {
                ushort raw_value = (ushort)((this.value - this.offset)/( this.multiplier));
                Debug.Assert(raw_value <= 0xffff);

                dataSize = 2;
                byte[] result = new byte[1 + dataSize];

                result[0] = (byte)this.PID;
                result[1] = (byte)((raw_value >> 0) & 0xff);
                result[2] = (byte)((raw_value >> 8) & 0xff);

                return result;
            }
            else
            {
                Debug.Assert(false);
            }

            return new byte[] { 0x00 };
        }
    }

    class Factory
    {
        public j1708_parameter createRoadSpeed()
        {
            return new j1708_parameter(84, 0.805, 0, "Road Speed", "km/h");
        }

        public j1708_parameter createCruiseStatus()
        {
            return new j1708_parameter(85, 1, 0, "Road Speed", "bitmap");
        }

        public j1708_parameter createAcceleratorPedalPosition()
        {
            return new j1708_parameter(91, 0.4, 0, "Accelerator Pedal Position", "%");
        }

        public j1708_parameter createEngineLoad()
        {
            return new j1708_parameter(92, 0.5, 0, "Engine Load", "%");
        }
        public j1708_parameter createFuelLevel()
        {
            return new j1708_parameter(96, 0.5, 0, "Fuel Level", "%");
        }

        public j1708_parameter createRPM()
        {
            return new j1708_parameter(190, 0.25, 0, "Engine Speed (RPM)", "rpm");
        }
    }
}
