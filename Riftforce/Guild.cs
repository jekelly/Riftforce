using System.Collections.Generic;

namespace Riftforce
{
    public class Guild
    {
        private static int NextIndex;
        private readonly int index;
        private readonly string name;

        public string Name => this.name;

        public Guild(string name)
        {
            this.name = name;
            this.index = NextIndex++;
        }

        public static readonly Guild Fire = new Guild("Fire");
        public static readonly Guild Ice = new Guild("Ice");
        public static readonly Guild Light = new Guild("Light");
        public static readonly Guild Shadow = new Guild("Shadow");
        public static readonly Guild Earth = new Guild("Earth");
        public static readonly Guild Water = new Guild("Water");
        public static readonly Guild Thunder = new Guild("Thunder");
        public static readonly Guild Plant = new Guild("Plant");
        public static readonly Guild Air = new Guild("Air");
        public static readonly Guild Crystal = new Guild("Crystal");

        public static readonly IReadOnlyList<Guild> Guilds = new[] { Fire, Ice, Light, Shadow, Earth, Water, Thunder, Plant, Air, Crystal };
    }
}
