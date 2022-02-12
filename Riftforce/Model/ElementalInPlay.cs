using System;
using System.Reactive.Subjects;

namespace Riftforce
{
    public class ElementalInPlay
    {
        private uint dmg;
        private readonly BehaviorSubject<uint> damage;
        public uint CurrentDamage
        {
            get => this.dmg;
            set
            {
                if (this.dmg != value)
                {
                    this.dmg = value;
                    this.damage.OnNext(this.dmg);
                }
            }
        }

        public ElementalInPlay(Elemental elemental, uint locationIndex, int index)
        {
            this.Elemental = elemental;
            this.Location = locationIndex;
            this.Index = index;
            this.damage = new BehaviorSubject<uint>(0);
        }

        public uint Location { get; set; }
        public int Index { get; set; }
        public uint Id => this.Elemental.Id;
        public Elemental Elemental { get; set; }
        public IObservable<uint> Damage => this.damage;

        //public void ApplyDamage(uint damage)
        //{
        //    this.dmg += damage;
        //    this.damage.OnNext(this.dmg);
        //}

        public void ApplyHealing(uint healing)
        {
            healing = Math.Min(healing, this.dmg);
            this.dmg -= healing;
            this.damage.OnNext(this.dmg);
        }

        public bool CanTarget(Game game, TargetElemental move) => this.Elemental.Guild.CanTarget(game, move);
        public bool CanTarget(Game game, TargetLocation move) => this.Elemental.Guild.CanTarget(game, move);

        public Phase Target(Game game, TargetLocation move) => this.Elemental.Guild.Target(game, move);
        public Phase Target(Game game, TargetElemental move) => this.Elemental.Guild.Target(game, move);
    }
}
