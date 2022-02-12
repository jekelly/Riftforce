namespace Riftforce
{
    public class EarthGuild : Guild
    {
        public EarthGuild() : base("Earth") { }

        public override void OnPlayed(Location location, uint playerIndex)
        {
            location.ApplyDamageToAll(1 - playerIndex, 1);
        }
    }
}
