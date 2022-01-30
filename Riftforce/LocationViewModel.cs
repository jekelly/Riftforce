using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Operators;
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

        public ReactiveCommand<Unit, Unit> Command { get; }

        public LocationViewModel(Location location, GameView game, Game game1)
        {
            location.Elementals[0]
                .ToObservableChangeSet()
                .Transform(x => new ElementalViewModel(x, game1))
                .Bind(out this.elementalOne)
                .Subscribe();
            location.Elementals[1]
                .ToObservableChangeSet()
                .Transform(x => new ElementalViewModel(x, game1))
                .Bind(out this.elementalTwo)
                .Subscribe();

            int index = game1.Locations.IndexOf(location);

            this.Command = ReactiveCommand.Create(() => PlaySelectedElemental(), game.WhenAnyValue(x => x.SelectedElemental, (Elemental e) => e is not null && game1.CanPlay(new PlayElemental() { ElementalId = e.Id, PlayerIndex = 0, LocationIndex = (uint)index })));

            Unit PlaySelectedElemental()
            {
                game.PlayElementalToLocation(location, game.SelectedElemental);
                return Unit.Default;
            }
        }
    }
}
