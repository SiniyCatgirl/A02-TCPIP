using SharedDefines;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using TCP_Client;

namespace Client {
    public partial class GameWindow : Window {
        private int wordsLeft;
        private CancellationTokenSource cts;
        private Guid clientGameID = Guid.Empty;
        private Task listenerTask;
        private TimeMonitor stopwatch;
        
        public Guid GameID {
            get{
                return clientGameID;
            }
        }
        
        public GameWindow() {
            InitializeComponent();
            ResetUI();
            stopwatch = new TimeMonitor(this);
            
            return;
        }
        internal void CancelToken() { 
            cts.Cancel();
        }
        public void CloseGame() {
            Window game = (Application.Current.MainWindow as GameWindow);

            if (game != null) game.Close();

            return;
        }
        
        private void File_Exit_Click(object sender, RoutedEventArgs e) {
            this.Close();

            return;
        }

        private void Edit_Config_Click(object sender, RoutedEventArgs e) {
            ConfigForm cfg = new ConfigForm();
            cfg.ShowDialog();

            return;
        }

        private void Help_About_Click(object sender, RoutedEventArgs e) {
            AboutWindow about = new AboutWindow();
            about.ShowDialog();

            return;
        }
        
        private void btnSubmit_Click(object sender, RoutedEventArgs e) {
            string guess = txtGuess.Text.Trim();
            if (!string.IsNullOrEmpty(guess)) {
                SendToServer(Defines.GUESS_PREFIX, guess);
                txtGuess.Text = string.Empty;
            }

            return;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e) {
            // turn off button to prevent player from clicking it again
            Button button = sender as Button;
            try {
                if (button != null) button.IsEnabled = false;
                //if (cts == null) cts = new CancellationTokenSource();

                SendToServer(Defines.ID_PREFIX, string.Empty);
            } catch (Exception ex) {
                // inform player of an error occurring
                MessageBox.Show($"Failed to start communication: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                //if (button == null) button.IsEnabled = true;
            }

            return;
        }

        private void ToggleButton() {
            if (btnStart != null) {
                btnStart.IsEnabled = false;
            } else {
                btnStart.IsEnabled = true;
            }
        }

        internal async void SendToServer(string prefix, string msg) {
            ClientRequestor request = new ClientRequestor(this);

            try {
                listenerTask = request.Listener(cts.Token, prefix, msg);

                await Task.Yield();
            } catch (Exception ex) {
                // cancel and get rid of token because it is no longer valid and will make a new one for another attempt
                cts.Cancel();
                cts.Dispose();
                cts = null;
                MessageBox.Show($"Failed to start communication: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return;
        }

        internal void ResetUI() {
            wordsLeft = -1;
            txtStringClue.Text = string.Empty;
            txtTimer.Text = string.Empty;
            txtGuess.Text = string.Empty;
            txtWordsLeft.Text = string.Empty;
            lbCorrectWords.Items.Clear();
            lbIncorrectWords.Items.Clear();
            //stopwatch.ResetTimer();

            return;
        }

        internal void UpdateUI(string clue, int wordsLeft) {
            RunOnUIThread(() => {
                if (this.wordsLeft == -1) this.wordsLeft = wordsLeft;
                txtStringClue.Text = clue;
                txtWordsLeft.Text = wordsLeft.ToString();
            });

            return;
        }
        internal async void SetID(Guid id) {
            RunOnUIThread(() => {
                clientGameID = id;
                ToggleButton();
            });

            //await stopwatch.MonitorTime(cts.Token);

            return;
        }

        internal void UpdateTimer(string time) {
            RunOnUIThread(() => {
                txtTimer.Text = time;
            });

            return;
        }
        internal void RunOnUIThread(Action action) {
            //If currently on UI thread/task, update controls.
            if(Application.Current.Dispatcher.CheckAccess()) {
                action();
            } else {
                //If not on UI thread/task invoke update with dispatcher.
                Application.Current.Dispatcher.Invoke(() => { 
                    action();
                });
            }

            return;
        }

        internal void AddCorrectWord(string word) {
            RunOnUIThread(() => {
                lbCorrectWords.Items.Add(word);
                UpdateUI(txtStringClue.Text, --wordsLeft);
            });

            if (wordsLeft <= 0) {
                SendToServer(Defines.GAME_OVER_WON_PREFIX, string.Empty);
            }

            return;
        }

        internal void AddIncorrectWord(string word) {
            RunOnUIThread(() => {
                lbIncorrectWords.Items.Add(word);
            });

            return;
        }


        protected override void OnClosing(CancelEventArgs e) {
            if (cts != null) {
                cts.Cancel();
                listenerTask.Wait(2000);
                cts.Dispose();
                listenerTask = null;
                cts = null;
            }

            base.OnClosing(e);

            if (Application.Current != null) Application.Current.Shutdown();

            return;
        }

        internal void ShowPopup(string msg){ 
            RunOnUIThread(() => {
                MessageBox.Show(msg);
            });

            return;
        }

        internal bool PromptYesNo(string caption, string msg) {
            bool result = false;

            RunOnUIThread(() => {
                result = MessageBox.Show(msg, caption, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
            });

            return result;
        }

        internal void ShowDebugPopup(string msg){ 
            RunOnUIThread(() => {
                MessageBox.Show(msg, "Debug", MessageBoxButton.OK, MessageBoxImage.Information);
            });

            return;
        }
    }
}
