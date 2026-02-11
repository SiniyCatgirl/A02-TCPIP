using System;
using System.Diagnostics;
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
        public GameWindow() {
            InitializeComponent();
            ResetUI();
            
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


            return;
        }
        private void btnStart_Click(object sender, RoutedEventArgs e) {


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
    }
}
