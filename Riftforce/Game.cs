﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;

namespace Riftforce
{
    public enum Phase
    {
        Main,
        Deploy,
        Activate,
        TargetElemental,
        TargetLocation
    }

    public class Game
    {
        public ElementalInPlay? FindElemental(uint id, uint side)
        {
            return this.Locations.SelectMany(location => location.Elementals[side].Where(eip => eip.Id == id)).SingleOrDefault();
        }

        private readonly uint[] scores;
        private readonly Location[] locations;
        private readonly Player[] players;

        public Phase Phase { get; set; }

        public uint[] Scores => this.scores;

        private int turnCounter = 1;
        private BehaviorSubject<int> turn;
        public IObservable<int> Turn => this.turn;

        private int activePlayerIndex;
        public Player ActivePlayer => this.players[this.activePlayerIndex];

        private readonly BehaviorSubject<Game> update;
        public IObservable<Game> UpdateState => this.update;

        private readonly BehaviorSubject<Game> minorUpdate;
        public IObservable<Game> MinorUpdate => this.minorUpdate;

        private Type moveType;

        public Player[] Players => this.players;

        public Location[] Locations => this.locations;

        private readonly List<uint> playedLocations;
        private readonly List<Elemental> usedElementals;

        public Game(Player[] players)
        {
            this.scores = new uint[2];
            this.players = players;
            this.locations = new Location[5];
            for (uint i = 0; i < 5; i++)
            {
                this.locations[i] = new Location(i);
            }
            this.playedLocations = new List<uint>(3);
            this.usedElementals = new List<Elemental>(3);
            this.update = new(this);
            this.minorUpdate = new(this);
            this.turn = new BehaviorSubject<int>(1);
            this.Phase = Phase.Main;
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
            return this.moveType is null && this.players[move.PlayerIndex].Hand.Count < 7;
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

            if (move.PlayerIndex != this.activePlayerIndex)
            {
                return false;
            }

            if (move.LocationIndex > this.locations.Length || move.LocationIndex < 0)
            {
                return false;
            }

            if (!this.ActivePlayer.Hand.Lookup(move.ElementalId).HasValue)
            {
                return false;
            }

            if (moveType is object && !typeof(PlayElemental).IsAssignableFrom(this.moveType))
            {
                // TODO: test this
                return false;
            }

            // check if we can play this elemental
            if (!this.MatchesStrengthOrGuild(move.PlayerIndex, move.ElementalId)) return false;

            // check if we can legally play here
            if (this.playedLocations is not null)
            {
                // if we've played only one card, we can play in the same spot or either adjacent spot
                if (this.playedLocations.Count == 1 && Math.Abs((int)move.LocationIndex - (int)this.playedLocations[0]) > 1)
                {
                    return false;
                }
                else if (this.playedLocations.Count == 2)
                {
                    // if both are the same, then new move must be same
                    if (this.playedLocations[0] == this.playedLocations[1] && this.playedLocations[0] != move.LocationIndex)
                    {
                        return false;
                    }
                    // else, bound on either side but don't allow either match
                    else if (this.playedLocations[0] != this.playedLocations[1])
                    {
                        if (move.LocationIndex == this.playedLocations[0] || move.LocationIndex == this.playedLocations[1]) return false;
                        if (Math.Abs((int)move.LocationIndex - (int)this.playedLocations[0]) != 1 && Math.Abs((int)move.LocationIndex - (int)this.playedLocations[1]) != 1) return false;
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

            this.Phase = Phase.Deploy;

            // remove from hand
            var elemental = this.ActivePlayer.PullFromHand(move.ElementalId);
            var eip = this.locations[move.LocationIndex].Add(elemental, move.PlayerIndex);

            eip.Elemental.Guild.OnPlayed(this.locations[move.LocationIndex], move.PlayerIndex);

            // update remaining moves and move type
            this.moveType = typeof(PlayElemental);

            this.playedLocations.Add(move.LocationIndex);
            this.usedElementals.Add(elemental);

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
            this.moveType = null;
            this.discard = null;
            this.ActiveElemental = null;
            this.usedElementals.Clear();
            this.playedLocations.Clear();
            this.SwitchActivePlayer();
        }

        private void CheckTurnEnd()
        {
            int max = this.usedElementals.Contains(this.discard) ? 4 : 3;
            if (this.usedElementals.Count >= max)
            {
                this.CompleteTurn();
            }

            this.minorUpdate.OnNext(this);
        }

        private Elemental discard;
        public bool CanPlay(DiscardAction move)
        {
            if (this.Phase != Phase.Main) return false;
            return this.discard is null;
        }

        public void ProcessMove(DiscardAction move)
        {
            this.Phase = Phase.Activate;
            this.discard = this.ActivePlayer.PullFromHand(move.DiscardId);
            this.ActivePlayer.Discard(this.discard);
            this.usedElementals.Add(this.discard);
            this.minorUpdate.OnNext(this);
        }

        private bool MatchesStrengthOrGuild(uint playerIndex, uint elementalId)
        {
            if (this.usedElementals is not null)
            {
                Elemental elemental = Elemental.Lookup(elementalId);
                bool matchesGuild = this.usedElementals.All(e => e.Guild == elemental.Guild);
                bool matchesStrength = this.usedElementals.All(e => e.Strength == elemental.Strength);
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
            if (this.discard is null) return false;
            // can't use the same elemental twice on a turn
            if (this.usedElementals.Select(e => e.Id).Contains(move.ElementalId)) return false;
            // must match type or power of discarded card
            var elemental = this.players[move.PlayerIndex].Hand.Lookup(move.ElementalId);
            if (!this.MatchesStrengthOrGuild(move.PlayerIndex, move.ElementalId)) return false;
            // must be legal elemental, played on the board
            if (!this.locations.Any(l => l.IsElementalPresent(move.ElementalId))) return false;
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
                    this.Phase = elemental.Elemental.Guild.Activate(location, elemental.Elemental, move.PlayerIndex);
                    this.usedElementals.Add(elemental.Elemental);
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
            var player = this.players[move.PlayerIndex];
            const int HandSize = 7;
            while (player.Hand.Count < HandSize)
            {
                this.players[move.PlayerIndex].DrawToHand();
            }

            int other = 1 - move.PlayerIndex;
            for (int i = 0; i < this.locations.Length; i++)
            {
                if (this.locations[i].Elementals[move.PlayerIndex].Any() && !this.locations[i].Elementals[other].Any())
                {
                    this.scores[move.PlayerIndex]++;
                }
            }

            // TODO: placeholder
            this.EndTurn();
            return true;
        }

        private void SwitchActivePlayer()
        {
            this.Phase = Phase.Main;
            this.activePlayerIndex = 1 - this.activePlayerIndex;
            this.turn.OnNext(++this.turnCounter);
            this.update.OnNext(this);
        }
    }

    //class CrystalActivation
    //{
    //    public bool CanActivate()
    //    {

    //    }

    //    public bool Activate(Game game)
    //    {
    //    }
    //}
}
