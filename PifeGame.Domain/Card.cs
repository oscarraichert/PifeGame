namespace PifeGame.Domain
{
    public class Card
    {
        public Card(int value, Suit suit)
        {
            Value = value;
            Suit = suit;
        }

        public int Value { get; set; }
        public Suit Suit { get; set; }

        public override string ToString()
        {
            return $"{Value} - {Suit}";
        }
    }
}
