using System.Threading;
using System.Threading.Tasks;
using System.Configuration;

namespace Client {
    internal class TimeMonitor {
        private bool gameOver;
        private long timeRemaining;
        public TimeMonitor() {
            ResetTimer();
        }
        internal async Task MonitorTime(CancellationToken ct) {
            while (!ct.IsCancellationRequested) {
                //use stopwatch to monitor time and update ui with time remaining.

                //if time runs out, send gameover timeout.


            }
        }
        internal void ResetTimer(){ 
            timeRemaining = long.Parse(ConfigurationManager.AppSettings["GameTimeLimit"]);
            gameOver = false;
        }

        private void UpdateUI() {
            //update the ui with the time remaining.
        }
    }
}
