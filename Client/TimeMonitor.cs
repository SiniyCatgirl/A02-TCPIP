/*
*	FILE	        :   TimeMonitor.cs
*	PROJECT         :   A02 - TCP/IP
*   PROGRAMMER      :   Jonathan Paventi, Joshua Visentin, Trent Beitz
*   FIRST VERSION   :   February 10, 20206
*   DESCRIPTION     :   This file contains all the logic to track the time limit of the game.
*/
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using System.Diagnostics;
using SharedDefines;

namespace Client {
    internal class TimeMonitor {
        private Stopwatch timer;
        private GameWindow gm;

        // Constructor
        public TimeMonitor(GameWindow gameWin) {
            gm = gameWin;
        }

        /*
        Method        : MonitorTime
        Description   : 
        Parameters    : CancellationToken ct    :   The token required for the tasks to know
                                                    when and if the cancellation token has been
                                                    cancelled.
        Return Values : Task                    :   As an Async method, it is required to return
                                                    a task. This allows the method to return control
                                                    to its caller.
        */
        internal async Task MonitorTime(CancellationToken ct, CancellationToken ct2, Stopwatch timer) {
            // Get and parse time limit from AppConfig
            string parseTime = ConfigurationManager.AppSettings["GameTimeLimit"];
            int.TryParse(parseTime, out int targetTime);
            long timeRemaining = long.Parse(ConfigurationManager.AppSettings["GameTimeLimit"]);
            bool isRunning = true;

            gm.UpdateTimer(parseTime);

            timer.Stop();
            timer.Reset();
            timer.Start();

            // while timer has not reached the time limit in the AppConfig
            while (timer.ElapsedMilliseconds < (targetTime * 1000) && !ct.IsCancellationRequested && !ct2.IsCancellationRequested && isRunning) {      // converting targetTime to milliseconds
                await Task.Delay(250);
                if (timeRemaining != (timer.ElapsedMilliseconds - targetTime) / 1000) {
                    timeRemaining = (targetTime - timer.ElapsedMilliseconds / 1000);    // back into seconds for UI
                    gm.UpdateTimer(timeRemaining.ToString());
                }

                // if the timer reaches 0 before the game is finished
                if ((timer.ElapsedMilliseconds / 1000) == targetTime) {
                    gm.SendToServer(Defines.GAME_OVER_TIMEOUT_PREFIX, string.Empty);
                    timer.Stop();
                    timer.Reset();
                    isRunning = false;
                }
            }

            return;
        }
    }
}
