using System;
using System.Linq;

namespace Riftforce
{
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
}
