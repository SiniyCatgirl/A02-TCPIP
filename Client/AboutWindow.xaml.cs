/*
* FILE:             AboutWindow.xaml.cs
* PROJECT:          A02 - TCPIP
* PROGRAMMER:       Josh Visentin, Jonathan Paventi
* FIRST VERSION:    February 13, 2026
* DESCRIPTION:      Code for About dialog window. Displays application info
*                   & provides OK button for closing dialog.
*/
using System.Windows;

namespace TCP_Client{
    public partial class AboutWindow : Window{
        /**
         * FUNCTION: AboutWindow (Constructor)
         * DESCRIPTION:
         * Initializes About window components.
         * PARAMETERS:
         * None.
         * RETURNS:
         * None.
         */
        public AboutWindow(){
            InitializeComponent();

            return;
        }
        /**
         * FUNCTION: Ok_Click
         * DESCRIPTION:
         * Handles OK button click. Closes About dialog.
         * PARAMETERS:
         * object sender: Button clicked.
         * RoutedEventArgs e: Event arguments.
         * RETURNS:
         * None.
         */
        private void Ok_Click(object sender, RoutedEventArgs e){
            this.Close();

            return;
        }
    }
}
