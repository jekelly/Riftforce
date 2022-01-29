using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DynamicData;
using ReactiveUI;

namespace Riftforce
{
    public class GameView : ReactiveObject
    {
        private readonly ReadOnlyObservableCollection<Elemental> hand;
        public ReadOnlyObservableCollection<Elemental> Hand => this.hand;

        private readonly LocationViewModel[] locations;
        private readonly ReactiveCommand<(Location loc, Elemental e), bool> playElementalToLocationCommand;

        private readonly ReactiveCommand<Unit, Unit> endTurnCommand;
        public ReactiveCommand<Unit, Unit> EndTurnCommand => this.endTurnCommand;

        public LocationViewModel[] Locations => this.locations;

        private Elemental? selectedElemental;
        public Elemental? SelectedElemental
        {
            get => this.selectedElemental;
            set => this.RaiseAndSetIfChanged(ref this.selectedElemental, value);
        }

        public GameView(Game game)
        {
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

            this.endTurnCommand = ReactiveCommand.Create(() =>
            {
                game.EndTurn();
            });
        }
        
        public async void PlayElementalToLocation(Location location, Elemental elemental)
        {
            await this.playElementalToLocationCommand.Execute((location, elemental));
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IViewFor<GameView>
    {
        public MainWindow()
        {
            InitializeComponent();

            var game = new GameBuilder().Build();
            this.ViewModel = new GameView(game);
            this.OneWayBind(this.ViewModel, vm => vm.Hand, v => v.Hand.ItemsSource);
            this.OneWayBind(this.ViewModel, vm => vm.Locations, v => v.Locations.ItemsSource);
            this.Bind(this.ViewModel, vm => vm.SelectedElemental, v => v.Hand.SelectedItem);

            this.BindCommand(this.ViewModel, vm => vm.EndTurnCommand, v => v.EndTurn);

            this.WhenAnyValue(x => x.Hand.SelectedValue).Subscribe(x => Debug.WriteLine($"{x} is new selected hand item"));
        }

        public GameView? ViewModel
        {
            get { return (GameView?)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(GameView), typeof(MainWindow), new PropertyMetadata(null));

        object? IViewFor.ViewModel { get => this.ViewModel; set => this.ViewModel = (GameView)value; }
    }
}
