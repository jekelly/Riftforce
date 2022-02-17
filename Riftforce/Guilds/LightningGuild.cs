namespace Riftforce
{
    public class LightningGuild : Guild
    {
        public LightningGuild() : base("Lightning")
        {
        }

        public override Phase Activate(Location location, Elemental elemental, uint playerIndex) => Phase.TargetElemental;

        public override bool CanTarget(Game game, TargetElemental move)
        {
            return game.FindElemental(move.ElementalId, move.PlayerIndex)?.Location == game.ActiveElemental?.Location;
        }

        public override Phase Target(Game game, TargetElemental move)
        {
            var elemental = game.FindElemental(move.ElementalId, move.PlayerIndex);
            bool willKillTarget = (elemental.Strength - elemental.CurrentDamage - 2) >= 0;
            game.Locations[elemental.Location].ApplyDamageToSpecificIndex(move.PlayerIndex, elemental.Index, 2);
            if (willKillTarget && !game.HasUsedLightningThisTurn)
            {
                game.HasUsedLightningThisTurn = true;
                return Phase.TargetElemental;
            }

            game.HasUsedLightningThisTurn = false;
            return Phase.Activate;
        }
    }
}
