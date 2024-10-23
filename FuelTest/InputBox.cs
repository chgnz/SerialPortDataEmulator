﻿using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace MsgBox
{
    public static class InputBox
    {
        private static System.Windows.Forms.Form frm = new System.Windows.Forms.Form();
        public static string ResultValue;
        private static DialogResult DialogRes;
        private static string[] buttonTextArray = new string[4];

        public enum Type
        {
            TextBox,
            Nothing
        }

        public enum Buttons
        {
            Ok,
            OkCancel,
            YesNo,
            YesNoCancel
        }

        public enum Language
        {
            English
        }

        /// <summary>
        /// This form is like a MessageBox, but you can select type of controls on it. 
        /// This form returning a DialogResult value.
        /// </summary>
        /// <param name="Message">Message in dialog(as System.String)</param>
        /// <param name="Title">Title of dialog (as System.String)</param>
        /// <param name="icon">Select icon (as InputBox.Icon)</param>
        /// <param name="buttons">Select icon (as InputBox.Buttons)</param>
        /// <param name="type">Type of control in Input box (as InputBox.Type)</param>
        /// <param name="ListItems">Array of ComboBox items (as System.String[])</param>
        /// <param name="FormFont">Font in form (as System.Drawing.Font)</param>
        /// <returns></returns>
        /// 

        public static DialogResult ShowDialog(string Message, string Title = "", Buttons buttons = Buttons.Ok, Type type = Type.Nothing, string default_control_text = "")
        {
            SetLanguage(Language.English);
            frm.Controls.Clear();
            ResultValue = "";
            //Form definition
            frm.MaximizeBox = false;
            frm.MinimizeBox = false;
            frm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            frm.Size = new System.Drawing.Size(350, 170);
            frm.Text = Title;
            frm.ShowIcon = false;
            frm.FormClosing += new System.Windows.Forms.FormClosingEventHandler(frm_FormClosing);
            frm.StartPosition = FormStartPosition.CenterParent;
            //Panel definition
            Panel panel = new Panel();
            panel.Location = new System.Drawing.Point(0, 0);
            panel.Size = new System.Drawing.Size(340, 97);
            panel.BackColor = System.Drawing.Color.White;
            frm.Controls.Add(panel);
            //Add icon in to panel
            //Label definition (message)
            System.Windows.Forms.Label label = new System.Windows.Forms.Label();
            label.Text = Message;
            label.Size = new System.Drawing.Size(245, 60);
            label.Location = new System.Drawing.Point(90, 10);
            label.TextAlign = ContentAlignment.MiddleLeft;
            panel.Controls.Add(label);
            //Add buttons to the form
            foreach (Button btn in Btns(buttons))
                frm.Controls.Add(btn);
            //Add ComboBox or TextBox to the form
            Control ctrl = Cntrl(type);
            ctrl.Text = default_control_text;
            panel.Controls.Add(ctrl);
            //Get automatically cursor to the TextBox
            if (ctrl.Name == "textBox")
            {
                frm.ActiveControl = ctrl;
            }
            frm.ShowDialog();
            //Return text value
            switch (type)
            {
                case Type.Nothing:
                    break;
                default:
                    if (DialogRes == DialogResult.OK || DialogRes == DialogResult.Yes)
                    {
                        ResultValue = ctrl.Text;
                    }
                    else
                    {
                        ResultValue = "";
                    }
                    break;
            }
            return DialogRes;
        }

        private static void button_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            switch (button.Name)
            {
                case "Yes":
                    DialogRes = DialogResult.Yes;
                    break;
                case "No":
                    DialogRes = DialogResult.No;
                    break;
                case "Cancel":
                    DialogRes = DialogResult.Cancel;
                    break;
                default:
                    DialogRes = DialogResult.OK;
                    break;
            }
            frm.Close();
        }

        private static void textBox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                DialogRes = DialogResult.OK;
                frm.Close();
            }
        }

        private static void frm_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            if (DialogRes != null) { }
            else DialogRes = DialogResult.None;
        }

        private static Button[] Btns(Buttons button, Language lang = Language.English)
        {
            //Buttons field for return
            System.Windows.Forms.Button[] returnButtons = new Button[3];
            //Buttons instances
            System.Windows.Forms.Button OkButton = new System.Windows.Forms.Button();
            System.Windows.Forms.Button StornoButton = new System.Windows.Forms.Button();
            System.Windows.Forms.Button AnoButton = new System.Windows.Forms.Button();
            System.Windows.Forms.Button NeButton = new System.Windows.Forms.Button();
            //Set buttons names and text
            OkButton.Text = buttonTextArray[0];
            OkButton.Name = "OK";
            AnoButton.Text = buttonTextArray[1];
            AnoButton.Name = "Yes";
            NeButton.Text = buttonTextArray[2];
            NeButton.Name = "No";
            StornoButton.Text = buttonTextArray[3];
            StornoButton.Name = "Cancel";
            //Set buttons position
            switch (button)
            {
                case Buttons.Ok:
                    OkButton.Location = new System.Drawing.Point(250, 101);
                    returnButtons[0] = OkButton;
                    break;
                case Buttons.OkCancel:
                    OkButton.Location = new System.Drawing.Point(170, 101);
                    returnButtons[0] = OkButton;
                    StornoButton.Location = new System.Drawing.Point(250, 101);
                    returnButtons[1] = StornoButton;
                    break;
                case Buttons.YesNo:
                    AnoButton.Location = new System.Drawing.Point(170, 101);
                    returnButtons[0] = AnoButton;
                    NeButton.Location = new System.Drawing.Point(250, 101);
                    returnButtons[1] = NeButton;
                    break;
                case Buttons.YesNoCancel:
                    AnoButton.Location = new System.Drawing.Point(90, 101);
                    returnButtons[0] = AnoButton;
                    NeButton.Location = new System.Drawing.Point(170, 101);
                    returnButtons[1] = NeButton;
                    StornoButton.Location = new System.Drawing.Point(250, 101);
                    returnButtons[2] = StornoButton;
                    break;
            }
            //Set size and event for all used buttons
            foreach (Button btn in returnButtons)
            {
                if (btn != null)
                {
                    btn.Size = new System.Drawing.Size(75, 23);
                    btn.Click += new System.EventHandler(button_Click);
                }
            }
            return returnButtons;
        }

        private static Control Cntrl(Type type)
        {
            //Textbox
            System.Windows.Forms.TextBox textBox = new System.Windows.Forms.TextBox();
            textBox.Size = new System.Drawing.Size(180, 23);
            textBox.Location = new System.Drawing.Point(90, 70);
            textBox.KeyDown += new System.Windows.Forms.KeyEventHandler(textBox_KeyDown);
            textBox.Name = "textBox";
            //Set returned Control
            Control returnControl = new Control();
            switch (type)
            {
                case Type.TextBox:
                    returnControl = textBox;
                    break;
            }
            return returnControl;
        }

        public static void SetLanguage(Language lang)
        {
            switch (lang)
            {
                default:
                    buttonTextArray = "OK,Yes,No,Cancel".Split(',');
                    break;
            }
        }
    }
}