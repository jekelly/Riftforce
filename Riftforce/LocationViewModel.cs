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
        private readonly ReadOnlyObservableCollection<Elemental>[] elementals;
        public ReadOnlyObservableCollection<Elemental>[] Elementals => this.elementals;
        public ReadOnlyObservableCollection<Elemental> ElementalOne => this.elementals[0];
        public ReadOnlyObservableCollection<Elemental> ElementalTwo => this.elementals[1];

        public ReactiveCommand<Unit, Unit> Command { get; }

        public LocationViewModel(Location location, GameView game, Game game1)
        {
            this.elementals = new ReadOnlyObservableCollection<Elemental>[2];
            for (int i = 0; i < location.Elementals.Length; i++)
            {
                location.Elementals[i]
                    .Connect()
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Bind(out this.elementals[i])
                    .Subscribe();
            }

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
