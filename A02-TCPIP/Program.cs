/*
*	FILE	        :   Program.cs
*	PROJECT         :   A02 - TCP/IP
*   PROGRAMMER      :   Jonathan Paventi, Joshua Visentin, Trent Beitz
*   FIRST VERSION   :   February 10, 20206
*   DESCRIPTION     :   Creates Cancellation tioken and creates and starts the server listener task
*/

namespace A02_TCPIP {
    class Program {
        private static CancellationTokenSource cts = new CancellationTokenSource();
        internal static Dictionary<Guid, GameState> clients = new Dictionary<Guid, GameState>();

        static async Task Main(string[] args) {
            CancellationToken token = cts.Token;

            ServerListener listener = new ServerListener();
            Task serverListener = listener.Listener(token);

            await Task.WhenAll(serverListener);

            return;
        }
    }
}