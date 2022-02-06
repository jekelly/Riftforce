using FluentAssertions;
using Xunit;

namespace Riftforce.Tests
{
    public class UnitTest1
    {
        private Elemental elemental1 = new Elemental(5, Guild.Plant);
        private Elemental elemental2 = new Elemental(6, Guild.Ice);
        private Elemental elemental3 = new Elemental(7, Guild.Shadow);

        [Fact]
        public void Test1()
        {
            LocationSide target = new LocationSide(0);
            target.Play(elemental1);
            var x = target.Play(elemental2);
            var y = target.Play(elemental3);
            target.Remove(x);
            y.Index.Should().Be(1);
        }
    }
}