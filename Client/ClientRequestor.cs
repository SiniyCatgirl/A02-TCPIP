/*
*	FILE	        :   ClientRequestor.cs
*	PROJECT         :   A02 - TCP/IP
*   PROGRAMMER      :   Jonathan Paventi, Joshua Visentin, Trent Beitz
*   FIRST VERSION   :   February 10, 20206
*   DESCRIPTION     :   
*/

using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using SharedDefines;

namespace Client {
    internal class ClientRequestor {
        private Guid clientGameID = Guid.Empty;
        private GameWindow gm = new GameWindow();

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
        internal async Task Listener(CancellationToken ct) {
            string serverIP = ConfigurationManager.AppSettings["ServerIP"];
            string serverPortStr = ConfigurationManager.AppSettings["ServerPort"];
            string clientBufferSize = ConfigurationManager.AppSettings["BufferSize"];
            int.TryParse(clientBufferSize, out int maxBufferSize);

            try {       // somewhere in here we need to send the ID Prefix and ID FIRST before anything else happens
                int port = 0;
                int.TryParse(serverPortStr, out port);
                IPAddress ipAddress = IPAddress.Parse(serverIP);
                TcpClient client = new TcpClient(serverIP, port);

                NetworkStream stream = client.GetStream();
                Byte[] serverBytes = new byte[maxBufferSize];


                string clientID = Defines.ID_PREFIX + clientGameID.ToString();
                Byte[] idStream = Encoding.ASCII.GetBytes(clientID);
                stream.Write(serverBytes, 0, clientID.Length);

                int i = await stream.ReadAsync(serverBytes, 0, serverBytes.Length, ct);
                string msg = Encoding.ASCII.GetString(serverBytes, 0, i);

                switch (msg) {
                    case string s when s.StartsWith(Defines.ID_PREFIX): //initial contact.
                        //parse id and store it for future sends.
                        string idString = s.Substring(Defines.ID_PREFIX.Length).Trim();
                        Guid.TryParse(idString, out clientGameID);
                        break;
                    case string s when s.StartsWith(Defines.GUESS_PREFIX): // guesses from client
                        switch (s) {
                            case Defines.GUESS_CORRECT_PREFIX:
                                //add word to correct list and update ui.
                                break;
                            case Defines.GUESS_INCORRECT_PREFIX:
                                //add word to incorrect list and update ui.
                                break;
                            case Defines.GUESS_REPEAT_PREFIX:
                                //popup dialog that says you already guessed this word.
                                break;
                        }

                        break;
                    case string s when s.StartsWith(Defines.GAME_OVER_PREFIX): //Could be TIMEOUT, NEWGAME, ENDGAME. win or lose.
                        switch (s) {
                            case Defines.GAME_OVER_NEWGAME_PREFIX:
                                //reset ui and start new game.
                                
                                break;
                            case Defines.GAME_OVER_ENDGAME_PREFIX:
                                //close client and end game.
                                if (clientGameID != Guid.Empty) {
                                    string endMsg = Defines.GAME_OVER_ENDGAME_PREFIX + clientGameID.ToString();
                                    byte[] endGame = Encoding.ASCII.GetBytes(endMsg);
                                    await stream.WriteAsync(endGame, 0, endGame.Length, ct);
                                }
                                gm.CloseGame();
                                break;
                            case Defines.GAME_OVER_TIMEOUT_PREFIX:
                                //popup dialog that asks if they wanna start new or end game. if new send new, if end close client.
                                break;
                        }

                        break;
                
                }

                stream.Close();

            } catch (Exception ex) {
                
            }

            return;
        }
    }
}
