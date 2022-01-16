using System.Linq;
using DynamicData;

namespace Riftforce
{
    public class Location
    {
        private readonly SourceCache<Elemental, uint>[] elementals;
        public IObservableCache<Elemental, uint>[] Elementals { get; }

        public Location()
        {
            this.elementals = new SourceCache<Elemental, uint>[2];
            this.elementals[0] = new SourceCache<Elemental, uint>(e => e.Id);
            this.elementals[1] = new SourceCache<Elemental, uint>(e => e.Id);
            this.Elementals = this.elementals.Select(e => e.AsObservableCache()).ToArray();
        }

        public void Add(Elemental elemental, uint side)
        {
            this.elementals[side].AddOrUpdate(elemental);
        }
    }
}
