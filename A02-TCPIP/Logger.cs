/*
*	FILE	        :   Logger.cs
*	PROJECT         :   A02 - TCP/IP
*   PROGRAMMER      :   Jonathan Paventi, Joshua Visentin, Trent Beitz
*   FIRST VERSION   :   February 10, 20206
*   DESCRIPTION     :   This file contains the logic log any errors and display any critical messages to the server UI
*/

using System.Configuration;

namespace A02_TCPIP {
    internal class Logger {

        /*
        Method        : LogMessage
        Description   : 
        Parameters    : string message     :   message to log.
        Return Values : N/A
        */
        public static void LogMessage(string message) {
            string logPath = ConfigurationManager.AppSettings["LogFile"];

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string line = timestamp + " - " + message + "\n";

            try {
                File.AppendAllText(logPath, line);
            } catch(Exception ex) {
                DisplayMessage("Failed to write to log file: " + ex.Message);
            }

            return;
        }

        /*
        Method        : DisplayMessage
        Description   : 
        Parameters    : string msg      :   message to display to user
        Return Values : N/A
        */
        internal static void DisplayMessage(string msg) {
            Console.WriteLine(msg);
            
            return;
        }
    }
}
