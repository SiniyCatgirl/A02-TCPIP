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
using SharedDefines;

namespace Client {
    internal class TimeMonitor {
        private bool gameOver;
        private long timeRemaining;
        private Stopwatch timer;
        private GameWindow gm;
        public TimeMonitor(GameWindow gameWin) {
            gm = gameWin;
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
        internal async Task MonitorTime(CancellationToken ct, Stopwatch timer) {
            string parseTime = ConfigurationManager.AppSettings["GameTimeLimit"];
            int.TryParse(parseTime, out int targetTime);
            bool isRunning = true;
            gm.RunOnUIThread(() => {
                gm.txtTimer.Text = parseTime;
            });
            if (!timer.IsRunning) {
                timer.Start();

                while (timer.ElapsedMilliseconds < (targetTime * 1000) && isRunning) {
                    await Task.Delay(250);
                    if (timeRemaining != (timer.ElapsedMilliseconds - targetTime) / 1000) {
                        timeRemaining = (targetTime - timer.ElapsedMilliseconds / 1000);
                        gm.RunOnUIThread(() => {
                            gm.txtTimer.Text = timeRemaining.ToString();
                        });
                    }

                    if ((timer.ElapsedMilliseconds / 1000) == targetTime) {
                        gm.SendToServer(Defines.GAME_OVER_TIMEOUT_PREFIX, string.Empty);
                        timer.Stop();
                        timer.Reset();
                        isRunning = false;
                    }
                }
            }

            return;
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
            if (timer != null) timer.Reset();
            gm.UpdateTimer(timeRemaining.ToString());
        }
    }
}
