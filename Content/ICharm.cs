namespace CupAPI.Content {
    public interface ICharm
    {
        public string DisplayName { get; }
        public string Subtext { get; }
        public string Description { get; }
        public string IconBundle { get; }
    }
}