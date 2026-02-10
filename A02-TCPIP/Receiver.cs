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
            string data = Encoding.ASCII.GetString(clientBytes, 0, i);
        
            string dirPath = ConfigurationManager.AppSettings["FileList"];
            string msg = string.Empty;
            string response = string.Empty;
            Byte[] serverBytes = new byte[bufferSize];

            switch (msg) {
                case string s when s.StartsWith("ID: "): //initial contact.
                    Logger.LogMessage($"Received ID: {msg.Substring(4).Trim()}");

                    GameState game = new GameState(dirPath);

                    Program.clients.Add(game.GameID, game);

                    response = "ID: " + game.GameID.ToString();

                    break;
                case string s when s.StartsWith("GUESS: "): // guesses from client
                    string guess = msg.Substring(43).Trim();
                    Logger.LogMessage($"Received GUESS: {guess}");

                    Guid clientGameID = Guid.Empty;
                    Guid.TryParse(guess.Substring(0, 36), out clientGameID);
                    Program.clients.TryGetValue(clientGameID, out GameState clientGame);
                    
                    string guessState = null;
                    if (clientGame.Guesses.Contains(guess)){ 
                        guessState = "REPEAT: ";
                    } else if(FileIO.CheckWordList(clientGame.CurrentGameFile, guess)) {
                        guessState = "CORRECT: ";
                    } else {
                        guessState = "INCORRECT: ";
                    }

                    response = "GUESS " + guessState + guess;

                    break;
                case string s when s.StartsWith("GAMEOVER "): //Could be TIMEOUT, NEWGAME, ENDGAME. win or lose.
                    string gameOverState = msg.Substring(9, 17).Trim();
                    Logger.LogMessage($"Received GAMEOVER: {gameOverState}");

                    Guid gameIDToEnd = Guid.Empty;
                    Guid.TryParse(s.Substring(17), out gameIDToEnd);
                    Program.clients.TryGetValue(gameIDToEnd, out GameState gameToEnd);

                    switch (gameOverState) {
                        case "NEWGAME: ":
                            gameToEnd.NewGame();

                            break;
                        case "ENDGAME: ":
                            Program.clients.Remove(gameIDToEnd);

                            break;
                    }
                    
                    response = "GAMEOVER " + gameOverState;

                    break;
            }

            serverBytes = Encoding.ASCII.GetBytes(response);
            stream.Write(serverBytes, 0, serverBytes.Length);
        }
    }
}
