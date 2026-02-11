using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedDefines {
    public class Defines {
        public const string ID_PREFIX = "ID: ";
        public const string GUESS_PREFIX = "GUESS: ";
        public const string GUESS_CORRECT_PREFIX = GUESS_PREFIX + "CORRECT: ";
        public const string GUESS_INCORRECT_PREFIX = GUESS_PREFIX + "INCORRECT: ";
        public const string GUESS_REPEAT_PREFIX = GUESS_PREFIX + "REPEAT: ";
        public const string GAME_OVER_PREFIX = "GAMEOVER: ";
        public const string GAME_OVER_TIMEOUT_PREFIX = GAME_OVER_PREFIX + "TIMEOUT: ";
        public const string GAME_OVER_NEWGAME_PREFIX = GAME_OVER_PREFIX + "NEWGAME: ";
        public const string GAME_OVER_ENDGAME_PREFIX = GAME_OVER_PREFIX + "ENDGAME: ";
    }
}
