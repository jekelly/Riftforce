using System.Linq;

namespace Riftforce
{
    public class IceGuild : Guild
    {
        public IceGuild() : base("Ice") { }

        public override Phase Activate(Location location, Elemental elemental, uint playerIndex)
        {
            var enemyIndex = 1 - playerIndex;
            var enemyElementals = location.Elementals[enemyIndex];
            var lastEnemy = enemyElementals.LastOrDefault();
            if (lastEnemy is not null)
            {
                uint damage = lastEnemy.CurrentDamage > 0 ? 4u : 1u;
                location.ApplyDamageToSpecificIndex(1 - playerIndex, lastEnemy.Index, damage);
            }

            return Phase.Activate;
        }
    }
}
