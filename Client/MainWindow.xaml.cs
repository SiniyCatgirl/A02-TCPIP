using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TCP_Client;
using static System.Net.Mime.MediaTypeNames;

namespace Client {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class GameWindow : Window {
        private int wordsLeft;
        private CancellationTokenSource cts;
        private Task listenerTask;

        public GameWindow() {
            InitializeComponent();
            ResetUI();
            
            return;
        }

        public void CloseGame() {
            Window game = (System.Windows.Application.Current.MainWindow as GameWindow);

            if (game != null) {
                game.Close();
            }
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


            return;
        }

        private async void btnStart_Click(object sender, RoutedEventArgs e) {
            // turn off button to prevent player from clicking it again
            var button = sender as System.Windows.Controls.Button;
            if (button != null) button.IsEnabled = false;

            if (cts == null) {
                cts = new CancellationTokenSource();
                ClientRequestor request = new ClientRequestor();

                try {
                    listenerTask = request.Listener(cts.Token);

                    await Task.Yield();
                } catch (Exception ex) {
                    // inform player of an error occurring
                    MessageBox.Show($"Failed to start communication: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    // cancel and get rid of token because it is no longer valid and will make a new one for another attempt
                    cts.Cancel();
                    cts.Dispose();
                    cts = null;

                    if (button != null) button.IsEnabled = true;    // turn button back on
                }
            }

            return;
        }

        private void ResetUI() {
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
            if(System.Windows.Application.Current.Dispatcher.CheckAccess()) {
                txtStringClue.Text = clue;
                txtWordsLeft.Text = wordsLeft.ToString();
            } else {
                //If not on UI thread/task invoke update with dispatcher.
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new Action(() => { 
                        txtStringClue.Text = clue;
                        txtWordsLeft.Text = wordsLeft.ToString();
                    }));
            }
            
            return;
        }

        internal void UpdateTimer(string time) {
            //If currently on UI thread/task, update controls.
            if(System.Windows.Application.Current.Dispatcher.CheckAccess()) {
                txtTimer.Text = time;
            } else {
                //If not on UI thread/task invoke update with dispatcher.
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new Action(() => { 
                        txtTimer.Text = time;
                    }));
            }

            return;
        }

        internal void AddCorrectWord(string word) {
            //If currently on UI thread/task, update controls.
            if(System.Windows.Application.Current.Dispatcher.CheckAccess()) {
                lbCorrectWords.Items.Add(word);
                wordsLeft--;
            } else {
                //If not on UI thread/task invoke update with dispatcher.
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new Action(() => { 
                        lbCorrectWords.Items.Add(word);
                        wordsLeft--;
                    }));
            }

            return;
        }

        internal void AddIncorrectWord(string word) {
            //If currently on UI thread/task, update controls.
            if(System.Windows.Application.Current.Dispatcher.CheckAccess()) {
                lbIncorrectWords.Items.Add(word);
            } else {
                //If not on UI thread/task invoke update with dispatcher.
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new Action(() => { 
                        lbIncorrectWords.Items.Add(word);
                    }));
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

            if (System.Windows.Application.Current != null) {
                System.Windows.Application.Current.Shutdown();
            }
        }
    }
}
