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
        public DiscordShardedClient Client { get; private set; }
        Logger Log { get; set; }

        public AirLockCore Load(DiscordShardedClient client)
        {
            Client = client;
            if (!Database.Exists)
                Database.CreateDatabase();
            ActiveGuilds = Database.GetGuilds();
            Configuration = Database.GetConfiguration();
            EventManager = new EventManager(Client, this);
            Log = Core.Logger;
            Log.LogGenericMessage(String.Format("Airlock Module Loaded. {0} Guilds Loaded", ActiveGuilds.Count));
            return this;
        }
        public async Task UpdateIntros(ParsedCommand context)
        {
            var channel = Client.GetChannel(Configuration.SafeChannelId) as IMessageChannel;
            var messages = channel.GetMessagesAsync(100);
            int count = 0;
            await foreach(IMessage intro in messages)
            {
                ulong _id = intro.Author.Id;                
                string permlink = intro.GetJumpUrl();
                foreach(Guild g in ActiveGuilds)
                {
                    User tempUser = new User(_id);
                    if (g.Members.Contains(tempUser))
                    {
                        int location = g.Members.IndexOf(tempUser);
                        g.Members[location].IntroURL = intro.GetJumpUrl();
                        g.Members[location].Authorized = true;
                        count++;
                    }
                }
            }
            context.Message.ReplyAsync("Intros updated: {count}");
        }

    }
}
