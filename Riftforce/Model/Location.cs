using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Riftforce
{
    public class Location
    {
        private readonly IReadOnlyList<ElementalInPlay>[] elementals;
        private readonly List<ElementalInPlay>[] sides;
        public IReadOnlyList<ElementalInPlay>[] Elementals => this.elementals;

        public uint Index { get; }

        public Location(uint index)
        {
            this.Index = index;
            this.sides = new List<ElementalInPlay>[2];
            this.sides[0] = new List<ElementalInPlay>();
            this.sides[1] = new List<ElementalInPlay>();
            this.elementals = this.sides.Select(s => s.AsReadOnly()).ToArray();
        }

        public ElementalInPlay Add(Elemental elemental, uint side)
        {
            var eip = new ElementalInPlay(elemental, this.Index, this.sides[side].Count);
            this.sides[side].Add(eip);
            return eip;
        }

        public void Remove(ElementalInPlay elemental, uint side) => this.sides[side].Remove(elemental);
        public void Move(ElementalInPlay elemental, uint side)
        {
            elemental.Location = this.Index;
            elemental.Index = this.sides[side].Count;
            this.sides[side].Add(elemental);
        }

        public void ApplyDamageToFront(uint sourceSide, uint damage)
        {
            uint side = 1 - sourceSide;
            var first = sides[side].FirstOrDefault();
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
            if (target.CurrentDamage >= target.Strength)
            {
                this.Remove(target, playerIndex);
            }
        }

        public bool IsElementalPresent(uint elementalId)
        {
            return this.sides.Any(side => side.Select(eip => eip.Id).Contains(elementalId));
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
