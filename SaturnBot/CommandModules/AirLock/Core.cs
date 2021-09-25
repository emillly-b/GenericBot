using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using SaturnBot.Entities;

namespace SaturnBot.CommandModules.AirLock
{
    public class AirLockCore
    {
        public List<Guild> ActiveGuilds { get; set; }
        public Configuration Configuration { get; set; }
        public EventManager EventManager { get; set; }
        Logger Log { get; set; }

        public AirLockCore Load(DiscordShardedClient client)
        {
            if (!Database.Exists)
                Database.CreateDatabase();
            ActiveGuilds = Database.GetGuilds();
            Configuration = Database.GetConfiguration();
            EventManager = new EventManager(client, this);
            Log = Core.Logger;
            Log.LogGenericMessage(String.Format("Airlock Module Loaded. {0} Guilds Loaded", ActiveGuilds.Count));
            return this;
        }

    }
}
