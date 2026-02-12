/*
*	FILE	        :   FileIO.cs
*	PROJECT         :   A02 - TCP/IP
*   PROGRAMMER      :   Jonathan Paventi, Joshua Visentin, Trent Beitz
*   FIRST VERSION   :   February 10, 20206
*   DESCRIPTION     :   
*/

namespace A02_TCPIP {
    internal class FileIO {

        /*
        Method        : GetListOfFiles
        Description   : 
        Parameters    : string directory    :   
        Return Values : List<string>        : 
        */
        internal static List<string> GetListOfFiles(string directory){ 
            return Directory.GetFiles(directory, "*.txt").ToList();
        }

        /*
        Method        : GetStringToGuess
        Description   : 
        Parameters    : string path     :   
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
        Description   : 
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
        Description   : 
        Parameters    : string path     : 
                        string word     : 
                        GameState game  : 
        Return Values : bool            :
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
