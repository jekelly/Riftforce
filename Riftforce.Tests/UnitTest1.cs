using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Riftforce.Tests
{
    public class UnitTest1
    {
        private Elemental elemental1 = new Elemental(5, Guild.Plant);
        private Elemental elemental2 = new Elemental(6, Guild.Ice);
        private Elemental elemental3 = new Elemental(7, Guild.Shadow);

        [Fact]
        public void RoundTripGame()
        {
            GameBuilder gb = new GameBuilder();
            var game = gb.Build();
            var state = game.State;

            var serializedGame = JsonConvert.SerializeObject(state);
            var g = JsonConvert.DeserializeObject<GameState>(serializedGame);
        }
    }
}