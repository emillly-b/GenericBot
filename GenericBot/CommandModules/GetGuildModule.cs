using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using GenericBot.Entities;

namespace GenericBot.CommandModules
{
    class GetGuildModule : Module
    {
        public List<Command> Load()
        {
            List<Command> commands = new List<Command>();

            Command getInvite = new Command("getinvite");
            getInvite.RequiredPermission = Command.PermissionLevels.BotOwner;
            getInvite.Description = "Get an invite link to a guild if possible";
            getInvite.Usage = "getinvite <guildId>";
            getInvite.WorksInDms = true;
            getInvite.ToExecute += async (context) =>
            {
                ulong guildId;
                if(context.Parameters.Count < 1 || !ulong.TryParse(context.Parameters[0], out guildId))
                {
                    await context.Message.ReplyAsync("Please pass in a Guild Id");
                }
                guildId = ulong.Parse(context.Parameters[0]);
                try
                {
                    var guild = Core.DiscordClient.GetGuild(guildId);
                    var invite = guild.DefaultChannel.CreateInviteAsync(maxUses: 1).Result;

                    await context.Message.ReplyAsync($"Guild: {guild.Name} (`{guild.Id}`)\n" +
                        $"Owner: {guild.Owner} (`{guild.Owner.Id}`)\n" +
                        $"Invite: {invite.Url}");
                }
                catch
                {
                    await context.Message.ReplyAsync($"Could not get an invite for that server");
                }
            };
            commands.Add(getInvite);

            Command dmuser = new Command("dmuser");
            dmuser.RequiredPermission = Command.PermissionLevels.BotOwner;
            dmuser.Description = "Sent a DM to the specified user";
            dmuser.Usage = "dmuser <userId> [message]";
            dmuser.WorksInDms = true;
            dmuser.ToExecute += async (context) =>
            {
                var channel = Core.DiscordClient.GetUser(ulong.Parse(context.Parameters[0])).CreateDMChannelAsync().Result;
                if(context.Parameters.Count == 1)
                {
                    var messages = channel.GetMessagesAsync().Flatten().ToListAsync().Result;
                    string data = "";
                    messages.Reverse();
                    foreach(var msg in messages)
                    {
                        data += $"{msg.Author.Username}: {msg.Content}\n";
                        if (msg.Attachments.Any())
                            foreach (var a in msg.Attachments)
                                data += $"    Attachment: {a.ProxyUrl}\n";
                    }
                    File.WriteAllText("messages.txt", data);
                    await context.Channel.SendFileAsync("messages.txt");
                    File.Delete("messages.txt");
                    return;
                }

                string message = context.ParameterString.Substring(context.ParameterString.IndexOf(' ')).Trim();
                await channel.SendMessageAsync(message);
                await context.Message.ReplyAsync($"Sent `{message}` to {Core.DiscordClient.GetUser(ulong.Parse(context.Parameters[0]))}");
            };
            commands.Add(dmuser);

            Command getGuildCommand = new Command("getguild");
            getGuildCommand.Aliases = new List<string> { "getguilds" };
            getGuildCommand.Usage = "getguild <user|guild|name>";
            getGuildCommand.Description = "Gets information about guilds this bot is in.";
            getGuildCommand.RequiredPermission = Command.PermissionLevels.GlobalAdmin;
            getGuildCommand.ToExecute = async (context) =>
            {
                if (MentionUtils.TryParseUser(context.ParameterString, out var userId))
                {
                    if (Core.DiscordClient.GetUser(userId) is SocketUser user)
                        await PrintUserGuilds(context, user);
                    else await context.Message.ReplyAsync($"User with ID `{userId}` not found.");
                }
                else if (ulong.TryParse(context.ParameterString, out var id))
                {
                    if (Core.DiscordClient.GetGuild(id) is SocketGuild guild)
                        await PrintGuildInformation(context, guild);
                    else if (Core.DiscordClient.GetUser(id) is SocketUser user)
                        await PrintUserGuilds(context, user);
                    else await context.Message.ReplyAsync($"User or guild with ID `{userId}` not found.");
                }
                else await PerformGuildSearch(context, context.ParameterString);
            };
            commands.Add(getGuildCommand);

            return commands;
        }

        private async Task PerformGuildSearch(ParsedCommand context, string searchQuery)
        {
            var foundGuilds = Core.DiscordClient.Guilds
                .Where(g => g.Name.Contains(searchQuery, StringComparison.InvariantCultureIgnoreCase))
                .OrderByDescending(g => g.MemberCount).ToList();
            if (foundGuilds.Count == 0)
            {
                await context.Message.ReplyAsync($"Search query `{searchQuery}` did not return any resulting guilds.");
                return;
            }
            
            if (foundGuilds.Count > 25)
            {
                string res = "";
                foreach (var guild in foundGuilds)
                    res += $"{guild.Name} ({guild.Id})\n";

                foreach (var str in res.MessageSplit('\n'))
                    await context.Message.ReplyAsync(str);
                return;
            }
            
            var eb = new EmbedBuilder().WithTitle($"Guilds matching \"{searchQuery}\"");
            foreach (var foundGuild in foundGuilds)
                eb.AddField(foundGuild.Name, $"**ID:** {foundGuild.Id}\n**Members:** {foundGuild.MemberCount}\n**Owned by:** {foundGuild.Owner.Username}#{foundGuild.Owner.Discriminator} ({foundGuild.OwnerId})");

            await context.Channel.SendMessageAsync(embed: eb.Build());
        }

        private async Task PrintGuildInformation(ParsedCommand context, SocketGuild guild)
        {
            var eb = new EmbedBuilder()
                .WithTitle($"{guild.Name}")
                .AddField("ID", guild.Id)
                .AddField("Members", guild.MemberCount)
                .AddField("Created on", $"{guild.CreatedAt:yyyy-MM-dd HH\\:mm\\:ss zzzz} UTC")
                .AddField("Owner", $"{guild.Owner.Username}#{guild.Owner.Discriminator} ({guild.OwnerId})");
            await context.Channel.SendMessageAsync(embed: eb.Build());
        }

        private async Task PrintUserGuilds(ParsedCommand context, SocketUser user)
        {
            var eb = new EmbedBuilder().WithTitle($"Guilds owned by user {user.Username}#{user.Discriminator} ({user.Id})");
            var ownedGuilds = Core.DiscordClient.Guilds.Where(g => g.OwnerId == user.Id).Take(25).ToList();
            if (ownedGuilds.Count == 0)
            {
                await context.Message.ReplyAsync($"User {user.Username}#{user.Discriminator} ({user.Id}) does not own any guilds known by this bot.");
            }
            else
            {
                foreach (var ownedGuild in ownedGuilds)
                    eb.AddField(ownedGuild.Name, $"**ID:** {ownedGuild.Id}\n**Members:** {ownedGuild.MemberCount}");
                await context.Channel.SendMessageAsync(embed: eb.Build());
            }
        }
    }
}