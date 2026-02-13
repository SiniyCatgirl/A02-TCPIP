/*
*	FILE	        :   FileIO.cs
*	PROJECT         :   A02 - TCP/IP
*   PROGRAMMER      :   Jonathan Paventi, Joshua Visentin, Trent Beitz
*   FIRST VERSION   :   February 10, 20206
*   DESCRIPTION     :   Handels All FileIO
*/

namespace A02_TCPIP {
    internal class FileIO {

        /*
        Method        : GetListOfFiles
        Description   : Retrevies the string files and puts them into a list
        Parameters    : string directory    :   
        Return Values : List<string>        : 
        */
        internal static List<string> GetListOfFiles(string directory){ 
            return Directory.GetFiles(directory, "*.txt").ToList();
        }

        /*
        Method        : GetStringToGuess
        Description   : Pulls the sring of 30 characters from the file and returns the String
        Parameters    : string path     :   where the file wth the string to guess from is located
        Return Values : string          :
        */
        internal static string GetStringToGuess(string path){ 
            string stringGuessWord = string.Empty;
            try {
                stringGuessWord = File.ReadLines(path).FirstOrDefault();
            } catch (Exception e) { 
                Logger.LogMessage($"Error reading file: {e.Message}");
            }

            return stringGuessWord;
        }

        /*
        Method        : GetAmountOfWords
        Description   : pulls the string from the second line of the txt file convers it to a int and sends it back
        Parameters    : string path     :   
        Return Values : int             :
        */
        internal static int GetAmountOfWords(string path){
            int amount = -1;

            try {
                string amountOfWords = File.ReadLines(path).Skip(1).FirstOrDefault();
                int.TryParse(amountOfWords, out amount);
            } catch (Exception e) { 
                Logger.LogMessage($"Error reading file: {e.Message}");
            }

            return amount;
        }

        /*
        Method        : CheckWordList
        Description   : takes the users guess and goes to the txt file and line by line compares the word to all the valid words isted on line 3+
        Parameters    : string path     : the path to the txt file
                        string word     : the users guess
        Return Values : bool            : weather the work is on the valid words list
        */
        internal static bool CheckWordList(string path, string word){
            bool correctGuess = false;

            try {
                string[] listOfWords = File.ReadLines(path).Skip(2).ToArray();
                correctGuess = listOfWords.Contains(word, StringComparer.OrdinalIgnoreCase);
            } catch (Exception e) { 
                Logger.LogMessage($"Error reading file: {e.Message}");
            }
            
            return correctGuess;
        }
    }
}
