using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;

namespace Riftforce
{
    public class LocationViewModel : ReactiveObject
    {
        private readonly ReadOnlyObservableCollection<ElementalViewModel> elementalOne;
        private readonly ReadOnlyObservableCollection<ElementalViewModel> elementalTwo;
        public ReadOnlyObservableCollection<ElementalViewModel> ElementalOne => this.elementalOne;
        public ReadOnlyObservableCollection<ElementalViewModel> ElementalTwo => this.elementalTwo;

        public ICommand Command { get; }

        public LocationViewModel(Location location, GameViewModel game, Game game1)
        {
            game1.MinorUpdate
                .Select(g => g.Locations[location.Index].Elementals[0])
                .ToObservableChangeSet(x => new { x.Id, x.Index })
                .Transform(x => new ElementalViewModel(x, game1, 0))
                .Bind(out this.elementalOne)
                .Subscribe();
            game1.MinorUpdate
                .Select(g => g.Locations[location.Index].Elementals[1])
                .ToObservableChangeSet(x => new { x.Id, x.Index })
                .Transform(x => new ElementalViewModel(x, game1, 1))
                .Bind(out this.elementalTwo)
                .Subscribe();

            int index = game1.Locations.IndexOf(location);

            var canDeployTo = game.WhenAnyValue(x => x.SelectedElemental, (Elemental e) => e is not null && game1.CanPlay(new PlayElemental() { ElementalId = e.Id, PlayerIndex = 0, LocationIndex = (uint)index }));
            var deployCommand = ReactiveCommand.Create(() => PlaySelectedElemental(), canDeployTo);

            var targetLocation = new TargetLocation() { LocationIndex = location.Index };
            var canTarget = game1.MinorUpdate.Select(x => x.CanPlay(targetLocation)).StartWith(false);
            var target = () => game1.ProcessMove(targetLocation);
            var targetCommand = ReactiveCommand.Create(target, canTarget);

            var eFunc = () =>
            {
                if (game1.CanPlay(targetLocation))
                {
                    game1.ProcessMove(targetLocation);
                }
                else
                {
                    PlaySelectedElemental();
                }
            };

            var either = canDeployTo.Merge(canTarget);

            this.Command = ReactiveCommand.Create(eFunc, either);

            Unit PlaySelectedElemental()
            {
                game.PlayElementalToLocation(location, game.SelectedElemental);
                return Unit.Default;
            }
        }
    }
}
