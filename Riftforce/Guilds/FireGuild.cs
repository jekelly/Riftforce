using System.Linq;

namespace Riftforce
{
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
                location.ApplyDamageToSpecificIndex(playerIndex, elementalId.Index + 1, 1);
            }

            return Phase.Activate;
        }
    }
}
