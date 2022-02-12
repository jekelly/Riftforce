using System;
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
}
