using System;
using System.Collections.ObjectModel;
using System.Linq;
using DynamicData;

namespace Riftforce
{
    public class ElementalInPlay
    {
        public ElementalInPlay(Elemental elemental, int index)
        {
            this.Elemental = elemental;
            this.Index = index;
        }

        public int Index { get; set; }
        public uint Id => this.Elemental.Id;
        public Elemental Elemental { get; set; }
    }

    public class LocationSide
    {
        private readonly SourceCache<ElementalInPlay, uint> cache = new SourceCache<ElementalInPlay, uint>(e => e.Id);
        private readonly ReadOnlyObservableCollection<ElementalInPlay> cards;

        public ReadOnlyObservableCollection<ElementalInPlay> Cards => this.cards;
        public int Count => this.cache.Count;

        public LocationSide()
        {
            this.cache
                .Connect()
                .SortBy(e => e.Index)
                .Bind(out this.cards)
                .Subscribe();
        }

        public ElementalInPlay Play(Elemental elemental)
        {
            var eip = new ElementalInPlay(elemental, this.Count);
            this.cache.AddOrUpdate(eip);
            return eip;
        }

        public void Remove(ElementalInPlay elemental)
        {
            this.cache.Edit((update) =>
            {
                var items = update.Items.Where(x => x.Index > elemental.Index);
                foreach (var item in items)
                {
                    item.Index--;
                }
                update.Remove(elemental);
            });
        }
    }

    public class Location
    {
        private readonly LocationSide[] sides;
        private readonly ReadOnlyObservableCollection<ElementalInPlay>[] eList;
        public ReadOnlyObservableCollection<ElementalInPlay>[] Elementals => this.eList;

        public Location()
        {
            this.sides = new LocationSide[2];
            this.sides[0] = new LocationSide();
            this.sides[1] = new LocationSide();
            this.eList = new ReadOnlyObservableCollection<ElementalInPlay>[2];
            this.eList[0] = this.sides[0].Cards;
            this.eList[1] = this.sides[1].Cards;
        }

        public void Add(Elemental elemental, uint side)
        {
            this.sides[side].Play(elemental);
        }

        public void ApplyDamageToFront(uint side)
        {
        }
    }
}
