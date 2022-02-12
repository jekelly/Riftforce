using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Riftforce
{
    public class Location
    {
        private readonly LocationSide[] sides;
        private readonly ReadOnlyObservableCollection<ElementalInPlay>[] eList;
        public ReadOnlyObservableCollection<ElementalInPlay>[] Elementals => this.eList;

        public uint Index { get; }

        public Location(uint index)
        {
            this.Index = index;
            this.sides = new LocationSide[2];
            this.sides[0] = new LocationSide(index);
            this.sides[1] = new LocationSide(index);
            this.eList = new ReadOnlyObservableCollection<ElementalInPlay>[2];
            this.eList[0] = this.sides[0].Cards;
            this.eList[1] = this.sides[1].Cards;
        }

        public ElementalInPlay Add(Elemental elemental, uint side)
        {
            return this.sides[side].Play(elemental);
        }

        public void Remove(ElementalInPlay elemental, uint side) => this.sides[side].Remove(elemental);
        public void Move(ElementalInPlay elemental, uint side) => this.sides[side].Move(elemental);

        public void ApplyDamageToFront(uint sourceSide, uint damage)
        {
            uint side = 1 - sourceSide;
            var first = sides[side].Cards.Where(c => c.Index == 0).SingleOrDefault();
            Debug.Assert(first == sides[side].Cards.FirstOrDefault());
            if (first is not null)
            {
                this.ApplyDamageToSpecificIndex(side, 0, damage);
            }
        }

        public void ApplyDamageToSpecificIndex(uint playerIndex, int elementalIndex, uint damage)
        {
            var target = this.Elementals[playerIndex][elementalIndex];
            target.CurrentDamage += damage;

            // check if the elemental has been destroyed
            if (target.CurrentDamage >= target.Elemental.Strength)
            {
                this.Remove(target, playerIndex);
            }
        }

        public bool IsElementalPresent(uint elementalId)
        {
            return this.sides.Any(side => side.Contains(elementalId));
        }

        internal void ApplyDamageToAll(uint index, uint damage)
        {
            for (int i = 0; i < this.sides[index].Count; i++)
            {
                this.ApplyDamageToSpecificIndex(index, i, damage);
            }
        }
    }
}
