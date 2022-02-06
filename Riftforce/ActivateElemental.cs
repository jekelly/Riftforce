namespace Riftforce
{
    public class ActivateElemental
    {
        public uint PlayerIndex { get; set; }
        public uint ElementalId { get; set; }
    }

    public class TargetElemental
    {
        public uint PlayerIndex { get; set; }
        public uint ElementalId { get; set; }
    }

    public class TargetLocation
    {
        public uint LocationIndex { get; set; }
    }

    public class DiscardAction
    {
        public uint DiscardId { get; set; }
    }
}
