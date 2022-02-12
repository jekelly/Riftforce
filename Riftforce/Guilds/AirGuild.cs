using System;
using System.Linq;

namespace Riftforce
{
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
}
