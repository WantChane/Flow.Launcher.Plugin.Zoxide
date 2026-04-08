using System.Collections.Generic;

namespace Flow.Launcher.Plugin.Zoxide
{
    public class Zoxide : IPlugin
    {
        private PluginInitContext _context = null!;

        public void Init(PluginInitContext context)
        {
            _context = context;
        }

        public List<Result> Query(Query query)
        {
            return new List<Result>();
        }
    }
}