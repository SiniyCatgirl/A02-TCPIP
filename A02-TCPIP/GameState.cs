/*
*	FILE	        :   GameState.cs
*	PROJECT         :   A02 - TCP/IP
*   PROGRAMMER      :   Jonathan Paventi, Joshua Visentin, Trent Beitz
*   FIRST VERSION   :   February 10, 20206
*   DESCRIPTION     :   This file contains the logic to construct and set the GameState for the Client that
*                       the server will 
*/

namespace A02_TCPIP {
    internal class GameState {
        private Guid gameID;
        private string currentGameFile;
        private int currentGuessNumber;
        private List<string> listOfGameFiles;
        private List<string> previousGameFiles;
        private List<string> guesses;
        private bool isGameRunning;

        #region Getters & Setters
        public Guid GameID {
            get {
                return gameID;
            }
        }
        public string GetClue {
            get{
                return FileIO.GetStringToGuess(currentGameFile);
            }
        }
        public int GetWordsToGuess {
            get{
                return FileIO.GetAmountOfWords(currentGameFile);
            }
        }
        public int CurrentGuessNumber {
            get{
                return currentGuessNumber;
            } set {
                currentGuessNumber = value;
            }
        }

        public string CurrentGameFile {
            get{
                return currentGameFile;
            }
        }

        public List<string> Guesses {
            get {
                return guesses;
            }
        }

        public bool IsGameRunning {
            get {
                return isGameRunning;
            } set {
                isGameRunning = value;
            }
        }
        #endregion

        // Constructor
        /*
        Constructor   : GameState
        Description   : Initializes a new instance of a GameState Object
        Parameters    : string path    :   the path to the chosen string file
        Return Values : N/A
        */
        public GameState(string path) {
            gameID = Guid.NewGuid();
            currentGameFile = string.Empty;
            previousGameFiles = new List<string>();
            listOfGameFiles = FileIO.GetListOfFiles(path);
            NewGame();

            return;
        }

        /*
        Method        : NewGame
        Description   : resets the values for a new game
        Parameters    : N/A
        Return Values : N/A
        */
        internal void NewGame() {
            guesses = new List<string>();
            isGameRunning = true;
            currentGuessNumber = 0;
            guesses.Clear();
            PickNewFile(currentGameFile);

            return;
        }

        /*
        Method        : AddGuess
        Description   : takes the users input and add it to a list of the guesses
        Parameters    : string guess    :   the string inputted guess
        Return Values : N/A
        */
        internal void AddGuess(string guess) {
            Guesses.Add(guess);

            return;
        }

        /*
        Method        : PickNewFile
        Description   : goes to the directory of the game files and selects a game file it has not already used
        Parameters    : string currentFile  :   the game file tht was just played
        Return Values : N/A
        */
        private void PickNewFile(string currentFile){ 
            if (currentFile != string.Empty) previousGameFiles.Add(currentFile);
            if (previousGameFiles.Count != listOfGameFiles.Count) {     // ensures that there are new games for the user to play
                Random rand = new Random();
                string newFile = string.Empty;

                do {        // iterates through this until the next file that the user has not played is selected
                    int index = rand.Next(0, listOfGameFiles.Count);
                    newFile = listOfGameFiles[index];
                } while(previousGameFiles.Contains(newFile));

                currentGameFile = newFile;
            }

            return;
        }

    }
}
