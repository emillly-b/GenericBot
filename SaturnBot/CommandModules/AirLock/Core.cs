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
        public bool FirstRun { get; private set; }
        public int TotalUsers { get; set; }
        Logger Log { get; set; }

        public AirLockCore Load(DiscordShardedClient client)
        {
            Client = client;
            if (!Database.Exists)
            {
                Database.CreateDatabase();
                FirstRun = true;
            }
            ActiveGuilds = Database.GetGuilds();
            Configuration = Database.GetConfiguration();            
            if(FirstRun)
            {
                Database.SaveConfiguration(Configuration);
                Database.SaveGuilds(ActiveGuilds);
            }
            UpdateUsers();
            EventManager = new EventManager(Client, this);
            Log = Core.Logger;
            Log.LogGenericMessage(String.Format("Airlock Module Loaded. {0} Guilds Loaded", ActiveGuilds.Count));
            return this;
        }
        public async Task UpdateIntros(ParsedCommand context)
        {
            ulong cxtguildID = context.Guild.Id; 
            var channel = Client.GetChannel(Core.Airlock.GetGuild(cxtguildID).SafeChannelId) as IMessageChannel;
            var enumerator = channel.GetMessagesAsync(100).Flatten().GetAsyncEnumerator();
            int count = 0;
            while(await enumerator.MoveNextAsync())
            {
                ulong userId = enumerator.Current.Author.Id;
                ulong messageId = enumerator.Current.Id;
                Guild g = Core.Airlock.GetGuild(context.Guild.Id);              
                User tempUser = new User(userId);
                if (g.Members.Contains(tempUser))
                {
                    int location = g.Members.IndexOf(tempUser);
                    g.Members[location].IntroID = messageId;
                    g.Members[location].Authorized = true;
                    count++;
                }
                else {
                    g.Members.Add(new User(userId, messageId));
                }
            }
            context.Message.ReplyAsync("Intros updated: " + count);
            if(count < 100)
            {
                context.Message.ReplyAsync("This will not work for more than 100 intros. let emily know to fix this");
            }
        }

        public async Task UpdateUsers()
        {
            foreach(Guild current in ActiveGuilds)
            {
                List<User> users = current.Members;
                var serverGuild = Client.GetGuild(current.Id);
                var enumerator = serverGuild.GetUsersAsync().Flatten().GetAsyncEnumerator();
                while(await enumerator.MoveNextAsync())
                {
                    users.Add(new User(enumerator.Current.Id));
                }
                current.Members = users;
            }
        }

        public Guild GetGuild(ulong id)
        {
            var temp = new Guild(id);
            if (ActiveGuilds.Contains(temp))
            {
                int index = ActiveGuilds.IndexOf(temp);
                return ActiveGuilds[index];
            }
            throw new Exception("Guild not found.");
        }
    }
}
