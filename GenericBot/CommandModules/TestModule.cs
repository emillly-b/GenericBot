using Discord;
using GenericBot.Entities;

using System.Collections.Generic;


namespace GenericBot.CommandModules
{
    class TestModule : Module
    {
        public List<Command> Load()
        {
            List<Command> commands = new List<Command>();

            Command le = new Command("listemojiis");
            le.Delete = true;
            le.Description = "List specified guild's emojii's";
            le.SendTyping = false;
            //TODO: Decide on this perm.
            le.RequiredPermission = Command.PermissionLevels.BotOwner;
            le.Usage = "listemojiis guildid";
            le.ToExecute += async (context) =>
            {
                ulong channelid = context.Channel.Id;
                if (ulong.TryParse(context.Parameters[0], out channelid))
                {
                    await ((ITextChannel)Core.DiscordClient.GetChannel(channelid)).SendMessageAsync(context.ParameterString.Substring(context.ParameterString.IndexOf(' ')));
                    return;
                }
                else
                {
                    await context.Message.ReplyAsync(context.ParameterString.Substring(context.ParameterString.IndexOf(' ')));
                }
            };
            commands.Add(le);

            return commands;
        }
    }
}
