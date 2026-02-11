/*
*	FILE	        :   TimeMonitor.cs
*	PROJECT         :   A02 - TCP/IP
*   PROGRAMMER      :   Jonathan Paventi, Joshua Visentin, Trent Beitz
*   FIRST VERSION   :   February 10, 20206
*   DESCRIPTION     :   
*/

using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using System.Diagnostics;

namespace Client {
    internal class TimeMonitor {
        private bool gameOver;
        private long timeRemaining;
        private Stopwatch timer;
        private GameWindow gm = new GameWindow();
        public TimeMonitor() {
            ResetTimer();
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
        internal async Task MonitorTime(CancellationToken ct) {
            timer = new Stopwatch();
            timer.Start();
            string parseTime = ConfigurationManager.AppSettings["GameTimeLimit"];
            int.TryParse(parseTime, out int targetTime);

            while (!ct.IsCancellationRequested) {
                while (timer.ElapsedMilliseconds < (targetTime * 1000)) {
                    Thread.Sleep(100);
                    if (timeRemaining != (timer.ElapsedMilliseconds - targetTime) / 1000) { 
                        timeRemaining = (timer.ElapsedMilliseconds / 1000);
                        gm.UpdateTimer(timeRemaining.ToString());
                    }
                }

                //if time runs out, send gameover timeout.

            }
        }

        /*
        Method        : ResetTime()
        Description   : 
        Parameters    : N/A
        Return Values : N/A
        */
        internal void ResetTimer(){ 
            timeRemaining = long.Parse(ConfigurationManager.AppSettings["GameTimeLimit"]);
            gameOver = false;
            timer.Restart();
            gm.UpdateTimer(timeRemaining.ToString());
        }

        /*
        Method        : UpdateUI()
        Description   : 
        Parameters    : N/A
        Return Values : N/A
        */
        private void UpdateUI() {
            //update the ui with the time remaining.
        }
    }
}
