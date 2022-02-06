using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;

namespace Riftforce
{
    public class ElementalViewModel : ReactiveObject
    {
        private readonly ObservableAsPropertyHelper<uint> damage;

        public uint Strength { get; }
        public string GuildName { get; }
        public uint DamageTaken => damage.Value;

        public ReactiveCommand<Unit, Unit> ActivateCommand { get; }

        public ElementalViewModel(ElementalInPlay model, Game game)
        {
            this.Strength = model.Elemental.Strength;
            this.GuildName = model.Elemental.Guild.Name;

            this.damage = model.Damage.ToProperty(this, nameof(DamageTaken));

            var activateMove = new ActivateElemental() { PlayerIndex = 0, ElementalId = model.Id };
            var activate = () => { game.ProcessMove(activateMove); };
            var canActivate = game.MinorUpdate.Select(g => g.CanActivate(activateMove));
            this.ActivateCommand = ReactiveCommand.Create(activate, canActivate);
        }
    }
}
