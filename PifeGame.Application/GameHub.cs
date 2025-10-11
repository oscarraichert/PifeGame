using PifeGame.Domain;
using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace PifeGame.Application
{
    public class GameHub
    {
        public List<Game> Rooms = new List<Game>();

        public Guid NewRoom(string username, WebSocket socket)
        {
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

        public async Task ChatMessageAsync(SocketMessage message, WebSocket socket)
        {
            var room = Rooms.FirstOrDefault(x => x.Connections.Any(x => x.Key == socket));
            await WebSocketUtils.Broadcast(message, room!.Connections);
        }

        public List<Game> ListRooms()
        {
            return Rooms;
        }
    }
}
