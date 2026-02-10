/*
*	FILE	        :   GameState.cs
*	PROJECT         :   A02 - TCP/IP
*   PROGRAMMER      :   Jonathan Paventi, Joshua Visentin, Trent Beitz
*   FIRST VERSION   :   February 10, 20206
*   DESCRIPTION     :   This file contains the logic to construct and set the GameState for the Client that
*                       the server will 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A02_TCPIP {
    internal class GameState {
        private string gameID;
        private string currentGameFile;
        private int currentGuessNumber;
        private List<string> listOfGameFiles;
        private List<string> previousGameFiles;
        private List<string> guesses;
        private bool isGameRunning;

        #region Getters & Setters
        public string GameID {
            get {
                return gameID;
            }
        }

        public int CurrentGuessNumber {
            get{
                return currentGuessNumber;
            } set {
                currentGuessNumber = value;
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
        public GameState(List<string> listOfFiles) {
            gameID = Guid.NewGuid().ToString();
            currentGameFile = string.Empty;
            previousGameFiles = new List<string>();
            listOfGameFiles = listOfFiles;
            newGame();

            return;
        }

        /*
        Method        : 
        Description   : 
        Parameters    : N/A
        Return Values : N/A
        */
        internal void newGame() {
            guesses = new List<string>();
            isGameRunning = true;
            currentGuessNumber = 0;
            guesses.Clear();
            pickNewFile(currentGameFile);

            return;
        }
        internal void AddGuess(string guess) {
            Guesses.Add(guess);

            return;
        }

        private void pickNewFile(string currentFile){ 
            if (currentFile != string.Empty) previousGameFiles.Add(currentFile);
            if (previousGameFiles.Count != listOfGameFiles.Count) {
                Random rand = new Random();
                string newFile = string.Empty;

                do {
                    int index = rand.Next(0, listOfGameFiles.Count);
                    newFile = listOfGameFiles[index];
                } while(previousGameFiles.Contains(newFile));

                currentGameFile = newFile;
            }

            return;
        }

    }
}
