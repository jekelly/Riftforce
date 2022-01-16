using System;
using System.Collections.Generic;

namespace Riftforce
{
    public class Game
    {
        private readonly uint[] scores;
        private readonly Location[] locations;
        private readonly Player[] players;

        private int activePlayerIndex;
        private Player ActivePlayer => this.players[this.activePlayerIndex];

        private Type moveType;
        private uint remainingMoves;

        public Player[] Players => this.players;

        public Location[] Locations => this.locations;

        private readonly List<uint> playedLocations;

        public Game(Player[] players)
        {
            this.scores = new uint[2];
            this.players = players;
            this.locations = new Location[5];
            for (int i = 0; i < 5; i++)
            {
                this.locations[i] = new Location();
            }
            this.playedLocations = new List<uint>(3);
        }

        public bool CanPlay(PlayElemental move)
        {
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

            // remove from hand
            var elemental = this.ActivePlayer.PlayFromHand(move.ElementalId);
            this.locations[move.LocationIndex].Add(elemental, move.PlayerIndex);
            // update remaining moves and move type
            this.moveType = typeof(PlayElemental);

            this.playedLocations.Add(move.LocationIndex);

            this.remainingMoves--;

            return true;
        }

        public bool ProcessMove(ActivateElemental move)
        {
            return true;
        }

        public bool ProcessMove(DrawAndScore move)
        {
            return true;
        }
    }
}
