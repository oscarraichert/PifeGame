using PifeGame.Domain;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Reflection.Metadata.Ecma335;

namespace PifeGame.Application
{
    public class GameLobbyService
    {
        public List<Game> Rooms = new List<Game>();
        public ConcurrentBag<WebSocket> Connections { get; set; } = new();

        public void JoinLobby(WebSocket socket)
        {
            Connections.Add(socket);
        }

        public async Task NewRoomAsync(SocketMessage message, WebSocket socket)
        {
            var room = new Game();

            room.Connections.Add(socket);

            Rooms.Add(room);

            var response = new SocketMessage { MessageType = MessageType.NewRoom, Payload = $"created room {room.Id}" };

            await WebSocketUtils.Send(response, socket);
        }

        public async Task JoinRoom(string username, Guid roomId, WebSocket socket)
        {
            var room = Rooms.FirstOrDefault(x => x.Id == roomId);

            if (room == null)
            {
                return;
            }

            room.Connections.Add(socket);

            var message = new SocketMessage { MessageType = MessageType.JoinRoom, Payload = $"{username} joined room" };

            await WebSocketUtils.Broadcast(message, room.Connections);
        }

        public async Task LeaveRoom(WebSocket socket)
        {
            var room = Rooms.FirstOrDefault(x => x.Connections.Any(x => x == socket));

            if (room != null)
            {
                room!.Connections.TryTake(out var connection);

                var message = new SocketMessage { MessageType = MessageType.LeaveRoom, Payload = $"{connection} left room" };

                if (room.Connections.Count == 0)
                {
                    Rooms.Remove(room);
                }

                await WebSocketUtils.Broadcast(message, room!.Connections);
            }
        }

        public async Task ChatMessageAsync(SocketMessage message, WebSocket socket)
        {
            var room = Rooms.FirstOrDefault(x => x.Connections.Any(x => x == socket));
            await WebSocketUtils.Broadcast(message, room!.Connections);
        }

        public List<Guid> ListRooms()
        {
            return Rooms.Select(x => x.Id).ToList();
        }
    }
}
