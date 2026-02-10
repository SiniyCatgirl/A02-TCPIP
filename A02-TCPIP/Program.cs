/*
*	FILE	        :   Program.cs
*	PROJECT         :   A02 - TCP/IP
*   PROGRAMMER      :   Jonathan Paventi, Joshua Visentin, Trent Beitz
*   FIRST VERSION   :   February 10, 20206
*   DESCRIPTION     :   
*/

using System.Configuration;

namespace A02_TCPIP {
    class Program {
        private static CancellationTokenSource cts = new CancellationTokenSource();
        internal static Dictionary<Guid, GameState> clients = new Dictionary<Guid, GameState>();

        static async Task Main(string[] args) {
            CancellationToken token = cts.Token;
            string dirPath = ConfigurationManager.AppSettings["FileList"];
            List<string> listOfFiles = FileIO.GetListOfFiles(dirPath);

            ServerListener listener = new ServerListener();
            GameState game = new GameState(listOfFiles);
            clients.Add(game.GameID, game);
            Task serverListener = listener.StartListener(token);

            await Task.WhenAll(serverListener);

            return;
        }
    }
}