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
        public uint CurrentDamage => this.dmg;

        public ElementalInPlay(Elemental elemental, uint locationIndex, int index)
        {
            this.Elemental = elemental;
            this.Location = locationIndex;
            this.Index = index;
            this.damage = new BehaviorSubject<uint>(0);
        }

        public uint Location { get; set; }
        public int Index { get; set; }
        public uint Id => this.Elemental.Id;
        public Elemental Elemental { get; set; }
        public IObservable<uint> Damage => this.damage;

        public void ApplyDamage(uint damage)
        {
            this.dmg += damage;
            this.damage.OnNext(this.dmg);
        }

        public void ApplyHealing(uint healing)
        {
            this.dmg -= healing;
            this.damage.OnNext(this.dmg);
        }

        public bool CanTarget(Game game, TargetElemental move) => this.Elemental.Guild.CanTarget(game, move);
        public bool CanTarget(Game game, TargetLocation move) => this.Elemental.Guild.CanTarget(game, move);

        public Phase Target(Game game, TargetLocation move) => this.Elemental.Guild.Target(game, move);
        public Phase Target(Game game, TargetElemental move) => this.Elemental.Guild.Target(game, move);
    }

    public class LocationSide
    {
        private readonly SourceCache<ElementalInPlay, uint> cache = new SourceCache<ElementalInPlay, uint>(e => e.Id);
        private readonly ReadOnlyObservableCollection<ElementalInPlay> cards;

        public ReadOnlyObservableCollection<ElementalInPlay> Cards => this.cards;
        public int Count => this.cache.Count;
        private readonly uint index;

        public LocationSide(uint index)
        {
            this.index = index;
            this.cache
                .Connect()
                .SortBy(e => e.Index)
                .Bind(out this.cards)
                .Subscribe();
        }

        public ElementalInPlay Play(Elemental elemental)
        {
            var eip = new ElementalInPlay(elemental, index, this.Count);
            this.cache.AddOrUpdate(eip);
            return eip;
        }

        public void Move(ElementalInPlay elemental)
        {
            elemental.Location = this.index;
            elemental.Index = this.Count;
            this.cache.AddOrUpdate(elemental);
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
                first.ApplyDamage(damage);
            }
        }

        public bool IsElementalPresent(uint elementalId)
        {
            return this.sides.Any(side => side.Contains(elementalId));
        }
    }
}
