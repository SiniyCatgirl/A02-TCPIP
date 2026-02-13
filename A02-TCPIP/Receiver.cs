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

            Console.WriteLine($"Received: {msg}");
            Logger.LogMessage($"Received {msg}");

            switch (msg) {
                case string s when s.StartsWith(Defines.ID_PREFIX): //initial contact.
                    //Console.WriteLine("entered thing");

                    GameState game = new GameState(dirPath);
                    Logger.LogMessage($"Opened {game.CurrentGameFile}");
                    //Console.WriteLine($"Found the files: {dirPath}{game.CurrentGameFile}");

                    Program.clients.Add(game.GameID, game);
                    //Console.WriteLine("Added the client to the dictionary!");

                    response = Defines.ID_PREFIX + game.GameID.ToString() + game.GetClue + game.GetWordsToGuess;
                    //Console.WriteLine($"Finished: {response}");

                    break;
                case string s when s.StartsWith(Defines.GUESS_PREFIX): // guesses from client
                    Guid clientGameID = Guid.Empty;
                    Guid.TryParse(s.Substring(Defines.GUESS_PREFIX.Length, Defines.GUID_SIZE), out clientGameID);
                    Program.clients.TryGetValue(clientGameID, out GameState clientGame);

                    string guess = msg.Substring(Defines.GUESS_PREFIX.Length + clientGameID.ToString().Length).Trim();
                    bool isRepeat = clientGame.Guesses.Contains(guess);

                    if (!isRepeat) clientGame.AddGuess(guess);

                    string guessState = null;
                    if (isRepeat){
                        guessState = Defines.GUESS_REPEAT_PREFIX;
                    } else if(FileIO.CheckWordList(clientGame.CurrentGameFile, guess)) {
                        guessState = Defines.GUESS_CORRECT_PREFIX;
                    } else {
                        guessState = Defines.GUESS_INCORRECT_PREFIX;
                    }

                    response = guessState + guess;

                    break;
                case string s when s.StartsWith(Defines.GAME_OVER_PREFIX): //Could be TIMEOUT, NEWGAME, ENDGAME. win or lose.
                    response = msg;

                    Guid gameIDToEnd = Guid.Empty;
                    Guid.TryParse(s.Substring(Defines.GAME_OVER_PREFIX.Length), out gameIDToEnd);
                    Program.clients.TryGetValue(gameIDToEnd, out GameState gameEnded);

                    switch (s) {
                        case string t when t.StartsWith(Defines.GAME_OVER_TIMEOUT_PREFIX):
                            Logger.LogMessage($"Game ended due to TIMEOUT: {gameEnded.CurrentGameFile}, on Client ID: {gameIDToEnd}");

                            break;
                        case string t when t.StartsWith(Defines.GAME_OVER_NEWGAME_PREFIX):
                            Logger.LogMessage($"New game requested: {gameEnded.CurrentGameFile}, on Client ID: {gameIDToEnd}");
                            gameEnded.NewGame();
                            response = Defines.GAME_OVER_NEWGAME_PREFIX + gameEnded.GetClue + gameEnded.GetWordsToGuess;

                            break;
                        case string t when t.StartsWith(Defines.GAME_OVER_ENDGAME_PREFIX):
                            Logger.LogMessage($"Game ended: {gameEnded.CurrentGameFile}, on Client ID: {gameIDToEnd}");
                            Program.clients.Remove(gameIDToEnd);

                            break;
                        case string t when t.StartsWith(Defines.GAME_OVER_WON_PREFIX):
                            Logger.LogMessage($"Game won: {gameEnded.CurrentGameFile}, on Client ID: {gameIDToEnd}");
                            Program.clients.Remove(gameIDToEnd);
                            
                            break;
                    }

                    break;
            }
            
            Console.WriteLine("Sent: " + response);
            serverBytes = Encoding.ASCII.GetBytes(response);
            stream.Write(serverBytes, 0, serverBytes.Length);

            return;
        }
    }
}
