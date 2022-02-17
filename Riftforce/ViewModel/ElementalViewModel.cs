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

        public ElementalViewModel(ElementalInPlay model, Game game, uint side)
        {
            this.Strength = model.Strength;
            this.GuildName = model.Guild.Name;

            this.damage = game.MinorUpdate
                .Select(g => g.Locations[model.Location].Elementals[side][model.Index].CurrentDamage)
                .DistinctUntilChanged()
                .ToProperty(this, nameof(DamageTaken));

            var activateMove = new ActivateElemental() { PlayerIndex = 0, ElementalId = model.Id };
            var activate = () => { game.ProcessMove(activateMove); };
            var canActivate = game.MinorUpdate.Select(g => g.CanActivate(activateMove));

            var targetElemental = new TargetElemental() { ElementalId = model.Id, PlayerIndex = side };
            var canTarget = game.MinorUpdate.Select(g => g.CanPlay(targetElemental));
            var target = () => { game.ProcessMove(targetElemental); };

            var mergedCommand = () =>
            {
                if (game.CanActivate(activateMove))
                {
                    activate();
                }
                else if (game.CanPlay(targetElemental))
                {
                    target();
                }
            };
            // TODO order matters here and it shouldn't...
            var mergedCanExecute = game.MinorUpdate.Select(g => g.CanPlay(targetElemental) || g.CanActivate(activateMove));

            this.ActivateCommand = ReactiveCommand.Create(mergedCommand, mergedCanExecute);
        }
    }
}
