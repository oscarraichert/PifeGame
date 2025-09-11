using PifeGame.Application;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace PifeGame.API
{
    public class GameHandler
    {
        public async Task HandleAsync(WebSocket socket)
        {
            var buffer = new byte[1024 * 16];

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    var response = Encoding.UTF8.GetBytes("a");

                    await socket.SendAsync(new ArraySegment<byte>(response), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", CancellationToken.None);
                }
            }
        }
    }
}
