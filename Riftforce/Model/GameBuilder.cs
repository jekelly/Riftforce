using System;
using System.Linq;
using System.Reactive.Linq;
using System.Collections.Generic;

namespace Riftforce
{
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

            var game = new Game(new GameState(decks));

            for (int i = 0; i < 7; i++)
            {
                game.Hands[0].Add(game.Decks[0].Draw());
                game.Hands[1].Add(game.Decks[1].Draw());
            }

            // rudimentary AI
            game.UpdateState
                .Where(x => x.ActivePlayerIndex == 1)
                .Subscribe(g =>
                {
                    g.ProcessMove(new DrawAndScore() { PlayerIndex = 1 });
                });
                    


            game.Locations[2].Add(game.Decks[1].Draw(), 1);

            return game;
        }
    }
}
