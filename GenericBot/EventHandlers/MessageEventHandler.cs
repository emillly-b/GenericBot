using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using GenericBot.Entities;
using Newtonsoft.Json;

namespace GenericBot
{
    public static class MessageEventHandler
    {
        public static async Task MessageRecieved(SocketMessage parameterMessage, bool edited = false)
        {
            Core.Messages++;
            // Don't do stuff if the user is blacklisted
            if (Core.CheckBlacklisted(parameterMessage.Author.Id))
                return;
            // Ignore self
            if (parameterMessage.Author.Id == Core.GetCurrentUserId())
                return;

            // Handle me saying "open an issue"
            try
            {
                if(parameterMessage.Content.ToLower().Contains("open an issue") && parameterMessage.Author.Id == Core.DiscordClient.GetApplicationInfoAsync().Result.Owner.Id)
                {
                    parameterMessage.Channel.SendMessageAsync("https://github.com/galenguyer/GenericBot/issues");
                }
            }
            catch { }
            // pluralkit logging integration
            try 
            { 
                if (parameterMessage.Author.IsWebhook)
                {
                    using (var client = new System.Net.WebClient()) 
                    {
                        var resp = client.DownloadString($"https://api.pluralkit.me/v1/msg/{parameterMessage.Id}");
                        var type = new
                        {
                            original = "string"
                        };
                        var obj = JsonConvert.DeserializeAnonymousType(resp, type);
                        Program.ClearedMessageIds.Add(ulong.Parse(obj.original));
                    }
                }
            }
            catch { }
            // points 
            try
            {
                var dbUser = Core.GetUserFromGuild(parameterMessage.Author.Id, parameterMessage.GetGuild().Id);
                dbUser.IncrementPointsAndMessages();

                var dbGuild = Core.GetGuildConfig(parameterMessage.GetGuild().Id);
                if (dbGuild.TrustedRoleId != 0 && dbUser.Points > dbGuild.TrustedRolePointThreshold)
                {
                    var guild = Core.DiscordClient.GetGuild(dbGuild.Id);
                    var guildUser = guild.GetUser(dbUser.Id);
                    if (!guildUser.Roles.Any(sr => sr.Id == dbGuild.TrustedRoleId))
                    {
                        guildUser.AddRoleAsync(guild.GetRole(dbGuild.TrustedRoleId));
                    }
                }

                Core.SaveUserToGuild(dbUser, parameterMessage.GetGuild().Id);
            }
            catch (Exception e)
            {
                await Core.Logger.LogErrorMessage(e, null);
            }

            try
            {
                ParsedCommand command;

                if (parameterMessage.Channel is SocketDMChannel)
                {
                    command = new Command("t").ParseMessage(parameterMessage);

                    Core.Logger.LogGenericMessage($"Recieved DM: {parameterMessage.Content}");

                    if (command != null && command.RawCommand != null && command.RawCommand.WorksInDms)
                    {
                        command.Execute();
                    }
                    else
                    {
                        IUserMessage alertMessage = null;
                        if (Core.GlobalConfig.CriticalLoggingChannel != 0)
                            alertMessage = ((ITextChannel)Core.DiscordClient.GetChannel(Core.GlobalConfig.CriticalLoggingChannel))
                            .SendMessageAsync($"```\nDM from: {parameterMessage.Author}({parameterMessage.Author.Id})\nContent: {parameterMessage.Content}\n```").Result;
                        if (parameterMessage.Content.Trim().Split().Length == 1)
                        {
                            var guild = VerificationEngine.GetGuildFromCode(parameterMessage.Content, parameterMessage.Author.Id);
                            if (guild == null)
                            {
                                parameterMessage.ReplyAsync("Invalid verification code");
                            }
                            else
                            {
                                guild.GetUser(parameterMessage.Author.Id)
                                    .AddRoleAsync(guild.GetRole(Core.GetGuildConfig(guild.Id).VerifiedRole));
                                if (guild.TextChannels.HasElement(c => c.Id == (Core.GetGuildConfig(guild.Id).LoggingChannelId), out SocketTextChannel logChannel))
                                {
                                    logChannel.SendMessageAsync($"`{DateTime.UtcNow.ToString(@"yyyy-MM-dd HH:mm tt")}`:  `{parameterMessage.Author}` (`{parameterMessage.Author.Id}`) just verified");
                                }
                                parameterMessage.ReplyAsync($"You've been verified on **{guild.Name}**!");
                                if (alertMessage != null)
                                    alertMessage.ModifyAsync(m =>
                                        m.Content = $"```\nDM from: {parameterMessage.Author}({parameterMessage.Author.Id})\nContent: {parameterMessage.Content.SafeSubstring(1900)}\nVerified on {guild.Name}\n```");
                            }
                        }
                    }
                }
                else
                {
                    ulong guildId = parameterMessage.GetGuild().Id;
                    command = new Command("t").ParseMessage(parameterMessage);

                    if (Core.GetCustomCommands(guildId).HasElement(c => c.Name == command.Name,
                        out CustomCommand customCommand))
                    {
                        Core.AddToCommandLog(command, guildId);
                        if (customCommand.Delete)
                            parameterMessage.DeleteAsync();
                        parameterMessage.ReplyAsync(customCommand.Response);
                    }

                    if (command != null && command.RawCommand != null)
                    {
                        Core.AddToCommandLog(command, guildId);
                        command.Execute(); 
                    }
                }
            }
            catch (Exception ex)
            {
                if (parameterMessage.Author.Id == Core.GetOwnerId())
                {
                    parameterMessage.ReplyAsync("```\n" + $"{ex.Message}\n{ex.StackTrace}".SafeSubstring(1000) + "\n```");
                }
                Core.Logger.LogErrorMessage(ex, new Command("t").ParseMessage(parameterMessage));
            }
        }

        public static async Task MessageRecieved(SocketMessage arg)
        {
            MessageRecieved(arg, edited: false);
            UserEventHandler.UserUpdated(null, arg.Author);
        }

        public static async Task HandleEditedCommand(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3)
        {
            if (!arg1.HasValue || arg1.Value.Content == arg2.Content) return;

            if (Core.GlobalConfig.DefaultExecuteEdits)
            {
                MessageEventHandler.MessageRecieved(arg2, edited: true);
            }

            var guildConfig = Core.GetGuildConfig(arg2.GetGuild().Id);

            if (guildConfig.LoggingChannelId == 0 || guildConfig.MessageLoggingIgnoreChannels.Contains(arg2.Channel.Id)
                                                  || !arg1.HasValue) return;

            EmbedBuilder log = new EmbedBuilder()
                .WithTitle("Message Edited")
                .WithColor(243, 110, 33)
                .WithCurrentTimestamp();

            if (string.IsNullOrEmpty(arg2.Author.GetAvatarUrl()))
            {
                log = log.WithAuthor(new EmbedAuthorBuilder().WithName($"{arg2.Author} ({arg2.Author.Id})"));
            }
            else
            {
                log = log.WithAuthor(new EmbedAuthorBuilder().WithName($"{arg2.Author} ({arg2.Author.Id})")
                    .WithIconUrl(arg2.Author.GetAvatarUrl() + " "));
            }

            log.AddField(new EmbedFieldBuilder().WithName("Channel").WithValue("#" + arg2.Channel.Name).WithIsInline(true));
            log.AddField(new EmbedFieldBuilder().WithName("Sent At").WithValue(arg1.Value.Timestamp.ToString(@"yyyy-MM-dd HH:mm.ss") + "GMT").WithIsInline(true));

            log.AddField(new EmbedFieldBuilder().WithName("Before").WithValue(arg1.Value.Content.SafeSubstring(1016)));
            log.AddField(new EmbedFieldBuilder().WithName("After").WithValue(arg2.Content.SafeSubstring(1016)));

            await arg2.GetGuild().GetTextChannel(guildConfig.LoggingChannelId).SendMessageAsync("", embed: log.Build());
        }

        public static async Task MessageDeleted(Cacheable<IMessage, ulong> arg, ISocketMessageChannel channel)
        {
            if (!arg.HasValue) return;
            if (Program.ClearedMessageIds.Contains(arg.Id)) return;
            var guildConfig = Core.GetGuildConfig((arg.Value as SocketMessage).GetGuild().Id);

            if (guildConfig.LoggingChannelId == 0 || guildConfig.MessageLoggingIgnoreChannels.Contains(channel.Id)) return;

            EmbedBuilder log = new EmbedBuilder()
                .WithTitle("Message Deleted")
                .WithColor(139, 0, 0)
                .WithCurrentTimestamp();

            if (string.IsNullOrEmpty(arg.Value.Author.GetAvatarUrl()))
            {
                log = log.WithAuthor(new EmbedAuthorBuilder().WithName($"{arg.Value.Author} ({arg.Value.Author.Id})"));
            }
            else
            {
                log = log.WithAuthor(new EmbedAuthorBuilder().WithName($"{arg.Value.Author} ({arg.Value.Author.Id})")
                    .WithIconUrl(arg.Value.Author.GetAvatarUrl() + " "));
            }

            log.AddField(new EmbedFieldBuilder().WithName("Channel").WithValue("#" + arg.Value.Channel.Name).WithIsInline(true));
            log.AddField(new EmbedFieldBuilder().WithName("Sent At").WithValue(arg.Value.Timestamp.ToString(@"yyyy-MM-dd HH:mm.ss") + "GMT").WithIsInline(true));


            if (!string.IsNullOrEmpty(arg.Value.Content))
            {
                log.WithDescription("**Message:** " + arg.Value.Content);
            }

            if (arg.Value.Attachments.Any())
            {
                log.AddField(new EmbedFieldBuilder().WithName("Attachments").WithValue(arg.Value.Attachments.Select(a =>
                    $"File: {a.Filename}").Aggregate((a, b) => a + "\n" + b)));
                log.WithImageUrl(arg.Value.Attachments.First().ProxyUrl);
            }

            if (string.IsNullOrEmpty(arg.Value.Content) && !arg.Value.Attachments.Any() && arg.Value.Embeds.Any())
            {
                log.WithDescription("**Embed:**\n```json\n" + JsonConvert.SerializeObject(arg.Value.Embeds.First(), Formatting.Indented) + "\n```");
            }

            log.Footer = new EmbedFooterBuilder().WithText(arg.Value.Id.ToString());

            await (arg.Value as SocketMessage).GetGuild().GetTextChannel(guildConfig.LoggingChannelId).SendMessageAsync("", embed: log.Build());
        }
    }
}