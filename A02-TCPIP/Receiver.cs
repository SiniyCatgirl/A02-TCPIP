/*
*	FILE	        :   Receiver.cs
*	PROJECT         :   A02 - TCP/IP
*   PROGRAMMER      :   Jonathan Paventi, Joshua Visentin, Trent Beitz
*   FIRST VERSION   :   February 10, 20206
*   DESCRIPTION     :   
*/

using System.Configuration;
using System.Net.Sockets;
using System.Text;
using SharedDefines;

namespace A02_TCPIP {
    internal class Receiver {

        /*
        Method        : StartListener
        Description   : 
        Parameters    : TcpClient client        :
                        CancellationToken ct    :   The token required for the tasks to know
                                                    when and if the cancellation token has been
                                                    cancelled.
        Return Values : Task                    :   As an Async method, it is required to return
                                                    a task. This allows the method to return control
                                                    to its caller.
        */
        internal async Task Worker(TcpClient client, CancellationToken ct){ 
            // read and parse buffer size from appconfig
            string buffer = ConfigurationManager.AppSettings["BufferSize"];
            int.TryParse(buffer, out int bufferSize);

            Byte[] clientBytes = new byte[bufferSize];
            NetworkStream stream = client.GetStream();

            int i = await stream.ReadAsync(clientBytes, 0, clientBytes.Length, ct);
            string msg = Encoding.ASCII.GetString(clientBytes, 0, i);
        
            string dirPath = ConfigurationManager.AppSettings["FileList"];

            string response = string.Empty;
            Byte[] serverBytes = new byte[bufferSize];
            
            Logger.LogMessage($"Received {msg}");

            switch (msg) {
                case string s when s.StartsWith(Defines.ID_PREFIX): //initial contact.
                    GameState game = new GameState(dirPath);

                    Program.clients.Add(game.GameID, game);

                    response = Defines.ID_PREFIX + game.GameID.ToString();

                    break;
                case string s when s.StartsWith(Defines.GUESS_PREFIX): // guesses from client
                    Guid clientGameID = Guid.Empty;
                    Guid.TryParse(s.Substring(Defines.GUESS_PREFIX.Length, (Defines.GUESS_PREFIX.Length + Defines.GUID_SIZE)), out clientGameID);
                    Program.clients.TryGetValue(clientGameID, out GameState clientGame);

                    string guess = msg.Substring(Defines.GUESS_PREFIX.Length + clientGameID.ToString().Length).Trim();

                    string guessState = null;
                    if (clientGame.Guesses.Contains(guess)){ 
                        guessState = Defines.GUESS_REPEAT_PREFIX;
                    } else if(FileIO.CheckWordList(clientGame.CurrentGameFile, guess)) {
                        guessState = Defines.GUESS_CORRECT_PREFIX;
                    } else {
                        guessState = Defines.GUESS_INCORRECT_PREFIX;
                    }

                    response = guessState + guess;

                    break;
                case string s when s.StartsWith(Defines.GAME_OVER_PREFIX): //Could be TIMEOUT, NEWGAME, ENDGAME. win or lose.
                    response = msg.Substring(0, Defines.GAME_OVER_PREFIX.Length).Trim();

                    Guid gameIDToEnd = Guid.Empty;
                    Guid.TryParse(s.Substring(Defines.GAME_OVER_PREFIX.Length), out gameIDToEnd);
                    Program.clients.TryGetValue(gameIDToEnd, out GameState gameToEnd);

                    switch (response) {
                        case Defines.GAME_OVER_NEWGAME_PREFIX:
                            gameToEnd.NewGame();

                            break;
                        case Defines.GAME_OVER_ENDGAME_PREFIX:
                            Program.clients.Remove(gameIDToEnd);

                            break;
                    }

                    break;
            }

            serverBytes = Encoding.ASCII.GetBytes(response);
            stream.Write(serverBytes, 0, serverBytes.Length);

            return;
        }
    }
}
