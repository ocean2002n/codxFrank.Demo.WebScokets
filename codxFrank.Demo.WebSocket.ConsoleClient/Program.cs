using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace codxFrank.Demo.WebSocket.ConsoleClient
{
    class Program
    {
        public static void Main(string[] args)
        {
            Task t = ChatWithServer();
            t.Wait();
        }

        private static async Task ChatWithServer()
        {
            using (var ws = new ClientWebSocket())
            {
                var serverUri = new Uri("ws://localhost:52032/api/WSChat");
                await ws.ConnectAsync(serverUri, CancellationToken.None);
                while (true)
                {
                    Console.Write("Input message ('exit' to exit): ");
                    string msg = Console.ReadLine();
                    if (msg == "exit")
                    {
                        break;
                    }
                    var bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg));

                    await ws.SendAsync(
                        bytesToSend, WebSocketMessageType.Text,
                        true, CancellationToken.None);

                    var bytesReceived = new ArraySegment<byte>(new byte[1024]);
                    var result = await ws.ReceiveAsync(bytesReceived, CancellationToken.None);
                        
                    Console.WriteLine(Encoding.UTF8.GetString(bytesReceived.Array, 0, result.Count));
                        
                    if (ws.State != WebSocketState.Open)
                    {
                        break;
                    }
                }
            }
        }
    }
}
