using MemoryPack;
using System;
using System.Collections.Generic;

namespace Flow.Launcher.Plugin.Zoxide.Models
{
    [MemoryPackable]
    public partial class CachedZoxideEntries
    {
        [MemoryPackOrder(0)]
        public DateTime CachedAt { get; set; }

        [MemoryPackOrder(1)]
        public List<ZoxideEntry> Entries { get; set; } = [];
    }
}
