using PifeGame.Domain;
using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace PifeGame.Application
{
    public class GameHub
    {
        public List<Game> Rooms = new List<Game>();

        public async Task<Guid> NewRoomAsync(string username, WebSocket socket)
        {
            var currentRoom = Rooms.FirstOrDefault(x => x.Connections.Any(x => x.Value == username));

            if (currentRoom != null)
            {
                var connection = GetConnectionByUsername(username);

                await LeaveRoom(connection.Key);
            }

            var room = new Game();

            room.Players.Add(new Player(username));
            room.Connections.TryAdd(socket, username);

            Rooms.Add(room);

            return room.Id;
        }

        public (bool, ConcurrentDictionary<WebSocket, string>?) JoinRoom(string username, Guid roomId, WebSocket socket)
        {
            var room = Rooms.FirstOrDefault(x => x.Id == roomId);

            if (room == null)
            {
                return (false, null);
            }

            var exists = room.Players.Any(x => x.Username == username);

            if (exists)
            {
                return (false, null);
            }

            room.Players.Add(new Player(username));
            room.Connections.TryAdd(socket, username);

            return (true, room.Connections);
        }

        public async Task LeaveRoom(WebSocket socket)
        {
            var room = Rooms.FirstOrDefault(x => x.Connections.Any(x => x.Key == socket));

            if (room != null)
            {
                room!.Connections.Remove(socket, out var username);
                room.Players.RemoveAll(x => x.Username == username);

                var message = new SocketMessage { MessageType = MessageType.LeaveRoom, Payload = $"{username} left room" };

                if (room.Players.Count == 0)
                {
                    Rooms.Remove(room);
                }

                await WebSocketUtils.Broadcast(message, room!.Connections);
            }
        }

        public KeyValuePair<WebSocket, string> GetConnectionByUsername(string username)
        {
            return Rooms.Select(x => x.Connections.FirstOrDefault(x => x.Value == username)).FirstOrDefault();
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
