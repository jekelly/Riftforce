using System;
using System.Linq;

namespace Riftforce
{
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
}
