using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData;
using ReactiveUI;

namespace Riftforce
{
    public class LocationViewModel : ReactiveObject
    {
        public ReadOnlyObservableCollection<ElementalInPlay> ElementalOne { get; }
        public ReadOnlyObservableCollection<ElementalInPlay> ElementalTwo { get; }

        public ReactiveCommand<Unit, Unit> Command { get; }

        public LocationViewModel(Location location, GameView game, Game game1)
        {
            this.ElementalOne = location.Elementals[0];
            this.ElementalTwo = location.Elementals[1];

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
