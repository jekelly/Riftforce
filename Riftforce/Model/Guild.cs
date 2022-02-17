using System.Collections.Generic;

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

        public Phase Activate(Location location, uint elementalId, uint playerIndex) => this.Activate(location, Elemental.Lookup(elementalId), playerIndex);

        public virtual Phase Activate(Location location, Elemental elemental, uint playerIndex)
        {
            location.ApplyDamageToFront(playerIndex, 2);
            return Phase.Activate;
        }

        public virtual void OnPlayed(Location location, uint playerIndex)
        {
        }
    }
}
