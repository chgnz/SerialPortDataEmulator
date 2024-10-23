
namespace FuelTest
{
    partial class FuelTestForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.bar_SensorValue = new System.Windows.Forms.TrackBar();
            this.l_FuelSensor = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.checkBox_randomValues = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBox_enable = new System.Windows.Forms.CheckBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.bar_SensorValue)).BeginInit();
            this.SuspendLayout();
            // 
            // bar_SensorValue
            // 
            this.bar_SensorValue.LargeChange = 256;
            this.bar_SensorValue.Location = new System.Drawing.Point(16, 31);
            this.bar_SensorValue.Margin = new System.Windows.Forms.Padding(4);
            this.bar_SensorValue.Maximum = 4096;
            this.bar_SensorValue.Name = "bar_SensorValue";
            this.bar_SensorValue.Size = new System.Drawing.Size(768, 56);
            this.bar_SensorValue.TabIndex = 0;
            this.bar_SensorValue.ValueChanged += new System.EventHandler(this.bar_SensorValue_ValueChanged);
            // 
            // l_FuelSensor
            // 
            this.l_FuelSensor.AutoSize = true;
            this.l_FuelSensor.Location = new System.Drawing.Point(16, 11);
            this.l_FuelSensor.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.l_FuelSensor.Name = "l_FuelSensor";
            this.l_FuelSensor.Size = new System.Drawing.Size(165, 16);
            this.l_FuelSensor.TabIndex = 1;
            this.l_FuelSensor.Text = "Fuel Level Sensor (0-4096)";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // checkBox_randomValues
            // 
            this.checkBox_randomValues.AutoSize = true;
            this.checkBox_randomValues.Location = new System.Drawing.Point(20, 94);
            this.checkBox_randomValues.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_randomValues.Name = "checkBox_randomValues";
            this.checkBox_randomValues.Size = new System.Drawing.Size(183, 20);
            this.checkBox_randomValues.TabIndex = 2;
            this.checkBox_randomValues.Text = "Generate Random values";
            this.toolTip.SetToolTip(this.checkBox_randomValues, "Generates random fuel level values");
            this.checkBox_randomValues.UseVisualStyleBackColor = true;
            this.checkBox_randomValues.CheckedChanged += new System.EventHandler(this.checkBox_randomValues_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Noto Mono", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(186)));
            this.label1.Location = new System.Drawing.Point(693, 80);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 35);
            this.label1.TabIndex = 3;
            this.label1.Text = "0000";
            // 
            // checkBox_enable
            // 
            this.checkBox_enable.AutoSize = true;
            this.checkBox_enable.Location = new System.Drawing.Point(20, 122);
            this.checkBox_enable.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_enable.Name = "checkBox_enable";
            this.checkBox_enable.Size = new System.Drawing.Size(128, 20);
            this.checkBox_enable.TabIndex = 4;
            this.checkBox_enable.Text = "Enable Emulator";
            this.toolTip.SetToolTip(this.checkBox_enable, "Enable Fuel Level data transmitting");
            this.checkBox_enable.UseVisualStyleBackColor = true;
            this.checkBox_enable.CheckedChanged += new System.EventHandler(this.checkBox_enable_CheckedChanged);
            // 
            // toolTip
            // 
            this.toolTip.IsBalloon = true;
            this.toolTip.Popup += new System.Windows.Forms.PopupEventHandler(this.toolTip_Popup);
            // 
            // FuelTestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 287);
            this.Controls.Add(this.checkBox_enable);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.checkBox_randomValues);
            this.Controls.Add(this.l_FuelSensor);
            this.Controls.Add(this.bar_SensorValue);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FuelTestForm";
            this.Text = "Fuel level sensor emulator";
            this.Load += new System.EventHandler(this.FuelTestForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.bar_SensorValue)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TrackBar bar_SensorValue;
        private System.Windows.Forms.Label l_FuelSensor;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.CheckBox checkBox_randomValues;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBox_enable;
        private System.Windows.Forms.ToolTip toolTip;
    }
}

