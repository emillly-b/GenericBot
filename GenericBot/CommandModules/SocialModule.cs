using Discord;
using GenericBot.Entities;

using System.Collections.Generic;


namespace GenericBot.CommandModules
{
    class SocialModule : Module
    {
        public List<Command> Load()
        {
            List<Command> commands = new List<Command>();

            Command say = new Command("say");
            say.Delete = true;
            say.Aliases = new List<string> { "echo" };
            say.Description = "Say something a contributor said";
            say.SendTyping = false;
            say.RequiredPermission = Command.PermissionLevels.GlobalAdmin;
            say.Usage = "say <phrase>";
            say.ToExecute += async (context) =>
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
            commands.Add(say);

            return commands;
        }
    }
}
