using Discord;
using Discord.API;
using SaturnBot.Entities;

using System.Collections.Generic;
using System.Threading.Tasks;


namespace SaturnBot.CommandModules.AirLock
{
    class CommandHandler : Module
    {
        public List<ulong> MirroredChannels { get; set;}
        public List<Command> Load()
        {
            
            List<Command> commands = new List<Command>();

            Command Airlock = new Command("airlock")
            {
                Description = "Airlock System",
                Usage = "TODO",
                RequiredPermission = Command.PermissionLevels.Admin
            };
            Airlock.ToExecute+= async (context) =>
            {
                if(context.Parameters.Count == 0) {
                    await context.Message.ReplyAsync("Unknown option");
                    return;
                }
                switch(context.Parameters[0]) {
                    
                    case "config":
                        await ConfigurationHandler(context);
                        break;
                    case "help":
                        await HelpHandler(context);
                        break;                    
                    default:
                        await context.Message.ReplyAsync("Unknown option");
                        break;
                }        
            };
            commands.Add(Airlock);

            Command Welcome = new Command("welcome")
            {
                Description = "Airlock Welcome Command",
                Usage = Core.GetGlobalPrefix() + "welcome @user",
                Delete = true,
                RequiredPermission = Command.PermissionLevels.Moderator
            };
            Welcome.ToExecute += async (context) =>
            {
                if (!Core.Airlock.GetGuild(context.Guild.Id).Enabled)
                {
                    context.Channel.TriggerTypingAsync();
                    context.Channel.SendMessageAsync("Airlock is not enabled in this server!");
                    return;
                }
                if (ulong.TryParse(context.Parameters[0], out ulong id))
                {
                    var client = Core.Airlock.Client;
                    var guild = Core.Airlock.GetGuild(context.Guild.Id);
                    var discordGuild = client.GetGuild(context.Guild.Id);
                    var airlockUser = guild.GetUser(id);
                    var channel = client.GetChannel(guild.UnsafeChannelId) as IMessageChannel;
                    var safeChannel = client.GetChannel(guild.SafeChannelId) as IMessageChannel;
                    var enumerator = channel.GetMessagesAsync(100).Flatten().GetAsyncEnumerator();
                    var guildUserEnum = discordGuild.GetUsersAsync().Flatten().GetAsyncEnumerator();
                    IGuildUser guildUser = null;
                    while (await guildUserEnum.MoveNextAsync())
                    {
                        if (id == guildUserEnum.Current.Id)
                            guildUser = guildUserEnum.Current;
                    }
                    while (await enumerator.MoveNextAsync())
                    {
                        if (enumerator.Current.Author.Id == id)
                        {
                            airlockUser.Authorized = true;
                            airlockUser.IntroID = enumerator.Current.Id;
                            airlockUser.IntoContent = enumerator.Current.Content;
                            guildUser.AddRoleAsync(guild.SafeRoleId);
                            guildUser.RemoveRoleAsync(guild.UnsafeRoleId);
                            safeChannel.TriggerTypingAsync();
                            safeChannel.SendMessageAsync("", embed: Utility.GetIntroEmbed(airlockUser, guildUser));
                            if (guild.DeleteIntros)
                                enumerator.Current.DeleteAsync();
                        }
                    }                    
                    enumerator.DisposeAsync();
                }
            };
            commands.Add(Welcome);
            return commands;
        }
        public async Task HelpHandler(ParsedCommand context)
        {
            var builder = new EmbedBuilder()
                    .WithTitle("Saturn Bot: Airlock Help Information")
                    .WithUrl("https://github.com/emillly-b/SaturnBot")
                    .WithColor(new Color(0xEF4347))
                    .WithFooter(new EmbedFooterBuilder().WithText($"If you have questions or notice any errors, please contact {Core.DiscordClient.GetUser(Core.GetOwnerId()).ToString()}"))
                    .AddField("Airlock Enabled?:", Core.Airlock.GetGuild(context.Guild.Id).Enabled);
                var embed = builder.Build();

                await context.Channel.SendMessageAsync("", embed: embed);
        }
        public async Task ConfigurationHandler(ParsedCommand context)
        {
            ulong currentGuild = context.Guild.Id;
            switch (context.Parameters[1])
            {
                case "enable":
                    context.Message.ReplyAsync("Airlock Enabled");
                    Core.Airlock.GetGuild(currentGuild).Enabled = true;
                    break;
                case "disable":
                    context.Message.ReplyAsync("Airlock Disabled");
                    Core.Airlock.GetGuild(currentGuild).Enabled = false;
                    break;
                case "update":
                    Core.Airlock.UpdateUsers();
                    Database.SaveGuilds(Core.Airlock.ActiveGuilds);
                    break;
                case "show":
                    PrintAirlockConfiguration(context);
                    break;
                case "save":
                    Database.SaveConfiguration(Core.Airlock.Configuration);
                    Database.SaveGuilds(Core.Airlock.ActiveGuilds);
                    context.Message.ReplyAsync("Config Saved");
                    break;
                case "":
                    context.Message.ReplyAsync("Printing help...");
                    break;
                case "role":
                    {
                        switch (context.Parameters[2])
                        {
                            case "safe":
                                {
                                    Core.Airlock.GetGuild(currentGuild).SafeRoleId = ulong.Parse(context.Parameters[3]);
                                    context.Message.ReplyAsync("Safe Role ID updated too " + context.Parameters[3]);
                                    break;
                                }
                            case "unsafe":
                                {
                                    Core.Airlock.GetGuild(currentGuild).UnsafeRoleId = ulong.Parse(context.Parameters[3]);
                                    context.Message.ReplyAsync("Unsafe Role ID updated too " + context.Parameters[3]);
                                    break;
                                }
                        }
                        break;
                    }
                case "zone":
                    {
                        switch (context.Parameters[2])
                        {
                            case "safe":
                            {
                                Core.Airlock.GetGuild(currentGuild).SafeChannelId = ulong.Parse(context.Parameters[3]);
                                context.Message.ReplyAsync("Safe channel ID updated too " + context.Parameters[3]);
                                break;
                            }
                            case "unsafe":
                            {
                                Core.Airlock.GetGuild(currentGuild).UnsafeChannelId = ulong.Parse(context.Parameters[3]);
                                context.Message.ReplyAsync("Unsafe channel ID updated too " + context.Parameters[3]);
                                break;
                            }                                
                        }
                        break;
                    }
                    
            }
        }
        public async Task PrintAirlockConfiguration(ParsedCommand context)
        {
            var builder = new EmbedBuilder()
                    .WithTitle("Saturn Bot: Airlock Configuration Information")
                    .WithUrl("https://github.com/emillly-b/SaturnBot")
                    .WithColor(new Color(0xEF4347))
                    .WithFooter(new EmbedFooterBuilder().WithText($"If you have questions or notice any errors, please contact {Core.DiscordClient.GetUser(Core.GetOwnerId()).ToString()}"))
                    .AddField("Airlock Enabled?:", Core.Airlock.GetGuild(context.Guild.Id).Enabled);
                var embed = builder.Build();

                await context.Channel.SendMessageAsync("", embed: embed);
        }
    }
}
