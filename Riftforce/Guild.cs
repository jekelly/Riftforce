using System;
using System.Collections.Generic;
using System.Linq;

namespace Riftforce
{
    public class Guild
    {
        private static int NextIndex;
        private readonly int index;
        private readonly string name;

        public virtual string Name => this.name;

        public Guild(string name)
        {
            this.name = name;
            this.index = NextIndex++;
        }

        public static readonly Guild Fire = new FireGuild();
        public static readonly Guild Ice = new IceGuild();
        public static readonly Guild Light = new Guild("Light");
        public static readonly Guild Shadow = new Guild("Shadow");
        public static readonly Guild Earth = new Guild("Earth");
        public static readonly Guild Water = new Guild("Water");
        public static readonly Guild Thunder = new Guild("Thunder");
        public static readonly Guild Plant = new Guild("Plant");
        public static readonly Guild Air = new Guild("Air");
        public static readonly Guild Crystal = new CrystalGuild();

        public static readonly IReadOnlyList<Guild> Guilds = new[] { Fire, Ice, Light, Shadow, Earth, Water, Thunder, Plant, Air, Crystal };

        public virtual Phase Activate(Location location, Elemental elemental, uint playerIndex)
        {
            location.ApplyDamageToFront(playerIndex, 2);
            return Phase.Activate;
        }
    }

    public class IceGuild : Guild
    {
        public IceGuild() : base("Ice") { }

        public override Phase Activate(Location location, Elemental elemental, uint playerIndex)
        {
            var enemyIndex = 1 - playerIndex;
            var enemyElementals = location.Elementals[enemyIndex];
            var lastEnemy = enemyElementals.Last();
            if (lastEnemy.CurrentDamage > 0)
            {
                lastEnemy.ApplyDamage(4);
            }
            else
            {
                lastEnemy.ApplyDamage(1);
            }

            return Phase.Activate;
        }
    }

    public class CrystalGuild : Guild
    {
        public CrystalGuild() : base("Crystal") { }

        public override Phase Activate(Location location, Elemental elemental, uint playerIndex)
        {
            location.ApplyDamageToFront(playerIndex, 4);
            return Phase.Activate;
        }
    }

    public class FireGuild : Guild
    {
        public FireGuild() : base("Fire") { }

        public override Phase Activate(Location location, Elemental elemental, uint playerIndex)
        {
            // 3 damage to front enemy
            location.ApplyDamageToFront(playerIndex, 3);
            // 1 damage to ally directly behind
            var playerElementals = location.Elementals[playerIndex];
            var elementalId = playerElementals.Select((eip, i) => new { Index = i, Id = eip.Id }).Single(a => a.Id == elemental.Id);
            if (elementalId.Index < playerElementals.Count - 1)
            {
                var allyTarget = location.Elementals[playerIndex][elementalId.Index + 1];
                allyTarget.ApplyDamage(1);
            }

            return Phase.Activate;
        }
    }
}
