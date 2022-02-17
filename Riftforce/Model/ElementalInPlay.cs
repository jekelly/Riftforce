using System;
using System.Reactive.Subjects;

namespace Riftforce
{
    public class ElementalInPlay
    {
        private readonly Elemental elemental;

        public ElementalInPlay(Elemental elemental, uint locationIndex, int index)
        {
            this.elemental = elemental;
            this.Location = locationIndex;
            this.Index = index;
        }

        public uint Id => this.elemental.Id;
        public uint Strength => this.elemental.Strength;
        public Guild Guild => this.elemental.Guild;
        public uint Location { get; set; }
        public int Index { get; set; }
        public uint CurrentDamage { get; set; }

        public void ApplyHealing(uint healing)
        {
            healing = Math.Min(healing, this.CurrentDamage);
            this.CurrentDamage -= healing;
        }

        public bool CanTarget(Game game, TargetElemental move) => this.elemental.Guild.CanTarget(game, move);
        public bool CanTarget(Game game, TargetLocation move) => this.elemental.Guild.CanTarget(game, move);

        public Phase Target(Game game, TargetLocation move) => this.elemental.Guild.Target(game, move);
        public Phase Target(Game game, TargetElemental move) => this.elemental.Guild.Target(game, move);
    }
}
