using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharedDefines;

namespace Client {
    internal class ClientListener {
        //Should change this to instead of perma listening maybe have a param of what to send then send once and wait for response.
        //there is no need to be listening all the time when server only sends responses.
        internal async Task Listener(CancellationToken ct) {
            TcpListener clientListener = null;

            string serverIP = ConfigurationManager.AppSettings["ServerIP"];
            string serverPortStr = ConfigurationManager.AppSettings["ServerPort"];
            string clientBufferSize = ConfigurationManager.AppSettings["BufferSize"];
            int.TryParse(clientBufferSize, out int maxBufferSize);

            try {
                int port = 0;
                int.TryParse(serverPortStr, out port);
                IPAddress localAddress = IPAddress.Parse(serverIP);

                clientListener = new TcpListener(localAddress, port);
                clientListener.Start();

                TcpClient client = new TcpClient();

                while (!ct.IsCancellationRequested) {
                    client = await clientListener.AcceptTcpClientAsync();

                    if (!ct.IsCancellationRequested) {
                        NetworkStream stream = client.GetStream();
                        Byte[] serverBytes = new byte[maxBufferSize];

                        int i = await stream.ReadAsync(serverBytes, 0, serverBytes.Length, ct);
                        string msg = Encoding.ASCII.GetString(serverBytes, 0, i);

                        switch (msg) {
                            case string s when s.StartsWith(Defines.ID_PREFIX): //initial contact.
                                //parse id and store it for future sends.
                                break;
                            case string s when s.StartsWith(Defines.GUESS_PREFIX): // guesses from client
                                switch (s) {
                                    case Defines.GUESS_CORRECT_PREFIX:
                                        //add word to correct list and update ui.
                                        break;
                                    case Defines.GUESS_INCORRECT_PREFIX:
                                        //add word to incorrect list and update ui.
                                        break;
                                    case Defines.GUESS_REPEAT_PREFIX:
                                        //popup dialog that says you already guessed this word.
                                        break;
                                }

                                break;
                            case string s when s.StartsWith(Defines.GAME_OVER_PREFIX): //Could be TIMEOUT, NEWGAME, ENDGAME. win or lose.
                                switch (s) {
                                    case Defines.GAME_OVER_NEWGAME_PREFIX:
                                        //reset ui and start new game.
                                        break;
                                    case Defines.GAME_OVER_ENDGAME_PREFIX:
                                        //close client and end game.
                                        break;
                                    case Defines.GAME_OVER_TIMEOUT_PREFIX:
                                        //popup dialog that asks if they wanna start new or end game. if new send new, if end close client.
                                        break;
                                }

                                break;
                        }

                        return;
                    }
                }
            }
        }
    }
}
