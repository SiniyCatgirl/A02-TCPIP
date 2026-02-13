/*
* FILE:             ConfigForm.xaml.cs
* PROJECT:          A02 - TCPIP
* PROGRAMMER:       Josh Visentin, Jonathan Paventi, Trent Beitz
* FIRST VERSION:    February 13, 2026
* DESCRIPTION:      Implements configuration editor window for client application.
*                   Loads current IP/Port settings & updates App.config when saved.
*/
using System;
using System.Configuration;
using System.Windows;

namespace TCP_Client {
    public partial class ConfigForm : Window {
        public event Action SettingsSaved;
        public ConfigForm() {
            InitializeComponent();
            LoadSettings();
            
            return;
        }
        /**
         * FUNCTION: LoadSettings
         * DESCRIPTION:
         * Reads current time limit & server IP & port values from App.config
         * & displays them in text boxes when window opens.
         * PARAMETERS:
         * None.
         * RETURNS:
         * None.
         */
        private void LoadSettings() {
            txtServerIP.Text = ConfigurationManager.AppSettings["ServerIP"] ?? "127.0.0.1";
            txtServerPort.Text = ConfigurationManager.AppSettings["ServerPort"] ?? "13000";
            txtConfigTimeLimit.Text = ConfigurationManager.AppSettings["GameTimeLimit"] ?? "120";
            
            return;
        }
        /**
         * FUNCTION: btnOk_Click
         * DESCRIPTION:
         * Saves updated time limit & server IP & port settings to App.config,
         * raises SettingsSaved event, & closes window.
         * PARAMETERS:
         * object sender: Button that triggered the click.
         * EventArgs e: Event details.
         * RETURNS:
         * None.
         */
        private void btnOk_Click(object sender, EventArgs e) {
            string serverIP = txtServerIP.Text.Trim();
            string serverPort = txtServerPort.Text.Trim();
            string timeLimit = txtConfigTimeLimit.Text.Trim();

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["ServerIP"].Value = serverIP;
            config.AppSettings.Settings["ServerPort"].Value = serverPort;
            config.AppSettings.Settings["GameTimeLimit"].Value = timeLimit;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");

            if(SettingsSaved != null) SettingsSaved();

            this.Close();

            return;
        }
        /**
         * FUNCTION: btnCancel_Click
         * DESCRIPTION:
         * Closes configuration window without saving any changes
         * to client/server IP & port settings.
         * PARAMETERS:
         * object sender: Button that triggered the event.
         * EventArgs e: Event details for click.
         * RETURNS:
         * None.
         */
        private void btnCancel_Click(object sender, EventArgs e) {
            Close();

            return;
        }
    }
}