using System;
using System.Collections.Generic;
using System.Linq;

namespace Riftforce
{
    /*
    Riftfore The effects of the different Guilds
    Fire: Place 3 damage on the first enemy at this location 
    Place 1 damage on the ally directly behind this fire
    Ice: If there is damage on the last enemy at this location place 4 damage on it 
    Otherwise, place 1 damage on it
    Light: Place 3 damage on the first enemy at this location 
    Remove 1 damage from this Light or any ally
    Shadow: Move this Shadow to any other location
    Place 1 damage on the first enemy at the new location
    If the Shadows destroys this enemy gain +1 Riftforce
    Earth: When you play this Earth place 1 damage on each enemy at this location
    Place 2 damage on the first enemy at this location 
    Water: Place 2 damage on the first enemy at this location
    Move this water to an adjacent location
    Place 1 damage on the first enemy at the new location.
    Thunderbolt: Place 2 damage on any enemy at this location
    If the thunderbolt destroys this enemy repeat this ability once immediately.
    Plant: Place 2 damage on the first enemy in an adjacent location
    Move this enemy to the location of this Plant
    Air: Move this Air to any other location
    Place 1 damage each on the first enemy at the new and the adjacent locations.
    Crystal: Place 4 damage on the first enemy at this location
    When this Crystal is destroyed your opponent gains +1 Riftforce
     * */

    public class WaterGuild : Guild
    {
        public WaterGuild() : base("Water")
        {
        }

        public override Phase Activate(Location location, Elemental elemental, uint playerIndex)
        {
            location.ApplyDamageToFront(playerIndex, 2);
            return Phase.TargetLocation;
        }

        public override bool CanTarget(Game game, TargetLocation move)
        {
            long li = move.LocationIndex;
            long ae = game.ActiveElemental?.Location ?? uint.MaxValue;
            return Math.Abs(li - ae) == 1;
        }

        public override Phase Target(Game game, TargetLocation move)
        {
            var activeLocation = game.Locations.Where(location => location.Elementals[move.PlayerIndex].Select(e => e.Id).Contains(game.ActiveElemental.Id)).Single();
            activeLocation.Remove(game.ActiveElemental, move.PlayerIndex);
            // TODO: should move handle the removal?
            game.Locations[move.LocationIndex].Move(game.ActiveElemental, move.PlayerIndex);
            game.Locations[move.LocationIndex].ApplyDamageToFront(move.PlayerIndex, 1);
            return Phase.Activate;
        }
    }

    public class AirGuild : Guild
    {
        public AirGuild() : base("Air")
        {
        }

        public override Phase Activate(Location location, Elemental elemental, uint playerIndex)
        {
            return Phase.TargetLocation;
        }

        public override bool CanTarget(Game game, TargetLocation move)
        {
            return move.LocationIndex >= 0 && move.LocationIndex < 5;
        }

        public override Phase Target(Game game, TargetLocation move)
        {
            var activeLocation = game.Locations.Where(location => location.Elementals[move.PlayerIndex].Select(e => e.Id).Contains(game.ActiveElemental.Id)).Single();
            activeLocation.Remove(game.ActiveElemental, move.PlayerIndex);
            // TODO: should move handle the removal?
            game.Locations[move.LocationIndex].Move(game.ActiveElemental, move.PlayerIndex);
            game.Locations[move.LocationIndex].ApplyDamageToFront(move.PlayerIndex, 1);
            int left = (int)move.LocationIndex - 1;
            if (left >= 0)
            {
                game.Locations[left].ApplyDamageToFront(move.PlayerIndex, 1);
            }
            int right = (int)move.LocationIndex + 1;
            if (right < 5)
            {
                game.Locations[right].ApplyDamageToFront(move.PlayerIndex, 1);
            }
            return Phase.Activate;
        }
    }

    public class ShadowGuild : Guild
    {
        public ShadowGuild() : base("Shadow")
        {
        }

        public override Phase Activate(Location location, Elemental elemental, uint playerIndex) => Phase.TargetLocation;

        public override bool CanTarget(Game game, TargetLocation move) => move.LocationIndex >= 0 && move.LocationIndex < 5;

        public override Phase Target(Game game, TargetLocation move)
        {
            var activeLocation = game.Locations.Where(location => location.Elementals[move.PlayerIndex].Select(e => e.Id).Contains(game.ActiveElemental.Id)).Single();
            activeLocation.Remove(game.ActiveElemental, move.PlayerIndex);
            // TODO: should move handle the removal?
            game.Locations[move.LocationIndex].Move(game.ActiveElemental, move.PlayerIndex);
            game.Locations[move.LocationIndex].ApplyDamageToFront(move.PlayerIndex, 1);
            // TODO: get an extra point if we kill
            return Phase.Activate;
        }
    }

    public class LightGuild : Guild
    {
        public LightGuild() : base("Light")
        {
        }

        public override Phase Activate(Location location, Elemental elemental, uint playerIndex)
        {
            location.ApplyDamageToFront(playerIndex, 3);
            return Phase.TargetElemental;
        }

        public override bool CanTarget(Game game, TargetElemental move)
        {
            var elementalInPlay = game.Locations.Where(location => location.Elementals[move.PlayerIndex].Select(e => e.Id).Contains(move.ElementalId)).SingleOrDefault();
            return elementalInPlay is not null;
        }

        public override Phase Target(Game game, TargetElemental move)
        {
            var elementalInPlay = game.Locations.SelectMany(l => l.Elementals[move.PlayerIndex].Where(e => e.Id == move.ElementalId)).Single();
            elementalInPlay.ApplyHealing(1);
            return Phase.Activate;
        }
    }

    public class LightningGuild : Guild
    {
        public LightningGuild() : base("Lightning")
        {
        }

        public override Phase Activate(Location location, Elemental elemental, uint playerIndex) => Phase.TargetElemental;

        public override bool CanTarget(Game game, TargetElemental move)
        {
            return game.FindElemental(move.ElementalId, move.PlayerIndex)?.Location == game.ActiveElemental?.Location;
        }

        public override Phase Target(Game game, TargetElemental move)
        {
            var elemental = game.FindElemental(move.ElementalId, move.PlayerIndex);
            bool willKillTarget = (elemental.Elemental.Strength - elemental.CurrentDamage - 2) >= 0;
            elemental.ApplyDamage(2);
            if (willKillTarget && !game.HasUsedLightningThisTurn)
            {
                game.HasUsedLightningThisTurn = true;
                return Phase.TargetElemental;
            }

            game.HasUsedLightningThisTurn = false;
            return Phase.Activate;
        }
    }

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
        public static readonly Guild Light = new LightGuild();
        public static readonly Guild Shadow = new ShadowGuild();
        public static readonly Guild Earth = new EarthGuild();
        public static readonly Guild Water = new WaterGuild();
        public static readonly Guild Thunder = new LightningGuild();
        public static readonly Guild Plant = new PlantGuild();
        public static readonly Guild Air = new AirGuild();

        public virtual bool CanTarget(Game game, TargetElemental move) => false;
        public virtual bool CanTarget(Game game, TargetLocation move) => false;

        public static readonly Guild Crystal = new CrystalGuild();

        public static readonly IReadOnlyList<Guild> Guilds = new[] { Fire, Ice, Light, Shadow, Earth, Water, Thunder, Plant, Air, Crystal };

        public virtual Phase Target(Game game, TargetLocation move) => Phase.Activate;
        public virtual Phase Target(Game game, TargetElemental move) => Phase.Activate;

        public virtual Phase Activate(Location location, Elemental elemental, uint playerIndex)
        {
            location.ApplyDamageToFront(playerIndex, 2);
            return Phase.Activate;
        }

        public virtual void OnPlayed(Location location, uint playerIndex)
        {
        }
    }

    public class EarthGuild : Guild
    {
        public EarthGuild() : base("Earth") { }

        public override void OnPlayed(Location location, uint playerIndex)
        {
            foreach (var opponent in location.Elementals[1 - playerIndex])
            {
                opponent.ApplyDamage(1);
            }
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

        public override Phase Target(Game game, TargetLocation move)
        {
            game.Locations[move.LocationIndex].ApplyDamageToFront(move.PlayerIndex, 2);
            var victim = game.Locations[move.LocationIndex].Elementals[1 - move.PlayerIndex].FirstOrDefault();
            if (victim is not null)
            {
                game.Locations[move.LocationIndex].Remove(victim, 1 - move.PlayerIndex);
                game.Locations[game.ActiveElemental.Location].Move(victim, 1 - move.PlayerIndex);
            }
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
            var lastEnemy = enemyElementals.LastOrDefault();
            if (lastEnemy is not null)
            {
                if (lastEnemy.CurrentDamage > 0)
                {
                    lastEnemy.ApplyDamage(4);
                }
                else
                {
                    lastEnemy.ApplyDamage(1);
                }
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
