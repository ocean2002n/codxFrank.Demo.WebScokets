using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.WebSockets;

namespace codxFrank.Demo.WebScokets.Controllers
{
    [RoutePrefix("api/chat")]
    public class ChatController : ApiController
    {
        private ILogger logger = LogManager.GetCurrentClassLogger();
        private static List<WebSocket> _sockets = new List<WebSocket>();

        [Route]
        [HttpGet]
        public HttpResponseMessage Connect(string nickName)
        {
            HttpContext.Current.AcceptWebSocketRequest(ProcessRequest); //在服務器端接受Web Socket請求，傳入的函數作為Web Socket的處理函數，待Web Socket創建後該函數會被調用，在該函數中可以對Web Socket進行消息收發

            return Request.CreateResponse(HttpStatusCode.SwitchingProtocols); //構造同意切換至Web Socket的Response.
        }

        public async Task ProcessRequest(AspNetWebSocketContext context)
        {
            var socket = context.WebSocket;//傳入的context中有當前的web socket對象
            _sockets.Add(socket);//此處將web socket對象加入一個靜態列表中

            //進入一個無限循環，當web socket close是循環結束
            while (true)
            {
                var buffer = new ArraySegment<byte>(new byte[1024]);
                var receivedResult = await socket.ReceiveAsync(buffer, CancellationToken.None);//對web socket進行異步接收數據
                if (receivedResult.MessageType == WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.Empty, string.Empty, CancellationToken.None);//如果client發起close請求，對client進行ack
                    _sockets.Remove(socket);
                    break;
                }

                if (socket.State == WebSocketState.Open)
                {
                    string recvMsg = Encoding.UTF8.GetString(buffer.Array, 0, receivedResult.Count);
                    var recvBytes = Encoding.UTF8.GetBytes(recvMsg);
                    var sendBuffer = new ArraySegment<byte>(recvBytes);
                    logger.Debug("Recive msg : " + recvMsg);
                    foreach (var innerSocket in _sockets)//當接收到文本消息時，對當前服務器上所有web socket連接進行廣播
                    {
                        if (innerSocket != socket)
                        {
                            await innerSocket.SendAsync(sendBuffer, WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                    }
                }
            }
        }
    }
}
