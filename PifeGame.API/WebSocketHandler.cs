using PifeGame.Domain;
using System.Net.WebSockets;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;

namespace PifeGame.API
{
    public class WebSocketHandler
    {
        public async Task HandleAsync(WebSocket socket)
        {
            var buffer = new byte[1024 * 4];

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    try
                    {
                        var message = JsonSerializer.Deserialize<SocketMessage>(json);

                        var response = message.MessageType switch
                        {
                            MessageType.NewRoom => HandleNewRoom(message.Payload),
                            _ => new SocketMessage { MessageType = MessageType.InvalidMessageType, Payload = "Invalid Message Type" },
                        };

                        var responseBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
                        await socket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    catch
                    {
                        var error = Encoding.UTF8.GetBytes("Invalid message");
                        await socket.SendAsync(new ArraySegment<byte>(error), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by the WebSocketHandler", CancellationToken.None);
                }
            }
        }

        public SocketMessage HandleNewRoom(string? payload)
        {
            return new SocketMessage { MessageType = MessageType.NewRoom, Payload = "Handle New Room" };
        }
    }
}
