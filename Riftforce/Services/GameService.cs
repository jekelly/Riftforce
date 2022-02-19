using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData;

namespace Riftforce
{
    public class GameRecord
    {
        public string Id { get; }
        public string[] Players { get; }
        public GameState State { get; }

        public GameRecord()
        {
            this.Id = Guid.NewGuid().ToString();
            this.Players = Enumerable.Repeat<int>(2, 2).Select(x => Guid.NewGuid().ToString()).ToArray();
            this.State = new GameBuilder().BuildState();
        }

        public PlayerView GetView(string playerId)
        {
            var index = this.Players.IndexOf(playerId);
            return new PlayerView(this.State, index);
        }
    }

    public class Games
    {
        private readonly SourceCache<GameRecord, string> store;

        public Games()
        {
            this.store = new SourceCache<GameRecord, string>(record => record.Id);
        }

        public GameRecord CreateGame()
        {
            var game = new GameRecord();
            this.store.AddOrUpdate(game);
            return game;
        }

        public PlayerView GetState(string gameId, string playerId)
        {
            var record = this.store.Lookup(gameId);
            return record.Value.GetView(playerId);
        }
    }

    public class PlayerView
    {
        public int GameId { get; init; }
        public uint[] Scores { get; } = new uint[2];

        public Phase Phase { get; set; } = Phase.Main;
        public int ActivePlayerIndex { get; set; }

        public Deck Deck { get; set; }
        public PublicDeckInfo OpponentDeckInfo { get; set; }

        public List<Elemental> Hand { get; }
        public int OpponentCardsInHand { get; }
        public Location[] Locations { get; } = new Location[5];

        public List<uint> UsedElementals { get; } = new List<uint>(3);
        public List<uint> UsedLocations { get; } = new List<uint>(3);
        public uint Discard { get; set; } = Elemental.NoneId;

        public PlayerView(GameState state, int playerIndex)
        {
            this.GameId = state.GameId;
            this.Scores = state.Scores;
            this.Phase = state.Phase;
            this.ActivePlayerIndex = state.ActivePlayerIndex;
            this.Deck = state.Decks[playerIndex];
            this.OpponentDeckInfo = new PublicDeckInfo(state.Decks[1 - playerIndex]);
            this.Hand = state.Hands[playerIndex];
            this.OpponentCardsInHand = state.Hands[1 - playerIndex].Count;
            this.Locations = state.Locations;
            this.UsedElementals = state.UsedElementals;
            this.UsedLocations = state.UsedLocations;
            this.Discard = state.Discard;
        }
    }
}
