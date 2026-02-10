/*
*	FILE	        :   ServerListener.cs
*	PROJECT         :   A02 - TCP/IP
*   PROGRAMMER      :   Jonathan Paventi, Joshua Visentin, Trent Beitz
*   FIRST VERSION   :   February 10, 20206
*   DESCRIPTION     :   
*/

using System.Net;
using System.Net.Sockets;
using System.Configuration;

namespace A02_TCPIP {
    internal class ServerListener {

        /*
        Method        : StartListener
        Description   : 
        Parameters    : CancellationToken ct    :   The token required for the tasks to know
                                                    when and if the cancellation token has been
                                                    cancelled.
        Return Values : Task                    :   As an Async method, it is required to return
                                                    a task. This allows the method to return control
                                                    to its caller.
        */
        internal async Task Listener(CancellationToken ct) {
            TcpListener server = null;

            string serverIP = ConfigurationManager.AppSettings["ServerIP"];
            string serverPortStr = ConfigurationManager.AppSettings["ServerPort"];
            string serverBufferSize = ConfigurationManager.AppSettings["BufferSize"];
            int.TryParse(serverBufferSize, out int maxBufferSize);

            try {
                int port = 0;
                int.TryParse(serverPortStr, out port);
                IPAddress localAddress = IPAddress.Parse(serverIP);

                server = new TcpListener(localAddress, port);
                server.Start();

                TcpClient client = new TcpClient();

                while (!ct.IsCancellationRequested) {
                    client = await server.AcceptTcpClientAsync();

                    if (!ct.IsCancellationRequested) {
                        Receiver work = new Receiver();
                        Task worker = work.Worker(client, ct);
                    }
                }
            } catch (Exception ex) {
                Logger.LogMessage($"{ex.Message}");
            } finally {
                server.Stop();
            }

            return;
        }
    }
}
