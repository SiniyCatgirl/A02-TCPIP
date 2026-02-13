/*
*	FILE	        :   Defines.cs
*	PROJECT         :   A02 - TCP/IP
*   PROGRAMMER      :   Jonathan Paventi, Joshua Visentin, Trent Beitz
*   FIRST VERSION   :   February 10, 20206
*   DESCRIPTION     :   Lists the defines that are critical to Server and Client logic.
*/

namespace SharedDefines {
    public class Defines {
        public const string ID_PREFIX = "ID: ";
        public const string GUESS_PREFIX = "GUESS: ";
        public const string GUESS_CORRECT_PREFIX = GUESS_PREFIX + "CORRECT: ";
        public const string GUESS_INCORRECT_PREFIX = GUESS_PREFIX + "INCORRECT: ";
        public const string GUESS_REPEAT_PREFIX = GUESS_PREFIX + "REPEAT: ";
        public const string GAME_OVER_PREFIX = "GAMEOVER: ";
        public const string GAME_OVER_WON_PREFIX = GAME_OVER_PREFIX + "GAMEWON: ";
        public const string GAME_OVER_TIMEOUT_PREFIX = GAME_OVER_PREFIX + "TIMEOUT: ";
        public const string GAME_OVER_NEWGAME_PREFIX = GAME_OVER_PREFIX + "NEWGAME: ";
        public const string GAME_OVER_ENDGAME_PREFIX = GAME_OVER_PREFIX + "ENDGAME: ";
        public const int GUID_SIZE = 36; //GUIDs are 36 characters long.
        public const int CLUE_SIZE = 30; //Clue is 30 characters long.
    }
}
