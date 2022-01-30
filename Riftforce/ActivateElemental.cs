namespace Riftforce
{
    public class ActivateElemental
    {
        public uint PlayerIndex { get; set; }
        public uint ElementalId { get; set; }
        public uint TargetLocation { get; set; }
        public uint TargetEnemy { get; set; }
        public uint TargetPlayer { get; internal set; }
    }

    public class DiscardAction
    {
        public uint DiscardId { get; set; }
    }
}
