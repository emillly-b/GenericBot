using System;
using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaturnBot.CommandModules.AirLock
{
    public class EventManager
    {
        public DiscordShardedClient Client { get; set; }
        public AirLockCore Core { get; set; }

        public EventManager(DiscordShardedClient client, AirLockCore core)
        {
            Client = client;
            Core = core;
            RegisterHandlers();
        }
        public void RegisterHandlers()
        {
            Client.MessageReceived += AirlockMessageRecieved;
            Client.GuildAvailable += GuildJoinEvent;
            SaturnBot.Core.Logger.LogGenericMessage("Airlock Event Handlers Registered");
        }
        public async Task AirlockMessageRecieved(SocketMessage parameterMessage)
        {
            if (parameterMessage.Author.IsBot) { return; }
            if (parameterMessage.Author.IsWebhook) { return; }

            SocketGuild _guild = parameterMessage.GetGuild();
            if (Core.ActiveGuilds.Count == 0)
            {
                Core.ActiveGuilds.Add(new Guild(_guild.Id));
            }
            foreach (Guild _cursor in Core.ActiveGuilds)
            {
                if (_cursor.Id == _guild.Id)
                {
                    _cursor.Members.Add(new User(parameterMessage.Author.Id));
                }
            }
            Database.SaveGuilds(Core.ActiveGuilds);
        }

        public async Task GuildJoinEvent(SocketGuild guild)
        {
            if(Core.ActiveGuilds.Contains(new Guild(guild.Id)))
            {
                return;
            }
            else
            {
                Core.ActiveGuilds.Add(new Guild(guild.Id));
            }
        }
    }
}
