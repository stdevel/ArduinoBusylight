using System;
using System.Windows.Forms;
using System.IO.Ports;

namespace ArduinoBusylight
{
    public partial class FormSettings : Form
    {
        public FormSettings()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Get _all_ the serial ports!
            string[] ports = SerialPort.GetPortNames();
            foreach(string port in ports)
            {
                cmbPort.Items.Add(port);
            }

            //Pre-select items
            cmbPort.SelectedItem = Properties.Settings.Default.port;
            cmbBaud.SelectedItem = Properties.Settings.Default.baud.ToString();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            //Save settings
            Console.WriteLine("Default port is '{0}', baud rate is '{1}'", cmbPort.SelectedItem, cmbBaud.SelectedItem);
            Properties.Settings.Default.port = cmbPort.SelectedItem.ToString();
            Properties.Settings.Default.baud = Convert.ToInt16(cmbBaud.SelectedItem);
            Properties.Settings.Default.Save();
            this.Close();
        }
    }
}
