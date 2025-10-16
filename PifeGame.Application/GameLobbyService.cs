using PifeGame.Domain;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Reflection.Metadata.Ecma335;

namespace PifeGame.Application
{
    public class GameLobbyService
    {
        public List<Game> Rooms = new List<Game>();

        public async Task NewRoomAsync(WebSocket socket, string username)
        {
            var connected = Rooms.Any(x => x.Connections.Any(x => x.Key == socket));

            if (connected)
            {
                await LeaveRoom(socket);
            }

            var room = new Game();

            room.Connections.TryAdd(socket, username);

            Rooms.Add(room);

            var response = new SocketMessage { MessageType = MessageType.NewRoom, Payload = $"{room.Id}" };

            await WebSocketUtils.Send(response, socket);
        }

        public async Task JoinRoom(string username, string roomId, WebSocket socket)
        {
            var room = Rooms.FirstOrDefault(x => x.Id == Guid.Parse(roomId));

            if (room == null)
            {
                return;
            }

            room.Connections.TryAdd(socket, username);

            var message = new SocketMessage { MessageType = MessageType.JoinRoom, Payload = $"{username} joined room" };

            await WebSocketUtils.Broadcast(message, room.Connections);
        }

        public async Task LeaveRoom(WebSocket socket)
        {
            var room = Rooms.FirstOrDefault(x => x.Connections.Any(x => x.Key == socket));

            if (room != null)
            {
                var removed = room!.Connections.Remove(socket, out var username);

                if (removed)
                {
                    var message = new SocketMessage { MessageType = MessageType.LeaveRoom, Payload = $"{username} left room" };

                    if (room.Connections.Count == 0)
                    {
                        Rooms.Remove(room);
                    }

                    await WebSocketUtils.Broadcast(message, room!.Connections);
                }

            }
        }

        public async Task ChatMessageAsync(SocketMessage message, WebSocket socket)
        {
            var room = Rooms.FirstOrDefault(x => x.Connections.Any(x => x.Key == socket));
            await WebSocketUtils.Broadcast(message, room!.Connections);
        }

        public List<Guid> ListRooms()
        {
            return Rooms.Select(x => x.Id).ToList();
        }
    }
}
