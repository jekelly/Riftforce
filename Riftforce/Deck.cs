using System;
using System.Collections.Generic;

namespace Riftforce
{
    public class Deck<T>
    {
        private static readonly Random r = new Random();
        private readonly IReadOnlyList<T> deck;
        private readonly Queue<T> draw;
        private readonly Stack<T> discard;

        public Deck(IEnumerable<T> cards)
        {
            this.deck = new List<T>(cards).AsReadOnly();
            this.discard = new Stack<T>();
            this.draw = new Queue<T>();
        }

        public void Shuffle()
        {
            var shuffledDeck = new List<T>(this.deck);
            for (int i = this.deck.Count - 1; i > 0; i--)
            {
                int index = r.Next(i);
                T value = shuffledDeck[index];
                shuffledDeck[index] = shuffledDeck[i];
                shuffledDeck[i] = value;
            }

            this.draw.Clear();
            for (int i = 0; i < shuffledDeck.Count; i++)
            {
                this.draw.Enqueue(shuffledDeck[i]);
            }
        }

        public T Draw()
        {
            return this.draw.Dequeue();
        }
    }
}
