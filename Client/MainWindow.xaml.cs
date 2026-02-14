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
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TCP_Client;

namespace Client {
    public partial class GameWindow : Window {
        private int wordsLeft;
        private CancellationTokenSource cts = new CancellationTokenSource();
        internal CancellationTokenSource isRunning = new CancellationTokenSource();
        private Guid clientGameID = Guid.Empty;
        private Task listenerTask;
        private TimeMonitor timeMonitor;
        private Stopwatch sw;
        
        // Getter
        public Guid GameID {
            get{
                return clientGameID;
            }
        }
        
        // Default constructor
        public GameWindow() {
            InitializeComponent();
            ResetUI();
            timeMonitor = new TimeMonitor(this);
            sw = new Stopwatch();

            return;
        }

        /*
        Method        : CancelToken
        Description   : This cancels and disposes of the token, setting the variable to null
        Parameters    : N/A
        Return Values : N/A
        */
        internal void CancelToken() {
            isRunning.Cancel();
            isRunning.Dispose();
            isRunning = null;
            cts.Cancel();
            cts.Dispose();
            cts = null;
        }

        /*
        Method        : CloseGame
        Description   : Closes the game.
        Parameters    : N/A
        Return Values : N/A
        */
        public void CloseGame() {
            Window game = (Application.Current.MainWindow as GameWindow);

            if (game != null) game.Close();

            return;
        }

        /*
        Method        : File_Exit_Click
        Description   : Handles the user clicking exit in the file menu
        Parameters    : Object sender       :   The object related to the action on the WPF
                        RoutedEventArgs e   :   The event triggered by the action
        Return Values : N/A
        */
        private void File_Exit_Click(object sender, RoutedEventArgs e) {
            this.Close();

            return;
        }

        /*
        Method        : Edit_Config_Click
        Description   : Handles the user clicking Config in the Edit menu
        Parameters    : Object sender       :   The object related to the action on the WPF
                        RoutedEventArgs e   :   The event triggered by the action
        Return Values : N/A
        */
        private void Edit_Config_Click(object sender, RoutedEventArgs e) {
            ConfigForm cfg = new ConfigForm();
            cfg.ShowDialog();

            return;
        }

        /*
        Method        : Help_About_Click
        Description   : Handles the user clicking the About under the Help menu
        Parameters    : Object sender       :   The object related to the action on the WPF
                        RoutedEventArgs e   :   The event triggered by the action
        Return Values : N/A
        */
        private void Help_About_Click(object sender, RoutedEventArgs e) {
            AboutWindow about = new AboutWindow();
            about.ShowDialog();

            return;
        }

        /*
        Method        : btnSubmit_Click
        Description   : When the user clicks submit, sends text to server and clears the textbox
        Parameters    : Object sender       :   The object related to the action on the WPF
                        RoutedEventArgs e   :   The event triggered by the action
        Return Values : N/A
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
        Method        : btnStart_Click
        Description   : When the user clicks the start button, it turns the button off, sends an
                        initial communication to the server, and handles any errors that occur
        Parameters    : Object sender       :   The object related to the action on the WPF
                        RoutedEventArgs e   :   The event triggered by the action
        Return Values : N/A
        */
        private void btnStart_Click(object sender, RoutedEventArgs e) {
            // turn off button to prevent player from clicking it again
            try {
                ToggleButton(false);
                SendToServer(Defines.ID_PREFIX, string.Empty);

            } catch (Exception ex) {
                // inform player of an error occurring
                MessageBox.Show($"Failed to start communication: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return;
        }

        /*
        Method        : ToggleButton
        Description   : Toggles the start button on/off in case an error occurs with sending an ID
        Parameters    : N/A
        Return Values : N/A
        */
        internal void ToggleButton(bool state) {
            RunOnUIThread(() => {
                if (btnStart != null) btnStart.IsEnabled = state;
            });

            return;
        }

        /*
        Method        : SendToServer
        Description   : Handles all the communication between the client and the server. Creates a new task every
                        communication that occurs. If an exception occurs when communicating, will cancel the token
                        and inform the user.
        Parameters    : string prefix   :   Contains the prefix used for the server-client communication protocol
                        string msg      :   Contains the message, if relevant, to the server
        Return Values : N/A
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
        Method        : ResetUI
        Description   : Resets the window UI for a new game
        Parameters    : N/A
        Return Values : N/A
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
                isRunning.Cancel();
                isRunning = new CancellationTokenSource();
                timeMonitor = new TimeMonitor(this);
                timeMonitor.MonitorTime(cts.Token, isRunning.Token, sw);
            }

            return;
        }

        /*
        Method        : UpdateUI
        Description   : Updates the UI in the window, specifically the words left and clue
        Parameters    : string clue     :   The string containing the scrambled word string
                        int wordsLeft   :   The number of words contained within clue
        Return Values : N/A
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
        Method        : SetID
        Description   : Stores the ID generated by the Server that is used to identify itself when
                        communicating to the server.
        Parameters    : Guid id     :   The ID that identifies the Client
        Return Values : N/A
        */
        internal void SetID(Guid id) {
            RunOnUIThread(() => {
                clientGameID = id;
                ToggleButton(false);
                timeMonitor.MonitorTime(cts.Token, isRunning.Token, sw);
            });

            return;
        }

        /*
        Method        : UpdateTimer
        Description   : Updates the timer textbox on the UI
        Parameters    : string time     :   Contains the time in string format to print
        Return Values : N/A
        */
        internal void UpdateTimer(string time) {
            if (clientGameID != Guid.Empty) {
                RunOnUIThread(() => {
                    txtTimer.Text = time;
                });
            }

            return;
        }

        /*
        Method        : RunOnUIThread
        Description   : Takes whatever action you give it as a lambda and runs it on the UI thread or
                        hands it off to a dispatcher
        Parameters    : Action action       :   The action used in the lambda call
        Return Values : N/A
        */
        private void RunOnUIThread(Action action) {
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
        Method        : AddCorrectWord
        Description   : Adds the correct word to the relevant textbox on the UI
        Parameters    : string word     :   The word given to it by the Server as correct
        Return Values : N/A
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
        Method        : AddIncorrectWord
        Description   : Adds the incorrect word to the relevant textbox on the UI
        Parameters    : string word     :   The word given to it by the Server as incorrect
        Return Values : N/A
        */
        internal void AddIncorrectWord(string word) {
            RunOnUIThread(() => {
                lbIncorrectWords.Items.Add(word);
            });

            return;
        }

        /*
        Method        : OnClosing
        Description   : This will kill any leftover tasks that were not properly killed by the Client
        Parameters    : CancelEventArgs e   :   Cancels anything still running
        Return Values : N/A
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

        /*
        Method        : ShowPopup
        Description   : Displays a popup for the user to see to inform them of something
        Parameters    : string msd      :   The message to be displayed in the popup
        Return Values : N/A
        */
        internal void ShowPopup(string msg){ 
            RunOnUIThread(() => {
                MessageBox.Show(msg);
            });

            return;
        }

        /*
        Method        : PromptYesNo
        Description   : Prompts the user with a message box that requires a yes or no response
        Parameters    : string caption      :   The title of the message box
                        string msg          :   The content of the message box
        Return Values : bool                :   Returns true if yes and false if no
        */
        internal bool PromptYesNo(string caption, string msg) {
            bool result = false;

            RunOnUIThread(() => {
                result = MessageBox.Show(msg, caption, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
            });

            return result;
        }

        /*
        Method        : ShowDebugPopup
        Description   : Used for debugging purposes to populate message boxes
        Parameters    : string msg      :   The message in the box
        Return Values : N/A
        */
        internal void ShowDebugPopup(string msg){ 
            RunOnUIThread(() => {
                MessageBox.Show(msg, "Debug", MessageBoxButton.OK, MessageBoxImage.Information);
            });

            return;
        }
    }
}
