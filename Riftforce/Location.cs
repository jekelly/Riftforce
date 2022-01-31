using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Subjects;
using DynamicData;

namespace Riftforce
{
    public class ElementalInPlay
    {
        private uint dmg;
        private readonly BehaviorSubject<uint> damage;

        public ElementalInPlay(Elemental elemental, int index)
        {
            this.Elemental = elemental;
            this.Index = index;
            this.damage = new BehaviorSubject<uint>(0);
        }

        public int Index { get; set; }
        public uint Id => this.Elemental.Id;
        public Elemental Elemental { get; set; }
        public IObservable<uint> Damage => this.damage;

        public void ApplyDamage(uint damage)
        {
            this.dmg += damage;
            this.damage.OnNext(this.dmg);
        }
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

        public bool Contains(uint elementalId) => this.cache.Lookup(elementalId).HasValue;
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

        public void ApplyDamageToFront(uint sourceSide, uint damage)
        {
            uint side = 1 - sourceSide;
            var first = sides[side].Cards.Where(c => c.Index == 0).SingleOrDefault();
            Debug.Assert(first == sides[side].Cards.FirstOrDefault());
            if (first is not null)
            {
                first.ApplyDamage(damage);
            }
        }

        public bool IsElementalPresent(uint elementalId)
        {
            return this.sides.Any(side => side.Contains(elementalId));
        }
    }
}
