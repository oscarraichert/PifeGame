using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace PifeGame.Domain
{
    public class Game
    {
        public Guid Id { get; set; }
        public List<Card> Deck { get; set; }
        public ConcurrentBag<WebSocket> Connections { get; set; } = new();

        public Game()
        {
            Id = Guid.NewGuid();
            Deck = InitializeDeck();
        }

        static List<Card> InitializeDeck()
        {
            var cards = new List<Card>();
            for (int i = 0; i < 2; i++)
            {
                foreach (Suit suit in Enum.GetValues(typeof(Suit)))
                {
                    for (int j = 1; j <= 13; j++)
                    {
                        cards.Add(new Card(j, suit));
                    }
                }
            }

            Shuffle(cards);
            return cards;
        }

        static void Shuffle<T>(List<T> cards)
        {
            var rng = new Random();
            int n = cards.Count;

            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (cards[n], cards[k]) = (cards[k], cards[n]);
            }
        }
    }
}
