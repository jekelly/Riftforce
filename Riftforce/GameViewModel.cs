using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
using ReactiveUI;

namespace Riftforce
{
    public class GameViewModel : ReactiveObject
    {
        private readonly ReadOnlyObservableCollection<Elemental> hand;
        public ReadOnlyObservableCollection<Elemental> Hand => this.hand;

        private readonly LocationViewModel[] locations;
        private readonly ReactiveCommand<(Location loc, Elemental e), bool> playElementalToLocationCommand;

        public ReactiveCommand<Unit, Unit> EndTurnCommand { get; }
        public ReactiveCommand<Unit, Unit> CheckAndDrawCommand { get; }
        public ReactiveCommand<Elemental, Unit> DiscardCommand { get; }

        private readonly ObservableAsPropertyHelper<int> turn;
        public int Turn => this.turn.Value;

        public LocationViewModel[] Locations => this.locations;

        private Elemental? selectedElemental;
        public Elemental? SelectedElemental
        {
            get => this.selectedElemental;
            set => this.RaiseAndSetIfChanged(ref this.selectedElemental, value);
        }

        public GameViewModel(Game game)
        {
            this.turn = game.Turn.ToProperty(this, nameof(Turn));

            game.Players[0].Hand
                .Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out this.hand)
                .Subscribe();

            this.locations = game.Locations.Select(l => new LocationViewModel(l, this, game)).ToArray();

            this.playElementalToLocationCommand = ReactiveCommand.Create<(Location loc, Elemental e), bool>(l =>
            {
                return game.ProcessMove(new PlayElemental()
                {
                    ElementalId = l.e.Id,
                    LocationIndex = (uint)game.Locations.IndexOf(l.loc),
                    PlayerIndex = 0,
                });
            },
            this.WhenAnyValue(x => x.SelectedElemental, (Elemental e) => e is not null));

            this.EndTurnCommand = ReactiveCommand.Create(() =>
            {
                game.EndTurn();
            });

            var handSizeChanged = game.ActivePlayer.Hand.CountChanged.Select(x => game.CanPlay(new DrawAndScore() { PlayerIndex = 0 }));
            var moveTypeChanged = game.MinorUpdate.Select(x => game.CanPlay(new DrawAndScore() { PlayerIndex = 0 }));
            var composed = Observable.Merge(handSizeChanged, moveTypeChanged);

            this.CheckAndDrawCommand = ReactiveCommand.Create(() =>
            {
                game.ProcessMove(new DrawAndScore() { PlayerIndex = 0 });
            },
            composed);

            this.DiscardCommand = ReactiveCommand.Create((Elemental elemental) =>
            {
                game.ProcessMove(new DiscardAction() { DiscardId = elemental.Id });
            },
            this.WhenAnyValue(game => game.SelectedElemental, e => e is not null && game.CanPlay(new DiscardAction() { DiscardId = e.Id })));
        }

        public async void PlayElementalToLocation(Location location, Elemental elemental)
        {
            await this.playElementalToLocationCommand.Execute((location, elemental));
        }
    }
}
