using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData;

namespace Riftforce
{
    public class Guild
    {
        private static int NextIndex;
        private readonly int index;
        private readonly string name;

        public string Name => this.name;

        public Guild(string name)
        {
            this.name = name;
            this.index = NextIndex++;
        }

        public static readonly Guild Fire = new Guild("Fire");
        public static readonly Guild Ice = new Guild("Ice");
        public static readonly Guild Light = new Guild("Light");
        public static readonly Guild Shadow = new Guild("Shadow");
        public static readonly Guild Earth = new Guild("Earth");
        public static readonly Guild Water = new Guild("Water");
        public static readonly Guild Thunder = new Guild("Thunder");
        public static readonly Guild Plant = new Guild("Plant");
        public static readonly Guild Air = new Guild("Air");
        public static readonly Guild Crystal = new Guild("Crystal");

        public static readonly IReadOnlyList<Guild> Guilds = new[] { Fire, Ice, Light, Shadow, Earth, Water, Thunder, Plant, Air, Crystal };
    }

    public class Elemental
    {
        private static uint NextId;
        public uint Id { get; private set; }
        public Guild Guild { get; private set; }
        public uint Strength { get; private set; }
        public uint Damage { get; set; }

        public Elemental(uint strength, Guild guild)
        {
            this.Id = NextId++;
            this.Guild = guild;
            this.Strength = strength;
            this.Damage = 0;
        }
    }

    public class Location
    {
        private readonly SourceCache<Elemental, uint>[] elementals;
        public IObservableCache<Elemental, uint>[] Elementals { get; }

        public Location()
        {
            this.elementals = new SourceCache<Elemental, uint>[2];
            this.elementals[0] = new SourceCache<Elemental, uint>(e => e.Id);
            this.elementals[1] = new SourceCache<Elemental, uint>(e => e.Id);
            this.Elementals = this.elementals.Select(e => e.AsObservableCache()).ToArray();
        }

        public void Add(Elemental elemental, uint side)
        {
            this.elementals[side].AddOrUpdate(elemental);
        }
    }

    public class Deck<T>
    {
        private static readonly Random r = new Random();
        private readonly IReadOnlyList<T> deck;
        private readonly Queue<T> draw;
        private readonly Stack<T> discard;

        public Deck(IEnumerable<T> cards)
        {
            this.deck = new List<T>(cards).AsReadOnly();
            this.discard = new Stack<T>();
            this.draw = new Queue<T>();
        }

        public void Shuffle()
        {
            var shuffledDeck = new List<T>(this.deck);
            for (int i = this.deck.Count - 1; i > 0; i--)
            {
                int index = r.Next(i);
                T value = shuffledDeck[index];
                shuffledDeck[index] = shuffledDeck[i];
                shuffledDeck[i] = value;
            }

            this.draw.Clear();
            for (int i = 0; i < shuffledDeck.Count; i++)
            {
                this.draw.Enqueue(shuffledDeck[i]);
            }
        }

        public T Draw()
        {
            return this.draw.Dequeue();
        }
    }

    public class Player
    {
        private readonly Deck<Elemental> elementals;
        private readonly SourceCache<Elemental, uint> hand;

        public IObservableCache<Elemental, uint> Hand => this.hand.AsObservableCache();

        public Player(List<Elemental> deck)
        {
            this.elementals = new Deck<Elemental>(deck);
            this.elementals.Shuffle();
            this.hand = new SourceCache<Elemental, uint>(e => e.Id);
        }

        public Elemental Draw()
        {
            return this.elementals.Draw();
        }

        public void DrawToHand()
        {
            this.hand.AddOrUpdate(this.elementals.Draw());
        }

        public Elemental PlayFromHand(uint id)
        {
            var value = this.hand.Lookup(id).Value;
            this.hand.RemoveKey(id);
            return value;
        }
    }

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

    public class PlayElemental
    {
        public uint PlayerIndex { get; set; }
        public uint ElementalId { get; set; }
        public uint LocationIndex { get; set; }
    }

    public class ActivateElemental
    {
    }

    public class DrawAndScore
    {
        public int PlayerIndex { get; set; }
    }

    public class GameBuilder
    {
        public Game Build()
        {
            // TODO: eventually, draft guilds, for now, random it
            var guilds = new List<Guild>(Guild.Guilds);
            var random = new Random();
            var decks = new List<Elemental>[2];
            decks[0] = new List<Elemental>();
            decks[1] = new List<Elemental>();

            for (int i = guilds.Count - 1, picks = 8; picks > 0; picks--)
            {
                var ind = random.Next(i);
                var selectedGuild = guilds[ind];
                for (int j = 0; j < 4; j++)
                {
                    decks[picks % 2].Add(new Elemental(5, selectedGuild));
                }
                for (int j = 0; j < 3; j++)
                {
                    decks[picks % 2].Add(new Elemental(6, selectedGuild));
                }
                for (int j = 0; j < 2; j++)
                {
                    decks[picks % 2].Add(new Elemental(7, selectedGuild));
                }
                guilds[ind] = guilds[i];
                guilds[i] = selectedGuild;
            }

            var player1 = new Player(decks[0]);
            var player2 = new Player(decks[1]);

            for (int i = 0; i < 7; i++)
            {
                player1.DrawToHand();
                player2.DrawToHand();
            }

            var game = new Game(new[] { player1, player2 });

            game.Locations[2].Add(player2.Draw(), 1);

            return game;
        }
    }
}
