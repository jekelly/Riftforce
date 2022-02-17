using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Riftforce
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Deck
    {
        private static readonly Random r = new Random();

        [JsonProperty]
        private readonly Queue<uint> draw;

        [JsonProperty]

        private readonly Stack<uint> discard;

        public Deck(IEnumerable<uint> cards)
        {
            this.discard = new Stack<uint>();
            this.draw = new Queue<uint>(cards ?? Enumerable.Empty<uint>());
            this.Shuffle();
        }

        public void Shuffle()
        {
            var items = this.discard.Concat(this.draw).ToList();
            this.discard.Clear();
            this.draw.Clear();
            var shuffledDeck = new List<uint>(items);
            for (int i = items.Count - 1; i > 0; i--)
            {
                int index = r.Next(i);
                uint value = shuffledDeck[index];
                shuffledDeck[index] = shuffledDeck[i];
                shuffledDeck[i] = value;
            }
            for (int i = 0; i < shuffledDeck.Count; i++)
            {
                this.draw.Enqueue(shuffledDeck[i]);
            }
        }

        public void Discard(Elemental elemental)
        {
            this.discard.Push(elemental.Id);
        }

        public Elemental Draw()
        {
            return Elemental.Lookup(this.draw.Dequeue());
        }
    }
}
