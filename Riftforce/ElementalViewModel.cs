using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;

namespace Riftforce
{
    public class ElementalViewModel : ReactiveObject
    {
        public uint Strength { get; }
        public string GuildName { get; }

        public ReactiveCommand<Unit, Unit> ActivateCommand { get; }

        public ElementalViewModel(ElementalInPlay model, Game game)
        {
            this.Strength = model.Elemental.Strength;
            this.GuildName = model.Elemental.Guild.Name;

            var activateMove = new ActivateElemental() { PlayerIndex = 0, ElementalId = model.Id };
            var activate = () => { game.ProcessMove(activateMove); };
            var canActivate = game.MinorUpdate.Select(g => g.CanPlay(activateMove));
            this.ActivateCommand = ReactiveCommand.Create(activate, canActivate);
        }
    }
}
