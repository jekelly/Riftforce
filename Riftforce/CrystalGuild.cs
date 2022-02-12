namespace Riftforce
{
    public class CrystalGuild : Guild
    {
        public CrystalGuild() : base("Crystal") { }

        public override Phase Activate(Location location, Elemental elemental, uint playerIndex)
        {
            location.ApplyDamageToFront(playerIndex, 4);
            return Phase.Activate;
        }
    }
}
