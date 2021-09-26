using Discord;
using Discord.API;
using SaturnBot.Entities;

using System.Collections.Generic;
using System.Threading.Tasks;


namespace SaturnBot.CommandModules.AirLock
{
    class CommandHandler : Module
    {

        public bool Enabled { get; set;}
        public List<ulong> MirroredChannels { get; set;}
        public List<Command> Load()
        {
            
            List<Command> commands = new List<Command>();

            Command Airlock = new Command("airlock");
            Airlock.Description = "Airlock System";
            Airlock.Usage = "TODO";
            Airlock.RequiredPermission = Command.PermissionLevels.Admin;
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

            return commands;
        }
        public async Task HelpHandler(ParsedCommand context)
        {
            var builder = new EmbedBuilder()
                    .WithTitle("Saturn Bot: Airlock Help Information")
                    .WithUrl("https://github.com/emillly-b/SaturnBot")
                    .WithColor(new Color(0xEF4347))
                    .WithFooter(new EmbedFooterBuilder().WithText($"If you have questions or notice any errors, please contact {Core.DiscordClient.GetUser(Core.GetOwnerId()).ToString()}"))
                    .AddField("Airlock Enabled?:", Enabled);
                var embed = builder.Build();

                await context.Channel.SendMessageAsync("", embed: embed);
        }
        public async Task ConfigurationHandler(ParsedCommand context)
        {
            switch(context.Parameters[1])
            {
                    case "enable":
                        context.Message.ReplyAsync("Airlock Enabled");
                        Enabled = true;
                        break;
                    case "disable":
                        context.Message.ReplyAsync("Airlock Disabled");
                        Enabled = false;
                        break;
                    case "role":
                    {
                        switch (context.Parameters[2])
                        {
                            case "safe":
                                {
                                    Core.Airlock.Configuration.SafeChannelId = ulong.Parse(context.Parameters[3]);
                                    context.Message.ReplyAsync("Safe Role ID updated too " + context.Parameters[3]);
                                    break;
                                }
                            case "unsafe":
                                {
                                    Core.Airlock.Configuration.UnsafeChannelId = ulong.Parse(context.Parameters[3]);
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
                                Core.Airlock.Configuration.SafeChannelId = ulong.Parse(context.Parameters[3]);
                                context.Message.ReplyAsync("Safe channel ID updated too " + context.Parameters[3]);
                                break;
                            }
                            case "unsafe":
                            {
                                Core.Airlock.Configuration.UnsafeChannelId = ulong.Parse(context.Parameters[3]);
                                context.Message.ReplyAsync("Unsafe channel ID updated too " + context.Parameters[3]);
                                break;
                            }                                
                        }
                        break;
                    }
                    case "show":
                        PrintAirlockConfiguration(context);
                        break;
                    case "":
                        context.Message.ReplyAsync("Printing help...");
                        break;
            }
        }
        public async Task PrintAirlockConfiguration(ParsedCommand context)
        {
            var builder = new EmbedBuilder()
                    .WithTitle("Saturn Bot: Airlock Configuration Information")
                    .WithUrl("https://github.com/emillly-b/SaturnBot")
                    .WithColor(new Color(0xEF4347))
                    .WithFooter(new EmbedFooterBuilder().WithText($"If you have questions or notice any errors, please contact {Core.DiscordClient.GetUser(Core.GetOwnerId()).ToString()}"))
                    .AddField("Airlock Enabled?:", Enabled);
                var embed = builder.Build();

                await context.Channel.SendMessageAsync("", embed: embed);
        }
        public async Task ChannelMirroringMessageRecieved()
        {
            //
        }
    }
}
