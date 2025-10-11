namespace PifeGame.Domain
{
    public class Player
    {
        public Player(string username)
        {
            Username = username;
            Hand = new List<Card>();
        }
        public string Username { get; set; }
        public List<Card> Hand { get; set; }
    }
}
