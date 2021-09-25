using Discord;
using Discord.API;
using SaturnBot.Entities;

using System.Collections.Generic;
using System.Threading.Tasks;


namespace SaturnBot.CommandModules
{
    class Commands : Module
    {

        public bool Enabled { get; set;}
        public List<ulong> MirroredChannels { get; set;}
        public List<Command> Load()
        {
            List<Command> commands = new List<Command>();

            Command MirrorChannel = new Command("mirrorchannel");
            MirrorChannel.Delete = false;
            MirrorChannel.Description = "Mirror all posts in a channel to another.";
            MirrorChannel.RequiredPermission = Command.PermissionLevels.Admin;
            MirrorChannel.Usage = "mirrorchannel channel\nmirrorchannel";
            MirrorChannel.ToExecute += async (context) =>
            {
                if(!Enabled)
                {
                    await context.Message.ReplyAsync("Channel Mirroring is been disabled.");
                    return;
                }
                ulong channelid = context.Channel.Id;
                if(context.Message.MentionedChannels.Count == 0)
                {
                    MirroredChannels.Add(channelid);
                    await context.Message.ReplyAsync("Current channel added to the mirroring list. ID: " + channelid);
                }
                if(context.Message.MentionedChannels.Count > 0)
                {
                    foreach(IGuildChannel c in context.Message.MentionedChannels)
                        MirroredChannels.Add(c.Id);
                    foreach(IGuildChannel c in context.Message.MentionedChannels)
                        await context.Message.ReplyAsync("Channel added to the mirroring list: " + c.Name + " ID: " + c.Id);
                }                
            };
            commands.Add(MirrorChannel);
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
