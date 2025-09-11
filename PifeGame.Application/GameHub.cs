using PifeGame.Domain;

namespace PifeGame.Application
{
    public class GameHub
    {
        public List<Game> Rooms = new List<Game>();

        public Guid NewRoom()
        {
            var room = new Game();

            return room.Id;
        }

        public List<Game> ListRooms()
        {
            return Rooms;
        }
    }
}
