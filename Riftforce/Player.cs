using System.Collections.Generic;
using DynamicData;

namespace Riftforce
{
    public class Player
    {
        private readonly Deck<Elemental> elementals;
        private readonly SourceCache<Elemental, uint> hand;

        public IObservableCache<Elemental, uint> Hand => this.hand.AsObservableCache();

        public Player(List<Elemental> deck)
        {
            this.elementals = new Deck<Elemental>(deck);
            this.elementals.Shuffle();
            this.hand = new SourceCache<Elemental, uint>(e => e.Id);
        }

        public Elemental Draw()
        {
            return this.elementals.Draw();
        }

        public void DrawToHand()
        {
            this.hand.AddOrUpdate(this.elementals.Draw());
        }

        public Elemental PlayFromHand(uint id)
        {
            var value = this.hand.Lookup(id).Value;
            this.hand.RemoveKey(id);
            return value;
        }
    }
}
