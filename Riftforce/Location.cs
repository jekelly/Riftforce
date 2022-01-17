using System;
using System.Collections.ObjectModel;
using System.Linq;
using DynamicData;

namespace Riftforce
{
    public class ElementalProxy
    {
        public int Order { get; set; }
        public Elemental? Elemental { get; set; }
    }

    public class Location
    {
        private readonly SourceCache<Elemental, uint>[] elementals;
        private readonly ReadOnlyObservableCollection<Elemental>[] eList;
        public ReadOnlyObservableCollection<Elemental>[] Elementals => this.eList;

        public Location()
        {
            this.elementals = new SourceCache<Elemental, uint>[2];
            this.elementals[0] = new SourceCache<Elemental, uint>(e => e.Id);
            this.elementals[1] = new SourceCache<Elemental, uint>(e => e.Id);
            this.eList = new ReadOnlyObservableCollection<Elemental>[2];

            for (int i = 0; i < 2; i++)
            {
                int order = 0;
                this.elementals[i]
                    .Connect()
                    .Transform(e => new ElementalProxy() { Elemental = e, Order = order++ })
                    .SortBy(x => x.Order)
                    .Transform(e => e.Elemental)
                    .Bind(out this.eList[i])
                    .Subscribe();
            }
            //this.Elementals = this.elementals.Select(e => e.AsObservableCache()).ToArray();
        }

        public void Add(Elemental elemental, uint side)
        {
            this.elementals[side].AddOrUpdate(elemental);
        }

        public void ApplyDamageToFront(uint side)
        {
        }
    }
}
