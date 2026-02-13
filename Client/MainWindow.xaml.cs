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
        
        public Guid GameID {
            get{
                return clientGameID;
            }
        }
        
        public GameWindow() {
            InitializeComponent();
            ResetUI();
            
            return;
        }

        public void CloseGame() {
            Window game = (Application.Current.MainWindow as GameWindow);

            if (game != null) game.Close();
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
                if (cts == null) cts = new CancellationTokenSource();

                SendToServer(Defines.ID_PREFIX, string.Empty);
            } catch (Exception ex) {
                // inform player of an error occurring
                MessageBox.Show($"Failed to start communication: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (button == null) button.IsEnabled = true;
            }

            return;
        }

        private async void SendToServer(string prefix, string msg) { 
            ClientRequestor request = new ClientRequestor(this);

            try {
                listenerTask = request.Listener(cts.Token, prefix, msg);

                await Task.Yield();
            } catch (Exception ex) {
                // cancel and get rid of token because it is no longer valid and will make a new one for another attempt
                cts.Cancel();
                cts.Dispose();
                cts = null;
            }

            return;
        }
        private void ResetUI() {
            wordsLeft = -1;
            txtStringClue.Text = string.Empty;
            txtTimer.Text = string.Empty;
            txtGuess.Text = string.Empty;
            txtWordsLeft.Text = string.Empty;
            lbCorrectWords.Items.Clear();
            lbIncorrectWords.Items.Clear();

            return;
        }

        internal void UpdateUI(string clue, int wordsLeft) {
            //If currently on UI thread/task, update controls.
            if (Application.Current.Dispatcher.CheckAccess()) {
                if (this.wordsLeft == -1) this.wordsLeft = wordsLeft;
                txtStringClue.Text = clue;
                //MessageBox.Show(clue + "\n" + txtStringClue.Text);
                txtWordsLeft.Text = wordsLeft.ToString();
                //MessageBox.Show(wordsLeft.ToString() + "\n" + txtWordsLeft.Text);
                //MessageBox.Show("I'm the UI now");
            } else {
                //If not on UI thread / task invoke update with dispatcher.
                Application.Current.Dispatcher.Invoke(() => {
                    if (this.wordsLeft == -1) this.wordsLeft = wordsLeft;   
                    txtStringClue.Text = clue;
                    //MessageBox.Show(clue + "\n" + txtStringClue.Text);
                    txtWordsLeft.Text = wordsLeft.ToString();
                    //MessageBox.Show(wordsLeft.ToString() + "\n" + txtWordsLeft.Text);
                    //MessageBox.Show("Let's let the UI do it");
                });
            }
            //MessageBox.Show("Psych!");

            return;
        }
        internal void SetID(Guid id) {
            //If currently on UI thread/task, update controls.
            if (Application.Current.Dispatcher.CheckAccess()) {
                clientGameID = id;
            } else {
                //If not on UI thread / task invoke update with dispatcher.
                Application.Current.Dispatcher.Invoke(() => {
                    clientGameID = id;
                });
            }

            return;
        }

        internal void UpdateTimer(string time) {
            //If currently on UI thread/task, update controls.
            if(Application.Current.Dispatcher.CheckAccess()) {
                txtTimer.Text = time;
            } else {
                //If not on UI thread/task invoke update with dispatcher.
                Application.Current.Dispatcher.Invoke(() => { 
                    txtTimer.Text = time;
                });
            }

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

            return;
        }

        internal void AddIncorrectWord(string word) {
            //If currently on UI thread/task, update controls.
            if(Application.Current.Dispatcher.CheckAccess()) {
                lbIncorrectWords.Items.Add(word);
            } else {
                //If not on UI thread/task invoke update with dispatcher.
                Application.Current.Dispatcher.Invoke(() => { 
                    lbIncorrectWords.Items.Add(word);
                });
            }

            return;
        }


        protected override void OnClosing(CancelEventArgs e) {      // this function ensures that tasks have been properly terminated and not left running
            
            if (cts != null) {
                cts.Cancel();
                listenerTask.Wait(2000);
                cts.Dispose();
                listenerTask = null;
                cts = null;
            }

            base.OnClosing(e);

            if (Application.Current != null) Application.Current.Shutdown();
        }
        internal void ShowPopup(string msg){ 
            //If currently on UI thread/task, update controls.
            if(Application.Current.Dispatcher.CheckAccess()) {
                MessageBox.Show(msg);
            } else {
                //If not on UI thread/task invoke update with dispatcher.
                Application.Current.Dispatcher.Invoke(() => { 
                    MessageBox.Show(msg);
                });
            }
        }

        internal bool PromptYesNo(string caption, string msg) {
            bool result = false;
            //If currently on UI thread/task, update controls.
            if(Application.Current.Dispatcher.CheckAccess()) {
                result = MessageBox.Show(msg, caption, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
            } else {
                //If not on UI thread/task invoke update with dispatcher.
                Application.Current.Dispatcher.Invoke(() => { 
                    result = MessageBox.Show(msg, caption, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
                });
            }
            return result;
        }

        internal void ShowDebugPopup(string msg){ 
            //If currently on UI thread/task, update controls.
            if(System.Windows.Application.Current.Dispatcher.CheckAccess()) {
                MessageBox.Show(msg);
            } else {
                //If not on UI thread/task invoke update with dispatcher.
                Application.Current.Dispatcher.Invoke(() => { 
                        MessageBox.Show(msg);
                    });
            }
        }


    }
}
