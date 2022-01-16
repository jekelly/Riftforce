using System;

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

        public Game(Player[] players)
        {
            this.scores = new uint[2];
            this.players = players;
            this.locations = new Location[5];
            for (int i = 0; i < 5; i++)
            {
                this.locations[i] = new Location();
            }
        }

        public bool ProcessMove(PlayElemental move)
        {
            if (move.PlayerIndex != this.activePlayerIndex)
            {
                return false;
            }

            if (move.LocationIndex > this.locations.Length)
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

            // remove from hand
            var elemental = this.ActivePlayer.PlayFromHand(move.ElementalId);
            this.locations[move.LocationIndex].Add(elemental, move.PlayerIndex);
            // update remaining moves and move type
            this.moveType = typeof(PlayElemental);
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
