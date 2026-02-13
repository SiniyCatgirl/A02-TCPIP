/*
*	FILE	        :   MainWindow.xaml.cs
*	PROJECT         :   A02 - TCP/IP
*   PROGRAMMER      :   Jonathan Paventi, Joshua Visentin, Trent Beitz
*   FIRST VERSION   :   February 10, 20206
*   DESCRIPTION     :   This window contains the logic for the MainWindow. It maintains the UI Task in order to update independently.
*/

using SharedDefines;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TCP_Client;

namespace Client {
    public partial class GameWindow : Window {
        private int wordsLeft;
        private CancellationTokenSource cts = new CancellationTokenSource();
        private Guid clientGameID = Guid.Empty;
        private Task listenerTask;
        private TimeMonitor timeMonitor;
        private Stopwatch sw;
        
        public Guid GameID {
            get{
                return clientGameID;
            }
        }
        
        public GameWindow() {
            InitializeComponent();
            ResetUI();
            timeMonitor = new TimeMonitor(this);
            sw = new Stopwatch();

            return;
        }

        //Should change this to instead of perma listening maybe have a param of what to send then send once and wait for response.
        //there is no need to be listening all the time when server only sends responses.

        /*
        Method        : 
        Description   : 
        Parameters    : 
        Return Values : 
        */
        internal void CancelToken() { 
            cts.Cancel();
            cts.Dispose();
            cts = null;
        }

        /*
        Method        : 
        Description   : 
        Parameters    : 
        Return Values : 
        */
        public void CloseGame() {
            Window game = (Application.Current.MainWindow as GameWindow);

            if (game != null) game.Close();

            return;
        }

        /*
        Method        : 
        Description   : 
        Parameters    : 
        Return Values : 
        */
        private void File_Exit_Click(object sender, RoutedEventArgs e) {
            this.Close();

            return;
        }

        /*
        Method        : 
        Description   : 
        Parameters    : 
        Return Values : 
        */
        private void Edit_Config_Click(object sender, RoutedEventArgs e) {
            ConfigForm cfg = new ConfigForm();
            cfg.ShowDialog();

            return;
        }

        /*
        Method        : 
        Description   : 
        Parameters    : 
        Return Values : 
        */
        private void Help_About_Click(object sender, RoutedEventArgs e) {
            AboutWindow about = new AboutWindow();
            about.ShowDialog();

            return;
        }

        /*
        Method        : 
        Description   : 
        Parameters    : 
        Return Values : 
        */
        private void btnSubmit_Click(object sender, RoutedEventArgs e) {
            string guess = txtGuess.Text.Trim();
            if (!string.IsNullOrEmpty(guess)) {
                SendToServer(Defines.GUESS_PREFIX, guess);
                txtGuess.Text = string.Empty;
            }

            return;
        }

        /*
        Method        : 
        Description   : 
        Parameters    : 
        Return Values : 
        */
        private void btnStart_Click(object sender, RoutedEventArgs e) {
            // turn off button to prevent player from clicking it again
            Button button = sender as Button;
            try {
                if (button != null) button.IsEnabled = false;
                SendToServer(Defines.ID_PREFIX, string.Empty);
                
            } catch (Exception ex) {
                // inform player of an error occurring
                MessageBox.Show($"Failed to start communication: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return;
        }

        /*
        Method        : 
        Description   : 
        Parameters    : 
        Return Values : 
        */
        private void ToggleButton() {
            if (btnStart != null) {
                btnStart.IsEnabled = false;
            } else {
                btnStart.IsEnabled = true;
            }

            return;
        }

        /*
        Method        : 
        Description   : 
        Parameters    : 
        Return Values : 
        */
        internal async void SendToServer(string prefix, string msg) {
            ClientRequestor request = new ClientRequestor(this);

            try {
                request.Listener(cts.Token, prefix, msg);

                await Task.Yield();
            } catch (Exception ex) {
                // cancel and get rid of token because it is no longer valid and will make a new one for another attempt
                MessageBox.Show($"Failed to start communication: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                CancelToken();
            }

            return;
        }

        /*
        Method        : 
        Description   : 
        Parameters    : 
        Return Values : 
        */
        internal void ResetUI() {
            wordsLeft = -1;
            txtStringClue.Text = string.Empty;
            txtTimer.Text = string.Empty;
            txtGuess.Text = string.Empty;
            txtWordsLeft.Text = string.Empty;
            lbCorrectWords.Items.Clear();
            lbIncorrectWords.Items.Clear();
            if (clientGameID != Guid.Empty) {
                timeMonitor.ResetTimer();
                timeMonitor.MonitorTime(cts.Token, sw);
            }

            return;
        }

        /*
        Method        : 
        Description   : 
        Parameters    : 
        Return Values : 
        */
        internal void UpdateUI(string clue, int wordsLeft) {
            RunOnUIThread(() => {
                if (this.wordsLeft == -1) this.wordsLeft = wordsLeft;
                txtStringClue.Text = clue;
                txtWordsLeft.Text = wordsLeft.ToString();
            });

            return;
        }

        /*
        Method        : 
        Description   : 
        Parameters    : 
        Return Values : 
        */
        internal void SetID(Guid id) {
            RunOnUIThread(() => {
                clientGameID = id;
                ToggleButton();
                timeMonitor.MonitorTime(cts.Token, sw);
                //StartCountdown();
            });

            return;
        }

        /*
        Method        : 
        Description   : 
        Parameters    : 
        Return Values : 
        */
        internal void UpdateTimer(string time) {
            RunOnUIThread(() => {
                txtTimer.Text = time;
            });

            return;
        }

        /*
        Method        : 
        Description   : 
        Parameters    : 
        Return Values : 
        */
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

        /*
        Method        : 
        Description   : 
        Parameters    : 
        Return Values : 
        */
        internal void AddCorrectWord(string word) {
            RunOnUIThread(() => {
                lbCorrectWords.Items.Add(word);
                UpdateUI(txtStringClue.Text, --wordsLeft);
            });

            if (wordsLeft <= 0) {
                ShowPopup("you win");
                SendToServer(Defines.GAME_OVER_WON_PREFIX, string.Empty);
            }

            return;
        }

        /*
        Method        : 
        Description   : 
        Parameters    : 
        Return Values : 
        */
        internal void AddIncorrectWord(string word) {
            RunOnUIThread(() => {
                lbIncorrectWords.Items.Add(word);
            });

            return;
        }

        /*
        Method        : 
        Description   : 
        Parameters    : 
        Return Values : 
        */
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
