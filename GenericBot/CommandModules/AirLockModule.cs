using Discord;
using Discord.API;
using GenericBot.Entities;

using System.Collections.Generic;
using System.Threading.Tasks;


namespace GenericBot.CommandModules
{
    class AirLockmodule : Module
    {

        public bool Enabled { get; set;}
        public List<ulong> MirroredChannels { get; set;}
        public List<Command> Load()
        {
            List<Command> commands = new List<Command>();

            Command MirrorChannel = new Command("mirrorchannel");
            MirrorChannel.Delete = true;
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
            return commands;
        }
        public async Task ChannelMirroringMessageRecieved()
        {
            //
        }
    }
}
