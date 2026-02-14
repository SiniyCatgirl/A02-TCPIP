/*
*	FILE	        :   Receiver.cs
*	PROJECT         :   A02 - TCP/IP
*   PROGRAMMER      :   Jonathan Paventi, Joshua Visentin, Trent Beitz
*   FIRST VERSION   :   February 10, 20206
*   DESCRIPTION     :   Handles incoming client message over TCP, interprets protocol commands based on custom defines,
*                   :   manages game satate and responds to client with appropriate information based on the message received.
*/

using System.Configuration;
using System.Net.Sockets;
using System.Text;
using SharedDefines;

namespace A02_TCPIP {
    internal class Receiver {

        /*
        Method        : Worker
        Description   : Listens for client messages, processes them according to the defined protocol, 
                      : manages game state, and sends appropriate responses back to the client.
        Parameters    : TcpClient client        :
                        CancellationToken ct    :   The token required for the tasks to know
                                                    when and if the cancellation token has been
                                                    cancelled.
        Return Values : Task                    :   As an Async method, it is required to return
                                                    a task. This allows the method to return control
                                                    to its caller.
        */
        internal async Task Worker(TcpClient client, CancellationToken ct){
            // Get buffer size from config.
            string buffer = ConfigurationManager.AppSettings["BufferSize"];
            int.TryParse(buffer, out int bufferSize);

            // Allocate buffer for incoming client messages.
            Byte[] clientBytes = new byte[bufferSize];
            NetworkStream stream = client.GetStream();

            // Read incoming messages from the client & convert received bytes into ASCII string.
            int i = await stream.ReadAsync(clientBytes, 0, clientBytes.Length, ct);
            string msg = Encoding.ASCII.GetString(clientBytes, 0, i);

            // Get the directory path for game files from config.
            string dirPath = ConfigurationManager.AppSettings["FileList"];

            // Initialize response string and buffer for outgoing messages to client.
            string response = string.Empty;
            Byte[] serverBytes = new byte[bufferSize];

            // Log received messages.
            Console.WriteLine($"Received: {msg}");
            Logger.LogMessage($"Received {msg}");

            // Determine the type of message received and process accordingly based on the defined protocol.
            switch(msg) {
                // Initial contact from client.
                case string s when s.StartsWith(Defines.ID_PREFIX):
                    // Create new game instance for the client.
                    GameState game = new GameState(dirPath);
                    
                    // Log the game file that was opened for the client.
                    Logger.LogMessage($"Opened {dirPath}{game.CurrentGameFile}");
                    Console.WriteLine($"Found the files: {dirPath}{game.CurrentGameFile}");

                    // Add the new game instance to the clients dictionary with the GameID as the key.
                    Program.clients.Add(game.GameID, game);
                    Console.WriteLine("Added the client to the dictionary!");

                    // create response string with client's unique GameID, clue, & number of words to guess.
                    response = Defines.ID_PREFIX + game.GameID.ToString() + game.GetClue + game.GetWordsToGuess;
                    Console.WriteLine($"Finished: {response}");

                    break;
                // Handle guess from client.
                case string s when s.StartsWith(Defines.GUESS_PREFIX):
                    // Extract game ID from the message and retrieve the corresponding game instance from the clients dictionary.
                    Guid clientGameID = Guid.Empty;
                    Guid.TryParse(s.Substring(Defines.GUESS_PREFIX.Length, Defines.GUID_SIZE), out clientGameID);
                    Program.clients.TryGetValue(clientGameID, out GameState clientGame);

                    // skips the prefix of guess and the ID and grabs the string containing the actual guess
                    string guess = msg.Substring(Defines.GUESS_PREFIX.Length + clientGameID.ToString().Length).Trim();
                    guess = guess.ToLower();

                    // Check if guess has already been guessed and add to the list if its a unique guess.
                    bool isRepeat = clientGame.Guesses.Contains(guess);
                    if (!isRepeat) clientGame.AddGuess(guess);

                    // depending on the "guessState" adds one of three prefixes
                    string guessState = null;
                    if (isRepeat){
                        guessState = Defines.GUESS_REPEAT_PREFIX;
                    } else if(FileIO.CheckWordList(clientGame.CurrentGameFile, guess)) {
                        guessState = Defines.GUESS_CORRECT_PREFIX;
                    } else {
                        guessState = Defines.GUESS_INCORRECT_PREFIX;
                    }

                    response = guessState + guess;      // builds response to Client

                    break;
                // Handle game over messages from client.
                case string s when s.StartsWith(Defines.GAME_OVER_PREFIX):
                    response = msg;
                    Guid gameIDToEnd = Guid.Empty;
                    GameState gameEnded = null;

                    // controls what the server does with the GAME_OVER_PREFIX
                    switch (msg) {
                        // parse the guid alert the client that they have run out of time, waits for response
                        case string t when t.StartsWith(Defines.GAME_OVER_TIMEOUT_PREFIX):
                            //Parse id and get game info from dictionary.
                            Guid.TryParse(s.Substring(Defines.GAME_OVER_TIMEOUT_PREFIX.Length), out gameIDToEnd);   
                            Program.clients.TryGetValue(gameIDToEnd, out gameEnded);
                            Logger.LogMessage($"Game ended due to TIMEOUT: {gameEnded.CurrentGameFile}, on Client ID: {gameIDToEnd}");

                            break;
                        // gets the userID, logs the new game request, responds with appropriate prefix to Client
                        case string t when t.StartsWith(Defines.GAME_OVER_NEWGAME_PREFIX):
                            //Parse id and get game info from dictionary.
                            Guid.TryParse(s.Substring(Defines.GAME_OVER_NEWGAME_PREFIX.Length), out gameIDToEnd);
                            Program.clients.TryGetValue(gameIDToEnd, out gameEnded);
                            Logger.LogMessage($"New game requested: {gameEnded.CurrentGameFile}, on Client ID: {gameIDToEnd}");
                            gameEnded.NewGame();    // erases current game statistics (not ID or games played)
                            response = Defines.GAME_OVER_NEWGAME_PREFIX + gameEnded.GetClue + gameEnded.GetWordsToGuess;

                            break;
                        // Handle end game case.
                        case string t when t.StartsWith(Defines.GAME_OVER_ENDGAME_PREFIX):
                            //Parse id and get game info from dictionary.
                            Guid.TryParse(s.Substring(Defines.GAME_OVER_ENDGAME_PREFIX.Length), out gameIDToEnd);
                            Program.clients.TryGetValue(gameIDToEnd, out gameEnded);
                            //Log end of game and remove client from dictionary.
                            Logger.LogMessage($"Game ended: {gameEnded.CurrentGameFile}, on Client ID: {gameIDToEnd}");
                            Program.clients.Remove(gameIDToEnd);

                            break;
                        //Handle game won case.
                        case string t when t.StartsWith(Defines.GAME_OVER_WON_PREFIX):
                            //Parse id and get game info from dictionary.
                            Guid.TryParse(s.Substring(Defines.GAME_OVER_WON_PREFIX.Length), out gameIDToEnd);
                            Program.clients.TryGetValue(gameIDToEnd, out gameEnded);
                            //Log game won.
                            Logger.LogMessage($"Game won: {gameEnded.CurrentGameFile}, on Client ID: {gameIDToEnd}");
                            
                            break;
                    }

                    break;
            }
            
            // Log and send response to client.
            Console.WriteLine("Sent: " + response);
            serverBytes = Encoding.ASCII.GetBytes(response);
            stream.Write(serverBytes, 0, serverBytes.Length);

            return;
        }
    }
}
