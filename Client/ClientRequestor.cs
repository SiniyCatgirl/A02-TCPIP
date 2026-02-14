/*
*	FILE	        :   ClientRequestor.cs
*	PROJECT         :   A02 - TCP/IP
*   PROGRAMMER      :   Jonathan Paventi, Joshua Visentin, Trent Beitz
*   FIRST VERSION   :   February 10, 2026
*   DESCRIPTION     :   This file contains all the logic to run the client listener. It functions as a disconnected model.
*                       It uses the shared defines which make up the communication protocol between the Server and Client
*                       to determine the actions which it take. 
*/
using SharedDefines;
using System;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client {
    internal class ClientRequestor {
        private GameWindow gm;
        public ClientRequestor(GameWindow window) {
           gm = window;
        }
        /*
        Method        : Listener
        Description   : The listener works as a disconnected model and uses the protocol to
                        determine its requests and the responses.
        Parameters    : CancellationToken ct    :   The token required for the tasks to know
                                                    when and if the cancellation token has been
                                                    cancelled.
        Return Values : Task                    :   As an Async method, it is required to return
                                                    a task. This allows the method to return control
                                                    to its caller.
        */
        internal async Task Listener(CancellationToken ct, string prefix, string sendMsg) {
            //Get info from config file.
            string serverIP = ConfigurationManager.AppSettings["ServerIP"];
            IPAddress.TryParse(serverIP, out IPAddress ipAddress);
            string serverPortStr = ConfigurationManager.AppSettings["ServerPort"];
            int.TryParse(serverPortStr, out int port);
            string clientBufferSize = ConfigurationManager.AppSettings["BufferSize"];
            int.TryParse(clientBufferSize, out int maxBufferSize);

            //Combine prefix & id to send to server.
            string clientID = prefix + gm.GameID.ToString();
            string word = string.Empty;

            try {
                //Establish connection to server & get stream.
                TcpClient client = new TcpClient(serverIP, port);
                NetworkStream stream = client.GetStream();
                Byte[] serverBytes = new byte[maxBufferSize];

                //Send Prefix + ID + Message to server.
                Byte[] idStream = Encoding.ASCII.GetBytes(clientID + sendMsg);
                stream.Write(idStream, 0, (clientID.Length + sendMsg.Length));

                //Wait for server to response & read it.
                int i = await stream.ReadAsync(serverBytes, 0, serverBytes.Length, ct);
                string msg = Encoding.ASCII.GetString(serverBytes, 0, i);
                bool startNewGame = false;

                switch (msg) {
                    //Handle initial ID handshake & game setup.
                    case string s when s.StartsWith(Defines.ID_PREFIX):
                        //Extract ID from server response.
                        string idString = s.Substring(Defines.ID_PREFIX.Length, Defines.GUID_SIZE).Trim();
                        Guid.TryParse(idString, out Guid temp);
                        gm.SetID(temp);

                        //Extract clue and words left from server response.
                        int clueStartIndex = Defines.ID_PREFIX.Length + Defines.GUID_SIZE;
                        string clue = s.Substring(clueStartIndex, Defines.CLUE_SIZE).Trim();
                        string wordsLeftStr = s.Substring(clueStartIndex + Defines.CLUE_SIZE).Trim();
                        int.TryParse(wordsLeftStr, out int wordsLeft);

                        //Update UI with clue and words left.
                        gm.UpdateUI(clue, wordsLeft);

                        break;
                    //Handle guesses and their results.
                    case string s when s.StartsWith(Defines.GUESS_PREFIX): // guesses from client
                        switch (s) {
                            //Handle correct guess.
                            case string t when t.StartsWith(Defines.GUESS_CORRECT_PREFIX):
                                //Extract word from server response and add to correct words list.
                                word = s.Substring(Defines.GUESS_CORRECT_PREFIX.Length).Trim();
                                gm.AddCorrectWord(word);

                                break;
                            //Handle incorrect guess.
                            case string t when t.StartsWith(Defines.GUESS_INCORRECT_PREFIX):
                                //Extract word from server response and add to incorrect words list.
                                word = s.Substring(Defines.GUESS_INCORRECT_PREFIX.Length).Trim();
                                gm.AddIncorrectWord(word);

                                break;
                            //Handle repeat guess.
                            case string t when t.StartsWith(Defines.GUESS_REPEAT_PREFIX):
                                //Extract word from server response and show popup that it was already guessed.
                                word = s.Substring(Defines.GUESS_REPEAT_PREFIX.Length).Trim();
                                gm.ShowPopup("Word already guessed: " + word);

                                break;
                        }

                        break;
                    //Handle game-ending scenarios (timeout, win, restart, end)
                    case string s when s.StartsWith(Defines.GAME_OVER_PREFIX):
                        switch (s) {
                            //Timeout: prompt user to restart or end
                            case string t when t.StartsWith(Defines.GAME_OVER_TIMEOUT_PREFIX):
                                //Prompt user to start new game or end game.
                                startNewGame = gm.PromptYesNo("Out of Time.", "Do you want to start a new game? if no game will close.");

                                //Send new game or end game message to server based on user response.
                                if (startNewGame) {
                                    gm.SendToServer(Defines.GAME_OVER_NEWGAME_PREFIX, string.Empty);
                                } else {
                                    gm.SendToServer(Defines.GAME_OVER_ENDGAME_PREFIX, string.Empty);
                                }

                                break;
                            //New game: reset UI and load new data
                            case string t when t.StartsWith(Defines.GAME_OVER_NEWGAME_PREFIX):
                                //Reset UI for new game.
                                gm.ResetUI();

                                //Extract clue and words left from server response.
                                string newGameData = s.Substring(Defines.GAME_OVER_NEWGAME_PREFIX.Length).Trim();
                                string newClue = newGameData.Substring(0, Defines.CLUE_SIZE).Trim();
                                string newWordsLeftStr = newGameData.Substring(newClue.Length).Trim();
                                int.TryParse(newWordsLeftStr, out int newWordsLeft);

                                //Update UI with new clue and words left.
                                gm.UpdateUI(newClue, newWordsLeft);

                                break;
                            //End game: cancel tasks and close window
                            case string t when t.StartsWith(Defines.GAME_OVER_ENDGAME_PREFIX):
                                //Cancel tasks and close game window.
                                gm.CancelToken();
                                gm.CloseGame();

                                break;
                            //Win: prompt for replay
                            case string t when t.StartsWith(Defines.GAME_OVER_WON_PREFIX):
                                //Prompt user to start new game or end game.
                                startNewGame = gm.PromptYesNo("You Won.", "Do you want to start a new game? if no game will close.");

                                //Send new game or end game message to server based on user response.
                                if (startNewGame) {
                                    gm.SendToServer(Defines.GAME_OVER_NEWGAME_PREFIX, string.Empty);
                                } else {
                                    gm.SendToServer(Defines.GAME_OVER_ENDGAME_PREFIX, string.Empty);
                                }

                                break;
                        }

                        break;
                }

                //Close stream and client connection.
                stream.Close();
            } catch (SocketException se) {
                gm.ShowDebugPopup($"Server Not Running.\n Try Again Later.");
                gm.ToggleButton(true);
            } catch (Exception ex) {
                gm.ShowDebugPopup($"Error: {ex.Message}");
            }

            return;
        }
    }
}
