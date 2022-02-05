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
        private readonly ObservableAsPropertyHelper<int> turn;
        private readonly ObservableAsPropertyHelper<uint> playerOneScore;
        private readonly ObservableAsPropertyHelper<uint> playerTwoScore;
        private readonly LocationViewModel[] locations;
        private readonly ReactiveCommand<(Location loc, Elemental e), bool> playElementalToLocationCommand;
        private Elemental? selectedElemental;

        public ReadOnlyObservableCollection<Elemental> Hand => this.hand;
        public ReactiveCommand<Unit, Unit> EndTurnCommand { get; }
        public ReactiveCommand<Unit, Unit> CheckAndDrawCommand { get; }
        public ReactiveCommand<Elemental, Unit> DiscardCommand { get; }
        public int Turn => this.turn.Value;
        public uint PlayerOneScore => this.playerOneScore.Value;
        public uint PlayerTwoScore => this.playerTwoScore.Value;
        public LocationViewModel[] Locations => this.locations;

        public Elemental? SelectedElemental
        {
            get => this.selectedElemental;
            set => this.RaiseAndSetIfChanged(ref this.selectedElemental, value);
        }

        public GameViewModel(Game game)
        {
            this.turn = game.Turn.ToProperty(this, nameof(Turn));

            this.playerOneScore = game.UpdateState.Select(g => g.Scores[0]).ToProperty(this, nameof(PlayerOneScore));
            this.playerTwoScore = game.UpdateState.Select(g => g.Scores[1]).ToProperty(this, nameof(PlayerTwoScore));

            game.Players[0].Hand
                .Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out this.hand)
                .Subscribe();

            this.locations = game.Locations.Select(l => new LocationViewModel(l, this, game)).ToArray();

            var canPlayElementalToLocation = this.WhenAnyValue(x => x.SelectedElemental, (Elemental? e) => e is not null);
            this.playElementalToLocationCommand = ReactiveCommand.Create<(Location loc, Elemental e), bool>(l =>
            {
                return game.ProcessMove(new PlayElemental()
                {
                    ElementalId = l.e.Id,
                    LocationIndex = (uint)game.Locations.IndexOf(l.loc),
                    PlayerIndex = 0,
                });
            }, canPlayElementalToLocation);

            this.EndTurnCommand = ReactiveCommand.Create(() =>
            {
                game.EndTurn();
            });

            var handSizeChanged = game.ActivePlayer.Hand.CountChanged.Select(x => game.CanPlay(new DrawAndScore() { PlayerIndex = 0 }));
            var moveTypeChanged = game.MinorUpdate.Select(x => game.CanPlay(new DrawAndScore() { PlayerIndex = 0 }));
            var canCheckAndDraw = Observable.Merge(handSizeChanged, moveTypeChanged);

            this.CheckAndDrawCommand = ReactiveCommand.Create(() =>
            {
                game.ProcessMove(new DrawAndScore() { PlayerIndex = 0 });
            },
            canCheckAndDraw);

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
