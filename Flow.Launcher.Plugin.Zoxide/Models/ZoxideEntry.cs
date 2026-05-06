namespace Flow.Launcher.Plugin.Zoxide.Models
{
    [MemoryPack.MemoryPackable]
    public partial class ZoxideEntry
    {
        [MemoryPack.MemoryPackOrder(0)]
        public string Path { get; set; } = string.Empty;

        [MemoryPack.MemoryPackOrder(1)]
        public int Score { get; set; }
    }
}
