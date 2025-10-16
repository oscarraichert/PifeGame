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
        public GameLobbyService LobbyService { get; }

        public WebSocketHandler(GameLobbyService lobbyService)
        {
            LobbyService = lobbyService;
        }

        public async Task HandleAsync(WebSocket socket, string token, HttpContext context)
        {
            var buffer = new byte[1024 * 4];

            var roomId = context.Request.Query["room_id"].ToString();
            var username = JwtHelper.GetClaims(token).FirstOrDefault(x => x.Type == "username")!.Value;

            if (string.IsNullOrEmpty(roomId))
            {
                await LobbyService.NewRoomAsync(socket, username);
            }
            else
            {
                await LobbyService.JoinRoom(username, roomId, socket);
            }

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    try
                    {
                        var message = JsonSerializer.Deserialize<SocketMessage>(json);

                        _ = message?.MessageType switch
                        {
                            MessageType.ChatMessage => LobbyService.ChatMessageAsync(message, socket),
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
                    var clients = LobbyService.LeaveRoom(socket);

                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by the WebSocketHandler", CancellationToken.None);
                }
            }
        }
    }
}
