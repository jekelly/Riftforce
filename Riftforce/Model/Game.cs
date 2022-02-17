using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using DynamicData;
using Newtonsoft.Json;

namespace Riftforce
{
    // Game is a stream of player inputs.
    // Game state can be reconstructed by replaying those inputs on the same starting state.
    // Certain operations are guaranteed and don't need to be represented as moves (i.e. changing active player).
    // Player views of the game are scoped to what that player can know (so not opponents hand content).

    // GameState itself is simple in-memory representation of current game state.
    // GameEngine is the logic to apply moves to state to produce new state, based on the game rules.

    /// <summary>
    /// Server-side game state. Aware of all context. Converted to PlayerGameState before sharing.
    /// </summary>
    public class GameState
    {
        public int GameId { get; init; }
        public uint[] Scores { get; } = new uint[2];

        public Phase Phase { get; set; } = Phase.Main;
        public int ActivePlayerIndex { get; set; }

        public Deck[] Decks { get; }
        public List<Elemental>[] Hands { get; } = new List<Elemental>[2];
        public Location[] Locations { get; } = new Location[5];

        public List<uint> UsedElementals { get; } = new List<uint>(3);
        public List<uint> UsedLocations { get; } = new List<uint>(3);
        public uint Discard { get; set; } = Elemental.NoneId;

        [JsonConstructor]
        public GameState(Deck[] decks)
        {
            this.Decks = decks;
            this.Hands[0] = new List<Elemental>();
            this.Hands[1] = new List<Elemental>();
            this.Decks[0].Shuffle();
            this.Decks[1].Shuffle();

            for (uint i = 0; i < 5; i++)
            {
                this.Locations[i] = new Location(i);
            }
        }

        public GameState(IEnumerable<Elemental>[] decks) : this(decks.Select(l => new Deck(l.Select(x => x.Id))).ToArray())
        {
        }
    }


    public class Game
    {
        private GameState state;

        private uint Discard
        {
            get => this.state.Discard;
            set => this.state.Discard = value;
        }

        public ElementalInPlay? FindElemental(uint id, uint side)
        {
            return this.Locations.SelectMany(location => location.Elementals[side].Where(eip => eip.Id == id)).SingleOrDefault();
        }

        public Phase Phase
        {
            get => this.state.Phase;
            set => this.state.Phase = value;
        }

        public uint[] Scores => this.state.Scores;

        private int turnCounter = 1;
        private BehaviorSubject<int> turn;
        public IObservable<int> Turn => this.turn;

        //public Player[] Players => this.state.Players;
        public Deck[] Decks => this.state.Decks;
        public List<Elemental>[] Hands => this.state.Hands;
        public Location[] Locations => this.state.Locations;

        public int ActivePlayerIndex
        {
            get => this.state.ActivePlayerIndex;
            set => this.state.ActivePlayerIndex = value;
        }

        //public Player ActivePlayer => this.Players[this.state.ActivePlayerIndex];

        private readonly BehaviorSubject<Game> update;
        public IObservable<Game> UpdateState => this.update;

        public GameState State
        {
            get => this.state;
            set => this.state = value;
        }

        private readonly BehaviorSubject<Game> minorUpdate;
        public IObservable<Game> MinorUpdate => this.minorUpdate;

        public List<uint> PlayedLocations => this.state.UsedLocations;
        public List<uint> UsedElementals => this.state.UsedElementals;

        public Game(GameState state)
        {
            this.state = state;
            this.update = new(this);
            this.minorUpdate = new(this);
            this.turn = new BehaviorSubject<int>(1);
        }

        public bool CanPlay(TargetLocation location)
        {
            if (this.Phase != Phase.TargetLocation) return false;
            return this.ActiveElemental?.CanTarget(this, location) ?? false;
        }

        public void ProcessMove(TargetElemental targetElemental)
        {
            this.Phase = this.ActiveElemental.Target(this, targetElemental);
            this.minorUpdate.OnNext(this);
        }

        public void ProcessMove(TargetLocation targetLocation)
        {
            this.Phase = this.ActiveElemental.Target(this, targetLocation);
            this.minorUpdate.OnNext(this);
        }

        public bool CanPlay(DrawAndScore move)
        {
            if (this.Phase != Phase.Main) return false;
            return this.Phase == Phase.Main && this.Hands[move.PlayerIndex].Count < 7;
        }

        public bool CanPlay(TargetElemental move)
        {
            if (this.Phase != Phase.TargetElemental) return false;
            if (this.ActiveElemental is null) return false;
            return this.ActiveElemental.CanTarget(this, move);
        }

        public bool CanPlay(PlayElemental move)
        {
            if (this.Phase != Phase.Main && this.Phase != Phase.Deploy) return false;

            if (move.PlayerIndex != this.ActivePlayerIndex)
            {
                return false;
            }

            if (move.LocationIndex > this.Locations.Length || move.LocationIndex < 0)
            {
                return false;
            }

            if (this.Hands[move.PlayerIndex].SingleOrDefault(e => e.Id == move.ElementalId) is null)
            {
                return false;
            }

            // check if we can play this elemental
            if (!this.MatchesStrengthOrGuild(move.PlayerIndex, move.ElementalId)) return false;

            // check if we can legally play here
            if (this.PlayedLocations is not null)
            {
                // if we've played only one card, we can play in the same spot or either adjacent spot
                if (this.PlayedLocations.Count == 1 && Math.Abs((int)move.LocationIndex - (int)this.PlayedLocations[0]) > 1)
                {
                    return false;
                }
                else if (this.PlayedLocations.Count == 2)
                {
                    // if both are the same, then new move must be same
                    if (this.PlayedLocations[0] == this.PlayedLocations[1] && this.PlayedLocations[0] != move.LocationIndex)
                    {
                        return false;
                    }
                    // else, bound on either side but don't allow either match
                    else if (this.PlayedLocations[0] != this.PlayedLocations[1])
                    {
                        if (move.LocationIndex == this.PlayedLocations[0] || move.LocationIndex == this.PlayedLocations[1]) return false;
                        if (Math.Abs((int)move.LocationIndex - (int)this.PlayedLocations[0]) != 1 && Math.Abs((int)move.LocationIndex - (int)this.PlayedLocations[1]) != 1) return false;
                    }
                }
            }

            return true;
        }

        public bool ProcessMove(PlayElemental move)
        {
            if (!CanPlay(move))
            {
                return false;
            }
            Log.WriteLine($"{move.PlayerIndex} plays {move.ElementalId}");

            this.Phase = Phase.Deploy;

            // remove from hand
            var elemental = this.Hands[this.ActivePlayerIndex].Single(e => e.Id == move.ElementalId);
            this.Hands[this.ActivePlayerIndex].Remove(elemental);
            var eip = this.Locations[move.LocationIndex].Add(elemental, move.PlayerIndex);

            eip.Guild.OnPlayed(this.Locations[move.LocationIndex], move.PlayerIndex);

            this.PlayedLocations.Add(move.LocationIndex);
            this.UsedElementals.Add(elemental.Id);

            this.CheckTurnEnd();

            return true;
        }

        public void EndTurn()
        {
            this.CompleteTurn();
        }

        private void CompleteTurn()
        {
            this.Phase = Phase.Main;
            this.Discard = Elemental.NoneId;
            this.ActiveElemental = null;
            this.UsedElementals.Clear();
            this.PlayedLocations.Clear();
            this.SwitchActivePlayer();
        }

        private void CheckTurnEnd()
        {
            int max = this.UsedElementals.Contains(this.Discard) ? 4 : 3;
            if (this.UsedElementals.Count >= max)
            {
                this.CompleteTurn();
            }

            this.minorUpdate.OnNext(this);
        }

        public bool CanPlay(DiscardAction move)
        {
            if (this.Phase != Phase.Main) return false;
            return this.Discard == Elemental.NoneId;
        }

        public void ProcessMove(DiscardAction move)
        {
            this.Phase = Phase.Activate;
            var elemental = this.Hands[this.ActivePlayerIndex].Single(e => e.Id == move.DiscardId);
            this.Hands[this.ActivePlayerIndex].Remove(elemental);
            this.Discard = move.DiscardId;
            this.Decks[this.ActivePlayerIndex].Discard(elemental);
            this.UsedElementals.Add(this.Discard);
            this.minorUpdate.OnNext(this);
        }

        private bool MatchesStrengthOrGuild(uint playerIndex, uint elementalId)
        {
            if (this.UsedElementals is not null)
            {
                Elemental elemental = Elemental.Lookup(elementalId);
                var usedElementals = this.UsedElementals.Select(e => Elemental.Lookup(e));
                bool matchesGuild = usedElementals.All(e => e.Guild == elemental.Guild);
                bool matchesStrength = usedElementals.All(e => e.Strength == elemental.Strength);
                if (!matchesGuild && !matchesStrength)
                {
                    return false;
                }
            }

            return true;
        }

        public ElementalInPlay? ActiveElemental { get; private set; }
        public bool HasUsedLightningThisTurn { get; set; }

        public bool CanActivate(ActivateElemental move)
        {
            if (this.Phase != Phase.Activate) return false;
            // must have a discard
            if (this.Discard == Elemental.NoneId) return false;
            // can't use the same elemental twice on a turn
            if (this.UsedElementals.Contains(move.ElementalId)) return false;
            // must match type or power of discarded card
            if (!this.MatchesStrengthOrGuild(move.PlayerIndex, move.ElementalId)) return false;
            // must be legal elemental, played on the board
            if (!this.Locations.Any(l => l.IsElementalPresent(move.ElementalId))) return false;
            // must meet requirements of specific elemental
            // TODO
            return true;
        }

        public bool ProcessMove(ActivateElemental move)
        {
            if (!CanActivate(move))
            {
                return false;
            }

            foreach (var location in this.Locations)
            {
                // setting a breakpoint - observe lag
                var elemental = location.Elementals[move.PlayerIndex].SingleOrDefault(e => e.Id == move.ElementalId);
                if (elemental is not null)
                {
                    this.Phase = elemental.Guild.Activate(location, elemental.Id, move.PlayerIndex);
                    this.UsedElementals.Add(elemental.Id);
                    this.ActiveElemental = elemental;
                    break;
                }
            }

            this.minorUpdate.OnNext(this);

            this.CheckTurnEnd();

            //this.DamageFirstAt(move.TargetLocation, move.TargetPlayer);


            return true;
        }

        public bool ProcessMove(DrawAndScore move)
        {
            const int HandSize = 7;
            while (this.Hands[move.PlayerIndex].Count < HandSize)
            {
                this.Draw(move.PlayerIndex);
            }

            int other = 1 - move.PlayerIndex;
            for (int i = 0; i < this.Locations.Length; i++)
            {
                if (this.Locations[i].Elementals[move.PlayerIndex].Any() && !this.Locations[i].Elementals[other].Any())
                {
                    this.Scores[move.PlayerIndex]++;
                }
            }

            // TODO: placeholder
            this.EndTurn();
            return true;
        }

        private void Draw(int playerIndex)
        {
            this.Hands[playerIndex].Add(this.Decks[playerIndex].Draw());
        }

        private void SwitchActivePlayer()
        {
            this.Phase = Phase.Main;
            this.ActivePlayerIndex = 1 - this.ActivePlayerIndex;
            this.turn.OnNext(++this.turnCounter);
            this.minorUpdate.OnNext(this);
            this.update.OnNext(this);
        }
    }
}
