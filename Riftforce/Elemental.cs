using System.Text;
using System.Threading.Tasks;

namespace Riftforce
{
    public class Elemental
    {
        private static uint NextId;
        public uint Id { get; private set; }
        public Guild Guild { get; private set; }
        public uint Strength { get; private set; }
        public uint Damage { get; set; }

        public Elemental(uint strength, Guild guild)
        {
            this.Id = NextId++;
            this.Guild = guild;
            this.Strength = strength;
            this.Damage = 0;
        }
    }
}
