namespace PifeGame.Domain
{
    public class Player
    {
        public Player(Guid id, string nickname, List<Card> hand)
        {
            Id = id;
            Nickname = nickname;
            Hand = new List<Card>();
        }

        public Guid Id { get; set; }
        public string Nickname { get; set; }
        public List<Card> Hand { get; set; }
    }
}
