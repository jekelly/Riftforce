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
        public static readonly Guild Plant = new PlantGuild();
        public static readonly Guild Air = new Guild("Air");

        public virtual bool CanTarget(Game game, TargetElemental move) => false;
        public virtual bool CanTarget(Game game, TargetLocation move) => false;

        public static readonly Guild Crystal = new CrystalGuild();

        public static readonly IReadOnlyList<Guild> Guilds = new[] { Fire, Ice, Light, Shadow, Earth, Water, Thunder, Plant, Air, Crystal };

        public virtual void Target(Game game, TargetLocation move) { }

        public virtual Phase Activate(Location location, Elemental elemental, uint playerIndex)
        {
            location.ApplyDamageToFront(playerIndex, 2);
            return Phase.Activate;
        }
    }

    public class PlantGuild : Guild
    {
        public PlantGuild() : base("Plant") { }

        public override Phase Activate(Location location, Elemental elemental, uint playerIndex)
        {
            return Phase.TargetLocation;
        }

        public override bool CanTarget(Game game, TargetLocation move)
        {
            long li = move.LocationIndex;
            long ae = game.ActiveElemental?.Location ?? uint.MaxValue;
            return Math.Abs(li - ae) == 1;
        }

        public override void Target(Game game, TargetLocation move)
        {
            game.Locations[move.LocationIndex].ApplyDamageToFront(move.PlayerIndex, 2);
            var victim = game.Locations[move.LocationIndex].Elementals[1 - move.PlayerIndex].First();
            game.Locations[move.LocationIndex].Remove(victim, 1 - move.PlayerIndex);
            game.Locations[game.ActiveElemental.Location].Move(victim, 1 - move.PlayerIndex);
            game.Phase = Phase.Activate;
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
