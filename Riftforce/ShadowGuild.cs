using System;
using System.Linq;

namespace Riftforce
{
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
}
