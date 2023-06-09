﻿using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Windows;
using DynamicData;
using ReactiveUI;

namespace Riftforce
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IViewFor<GameViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();

            var gameService = new Games();
            var gameId = gameService.CreateGame();
            var playerId = gameId.Players[0];

            var game = new Game(gameId.State);
            GameBuilder.Initialize(game);

            this.ViewModel = new GameViewModel(game);
            this.OneWayBind(this.ViewModel, vm => vm.Turn, v => v.TurnText.Text, v => $"Turn: {v}");
            this.OneWayBind(this.ViewModel, vm => vm.PlayerOneScore, v => v.PlayerOneScore.Text, v => $"Mine: {v} points");
            this.OneWayBind(this.ViewModel, vm => vm.PlayerTwoScore, v => v.PlayerTwoScore.Text, v => $"Theirs: {v} points");
            this.OneWayBind(this.ViewModel, vm => vm.Hand, v => v.Hand.ItemsSource);
            this.OneWayBind(this.ViewModel, vm => vm.Locations, v => v.Locations.ItemsSource);
            this.Bind(this.ViewModel, vm => vm.SelectedElemental, v => v.Hand.SelectedItem);

            this.BindCommand(this.ViewModel, vm => vm.EndTurnCommand, v => v.EndTurn);
            this.BindCommand(this.ViewModel, vm => vm.CheckAndDrawCommand, v => v.CheckAndDraw);
            this.BindCommand(this.ViewModel, vm => vm.DiscardCommand, v => v.Discard, vm => vm.SelectedElemental);

            this.WhenAnyValue(x => x.Hand.SelectedValue).Subscribe(x => Debug.WriteLine($"{x} is new selected hand item"));

            var dbgView = new DebugView();
            this.Loaded += (o, e) =>
            {
                dbgView.WindowStartupLocation = WindowStartupLocation.Manual;
                dbgView.Left = this.Left + this.Width;
                dbgView.Top = this.Top;
                dbgView.Owner = this;
                dbgView.Show();
            };
        }

        public GameViewModel? ViewModel
        {
            get { return (GameViewModel?)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(GameViewModel), typeof(MainWindow), new PropertyMetadata(null));

        object? IViewFor.ViewModel { get => this.ViewModel; set => this.ViewModel = (GameViewModel)value; }
    }
}
