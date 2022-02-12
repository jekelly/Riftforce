using System;
using System.Collections.ObjectModel;
using System.Linq;
using DynamicData;

namespace Riftforce
{
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
}
