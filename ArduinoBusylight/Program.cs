using System;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
//Skype
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Conversation;

namespace ArduinoBusylight
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new AgentContext());
        }
    }

    //TrayIcon context
    public class AgentContext : ApplicationContext
    {
        //Some variables
        private NotifyIcon trayIcon;
        private SerialPort mySerialPort;
        private LyncClient lyncClient;

        public AgentContext()
        {
            ContextMenu myContext = new ContextMenu();
            myContext.MenuItems.Add("Configure", configure);
            MenuItem manual = new MenuItem("Manual");
            manual.MenuItems.Add("Online/Free", manualOnline);
            manual.MenuItems.Add("Away", manualAway);
            manual.MenuItems.Add("Busy", manualBusy);
            manual.MenuItems.Add("Do not disturb", manualDND);
            manual.MenuItems.Add("Offline", manualOffline);
            manual.MenuItems.Add("-");
            manual.MenuItems.Add("Rrrring!", manualRing);
            myContext.MenuItems.Add(manual);
            myContext.MenuItems.Add("-");
            myContext.MenuItems.Add("Exit", exit);

            //Initialize tray icon
            trayIcon = new NotifyIcon()
            {
                Icon = Properties.Resources.trayicon,
                ContextMenu = myContext,
                Visible = true,
                Text = "Arduino Busylight Agent"
            };

            //Create connection
            createConnection();

            //Connect to Lync/SfB
            try
            {
                lyncClient = LyncClient.GetClient();
                if (lyncClient.Self.Contact != null)
                {
                    //Add event handler for changed availabilities
                    lyncClient.Self.Contact.ContactInformationChanged += Contact_ContactInformationChanged;
                    //Add event handler for incoming conversations
                    lyncClient.ConversationManager.ConversationAdded += ConversationManager_ConversationAdded;
                }
            }
            catch(ClientNotFoundException ex)
            {
                MessageBox.Show("No running Lync or Skype for Business client was found. Your state will not be changed automatically.", "Lync/SfB client not found!");
                Console.WriteLine("Lync/SfB client not found: '{0}'", ex.Message);
            }
            catch (System.Runtime.InteropServices.MarshalDirectiveException ex)
            {
                Console.WriteLine("Unable to read Lync/SfB state: '{0}'", ex.Message);
            }

        }

        private void Contact_ContactInformationChanged(object sender, ContactInformationChangedEventArgs e)
        {
            //Set LED matching Lync/SfB state
            Console.WriteLine("Lync/SfB state changed!");
            ContactAvailability myAvailability = (ContactAvailability)lyncClient.Self.Contact.GetContactInformation(ContactInformationType.Availability);
            controlBusylight(string.Format("kSetLed,{0};", myAvailability.ToString()));
        }

        private void ConversationManager_ConversationAdded(object sender, ConversationManagerEventArgs e)
        {
            //Alert incoming call
            if (e.Conversation != null)
            {
                    if(e.Conversation.Modalities.ContainsKey(ModalityTypes.AudioVideo) &&
                        e.Conversation.Modalities[ModalityTypes.AudioVideo].State != ModalityState.Disconnected)
                    {
                        //Incoming call
                        Console.WriteLine("Incoming call!");
                        controlBusylight("1,kRing;");
                    }
            }
        }

        private void createConnection()
        {
            //(Re-)create connection
            mySerialPort = new SerialPort(Properties.Settings.Default.port, Properties.Settings.Default.baud);
            Console.WriteLine("Created connection to '{0}' with {1} baud",
                Properties.Settings.Default.port, Properties.Settings.Default.baud.ToString());
        }

        private void manualOffline(object sender, EventArgs e)
        {
            controlBusylight("kSetLed,Offline;");
        }

        private void manualRing(object sender, EventArgs e)
        {
            controlBusylight("1,kRing;");
        }

        private void manualDND(object sender, EventArgs e)
        {
            controlBusylight("kSetLed,DoNotDisturb;");
        }

        private void manualBusy(object sender, EventArgs e)
        {
            controlBusylight("kSetLed,Busy;");
        }

        private void manualAway(object sender, EventArgs e)
        {
            controlBusylight("kSetLed,Away;");
        }

        private void manualOnline(object sender, EventArgs e)
        {
            controlBusylight("kSetLed,Free;");
        }

        private void controlBusylight(string command)
        {
            //Control LED behavior
            Console.WriteLine("About to execute LED command: {0}", command);
            try
            {
                mySerialPort.Open();
                mySerialPort.WriteLine(command);
                mySerialPort.Close();
            }
            catch (System.UnauthorizedAccessException ex)
            {
                Console.WriteLine("Console port not accessible ({0}), already in use?", ex.Message);
                MessageBox.Show("Unable to connect to serial port - is the port already in use?", "Unable to connect",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IOException ex)
            {
                Console.WriteLine("Exception: '{0}'", ex.Message);
                MessageBox.Show("Unable to connect to serial port - does the port exist?", "Unable to connect",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void exit(object sender, EventArgs e)
        {
            //Hide tray icon and die in a fire
            trayIcon.Visible = false;
            Application.Exit();
        }

        private void configure(object sender, EventArgs e)
        {
            //Show configure dialog
            FormSettings form_conf = new FormSettings();
            var dialogResult = form_conf.ShowDialog();
            createConnection();
        }
    }
}
