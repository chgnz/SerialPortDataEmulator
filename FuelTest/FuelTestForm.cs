using FuelTest.Properties;
using MsgBox;
using SerialPortDataEmulatorConsole.SerialProtocols;
using System;
using System.IO.Ports;
using System.Windows.Forms;

namespace FuelTest
{
    public partial class FuelTestForm : Form
    {
        ISerialEmulator emulator;
        SerialPort serialport;

        public FuelTestForm()
        {
            InitializeComponent();

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!checkBox_enable.Checked)
            {
                return;
            }

            emulator.Trigger();
        }

        private void checkBox_randomValues_CheckedChanged(object sender, EventArgs e)
        {
            bool random_values_enabled = checkBox_randomValues.Checked;

            IFuelSensorEmulator em = emulator as IFuelSensorEmulator;
            if (random_values_enabled)
            {
                em.EnableRandomValueMode();
            }
            else
            {
                em.EnableFixedValueMode();
            }

            bar_SensorValue.Enabled = !random_values_enabled; // atsledzam trackbar, ja random vērtības tiek izmantotas
            Settings.Default.GENERATE_RANDOM_VALUES = random_values_enabled;
            Settings.Default.Save();
        }

        private void bar_SensorValue_ValueChanged(object sender, EventArgs e)
        {
            int fuel_level = bar_SensorValue.Value;
            Console.WriteLine($"bar_SensorValue_ValueChanged {fuel_level}");

            IFuelSensorEmulator em = emulator as IFuelSensorEmulator;
            em.SetFixedFuelValue(fuel_level);
            this.label1.Text = fuel_level.ToString();

            Settings.Default.FUEL_LEVEL = fuel_level;
            Settings.Default.Save();
        }

        private void FuelTestForm_Load(object sender, EventArgs e)
        {

            string available_serialports = string.Join(", ", SerialPort.GetPortNames());

            string port = Settings.Default.COMPORT;
            var x = InputBox.ShowDialog(available_serialports, "Enter COM PORT", InputBox.Buttons.Ok, InputBox.Type.TextBox, ((port != null) ? port : ""));

            emulator = new EpsilonESEmulator();

            if (x != DialogResult.OK)
            {
                MessageBox.Show("Please enter COM PORT! Closing software ", "ERROR");
                this.Close();
                return;
            }

            string comport = InputBox.ResultValue;

            try
            {
                serialport = new SerialPort(comport);
                emulator.Init(serialport);
                Settings.Default.COMPORT = comport;
                Settings.Default.Save();
            }
            catch (Exception ex)
            {
                Console.WriteLine("catch #1");
                MessageBox.Show($"Failed to open COM port '{serialport.PortName}' exception: '{ex.Message}'", "ERROR");
                Console.WriteLine("catch #2");
                this.Close();
            }

            checkBox_randomValues.Checked = Settings.Default.GENERATE_RANDOM_VALUES;
            checkBox_enable.Checked = Settings.Default.ENABLE;

            checkBox_randomValues_CheckedChanged(null, null);
            bar_SensorValue_ValueChanged(null, null);
            checkBox_enable_CheckedChanged(null, null);
        }

        private void toolTip_Popup(object sender, PopupEventArgs e)
        {

        }

        private void checkBox_enable_CheckedChanged(object sender, EventArgs e)
        {
            bool emulator_enabled = checkBox_enable.Checked;

            Settings.Default.ENABLE = emulator_enabled;
            Settings.Default.Save();
        }
    }
}
