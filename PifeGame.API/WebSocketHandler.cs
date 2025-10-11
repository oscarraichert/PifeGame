using PifeGame.Application;
using PifeGame.Domain;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PifeGame.API
{
    public class WebSocketHandler
    {
        public GameHub Hub { get; }

        public WebSocketHandler(GameHub hub)
        {
            Hub = hub;
        }

        public async Task HandleAsync(WebSocket socket, string token, HttpContext context)
        {
            var buffer = new byte[1024 * 4];

            while (socket.State == WebSocketState.Open)
            {
                var roomId = context.Request.Query["room_id"].ToString();

                if (string.IsNullOrEmpty(roomId))
                {
                    var response = HandleNewRoom(token, socket);

                    var responseBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
                    await socket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                else
                {
                    await HandleJoinRoom(token, roomId, socket);
                }

                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    try
                    {
                        var message = JsonSerializer.Deserialize<SocketMessage>(json);

                        _ = message?.MessageType switch
                        {
                            MessageType.ChatMessage => Hub.ChatMessageAsync(message, socket),
                            _ => Task.CompletedTask,
                        };
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

        public SocketMessage HandleNewRoom(string jwt, WebSocket socket)
        {
            var username = JwtHelper.GetClaims(jwt).FirstOrDefault(x => x.Type == "username")!.Value;

            var roomId = Hub.NewRoom(username, socket);
            return new SocketMessage { MessageType = MessageType.NewRoom, Payload = $"created room {roomId}" };
        }

        public async Task HandleJoinRoom(string jwt, string roomId, WebSocket socket)
        {
            var username = JwtHelper.GetClaims(jwt).FirstOrDefault(x => x.Type == "username")!.Value;

            var result = Hub.JoinRoom(username, Guid.Parse(roomId), socket);

            if (result.Item1)
            {
                var message = new SocketMessage { MessageType = MessageType.NewRoom, Payload = $"joined room {roomId}" };
                await WebSocketUtils.Broadcast(message, result.Item2!);
            }
        }
    }
}
