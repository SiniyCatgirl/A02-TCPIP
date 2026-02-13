/*
*	FILE	        :   ClientRequestor.cs
*	PROJECT         :   A02 - TCP/IP
*   PROGRAMMER      :   Jonathan Paventi, Joshua Visentin, Trent Beitz
*   FIRST VERSION   :   February 10, 20206
*   DESCRIPTION     :   
*/

using SharedDefines;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace Client {
    internal class ClientRequestor {
        private GameWindow gm;
        public ClientRequestor(GameWindow window) {
           gm = window;
        }

        //Should change this to instead of perma listening maybe have a param of what to send then send once and wait for response.
        //there is no need to be listening all the time when server only sends responses.

        /*
        Method        : Listener
        Description   : 
        Parameters    : CancellationToken ct    :   The token required for the tasks to know
                                                    when and if the cancellation token has been
                                                    cancelled.
        Return Values : Task                    :   As an Async method, it is required to return
                                                    a task. This allows the method to return control
                                                    to its caller.
        */
        internal async Task Listener(CancellationToken ct, string prefix, string sendMsg) {
            string serverIP = ConfigurationManager.AppSettings["ServerIP"];
            string serverPortStr = ConfigurationManager.AppSettings["ServerPort"];
            string clientBufferSize = ConfigurationManager.AppSettings["BufferSize"];
            int.TryParse(clientBufferSize, out int maxBufferSize);
            string clientID = prefix + gm.GameID.ToString();
            string word = string.Empty;

            try {
                int port = 0;
                int.TryParse(serverPortStr, out port);
                IPAddress ipAddress = IPAddress.Parse(serverIP);
                TcpClient client = new TcpClient(serverIP, port);

                NetworkStream stream = client.GetStream();
                Byte[] serverBytes = new byte[maxBufferSize];
                
                //This sends to server. we need this.
                Byte[] idStream = Encoding.ASCII.GetBytes(clientID + sendMsg);
                stream.Write(idStream, 0, (clientID.Length + sendMsg.Length));

                int i = await stream.ReadAsync(serverBytes, 0, serverBytes.Length, ct); // 
                string msg = Encoding.ASCII.GetString(serverBytes, 0, i);
                bool startNewGame = false;

                switch (msg) {
                    //parse id and store it for future sends.
                    case string s when s.StartsWith(Defines.ID_PREFIX):
                        string idString = s.Substring(Defines.ID_PREFIX.Length, Defines.GUID_SIZE).Trim();
                        Guid.TryParse(idString, out Guid temp);
                        gm.SetID(temp);
                        //gm.StartCountdown();

                        int clueStartIndex = Defines.ID_PREFIX.Length + Defines.GUID_SIZE;
                        string clue = s.Substring(clueStartIndex, Defines.CLUE_SIZE).Trim();
                        string wordsLeftStr = s.Substring(clueStartIndex + Defines.CLUE_SIZE).Trim();

                        int.TryParse(wordsLeftStr, out int wordsLeft);
                        gm.UpdateUI(clue, wordsLeft);

                        break;
                    case string s when s.StartsWith(Defines.GUESS_PREFIX): // guesses from client
                        switch (s) {
                            case string t when t.StartsWith(Defines.GUESS_CORRECT_PREFIX):
                                word = s.Substring(Defines.GUESS_CORRECT_PREFIX.Length).Trim();
                                gm.AddCorrectWord(word);

                                break;
                            case string t when t.StartsWith(Defines.GUESS_INCORRECT_PREFIX):
                                word = s.Substring(Defines.GUESS_INCORRECT_PREFIX.Length).Trim();
                                gm.AddIncorrectWord(word);

                                break;
                            case string t when t.StartsWith(Defines.GUESS_REPEAT_PREFIX):
                                word = s.Substring(Defines.GUESS_REPEAT_PREFIX.Length).Trim();
                                gm.ShowPopup("Word already guessed: " + word);

                                break;
                        }

                        break;
                    case string s when s.StartsWith(Defines.GAME_OVER_PREFIX): //Could be TIMEOUT, NEWGAME, ENDGAME. win or lose.
                        switch (s) {
                            case string t when t.StartsWith(Defines.GAME_OVER_TIMEOUT_PREFIX):
                                startNewGame = gm.PromptYesNo("Out of Time.", "Do you want to start a new game? if no game will close.");

                                if (startNewGame){
                                    gm.SendToServer(Defines.GAME_OVER_NEWGAME_PREFIX, string.Empty);
                                } else { 
                                    gm.SendToServer(Defines.GAME_OVER_ENDGAME_PREFIX, string.Empty);
                                }

                                    break;
                            case string t when t.StartsWith(Defines.GAME_OVER_NEWGAME_PREFIX):
                                gm.ResetUI();

                                string newGameData = s.Substring(Defines.GAME_OVER_NEWGAME_PREFIX.Length).Trim();

                                string newClue = newGameData.Substring(0, Defines.CLUE_SIZE).Trim();
                                string newWordsLeftStr = newGameData.Substring(newClue.Length).Trim();

                                int.TryParse(newWordsLeftStr, out int newWordsLeft);
                                gm.UpdateUI(newClue, newWordsLeft);

                                break;
                            case string t when t.StartsWith(Defines.GAME_OVER_ENDGAME_PREFIX):
                                gm.CancelToken();

                                break;
                            case string t when t.StartsWith(Defines.GAME_OVER_WON_PREFIX):
                                startNewGame = gm.PromptYesNo("You Won.", "Do you want to start a new game? if no game will close.");

                                if (startNewGame) {
                                    gm.SendToServer(Defines.GAME_OVER_NEWGAME_PREFIX, string.Empty);
                                } else {
                                    gm.SendToServer(Defines.GAME_OVER_ENDGAME_PREFIX, string.Empty);
                                }
                                break;
                        }

                        switch (s) {
                            case Defines.GAME_OVER_NEWGAME_PREFIX:
                                //reset ui and start new game.
                                
                                break;
                            case Defines.GAME_OVER_ENDGAME_PREFIX:
                                //close client and end game.
                                if (gm.GameID != Guid.Empty) {
                                    string endMsg = Defines.GAME_OVER_ENDGAME_PREFIX + gm.GameID.ToString();
                                    byte[] endGame = Encoding.ASCII.GetBytes(endMsg);
                                    await stream.WriteAsync(endGame, 0, endGame.Length, ct);
                                }
                                gm.CloseGame();

                                break;
                            case Defines.GAME_OVER_TIMEOUT_PREFIX:
                                gm.PromptYesNo("TIMEOUT!", "You've run out of time! Start a new Game?");

                                break;
                        }

                        break;
                }

                stream.Close();

            } catch (Exception ex) {
                    gm.ShowDebugPopup(ex.Message);
                
            }

            return;
        }
    }
}
