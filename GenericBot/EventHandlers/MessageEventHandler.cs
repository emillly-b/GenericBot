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
            //Increment message counter no matter the condition. 
            Core.Messages++;
            // Check if the message was sent from ourselves, if so disregard the message.
            if (parameterMessage.Author.Id == Core.GetCurrentUserId())
                return;

            #region PluralKit Integration
            try 
            { 
                // Pluralkit uses webhooks to proxy messages from the main system account, This might get triggered with a logging webhook
                // TODO: Check webhook log, Eventually.
                if (parameterMessage.Author.IsWebhook)
                {
                    /*
                    TODO: FIX PK INTEGRATION
                    using (var client = new System.Net.WebClient()) 
                    {
                        var resp = client.DownloadString($"https://api.pluralkit.me/v1/msg/{parameterMessage.Id}");
                        var type = new {original = "string"};
                        var obj = JsonConvert.DeserializeAnonymousType(resp, type);
                        Program.ClearedMessageIds.Add(ulong.Parse(obj.original));
                    }

                    */
                }
            }   catch { }
            #endregion
            #region Points
            try
            {
                var dbUser = Core.GetUserFromGuild(parameterMessage.Author.Id, parameterMessage.GetGuild().Id);

                //Increment user points
                dbUser.IncrementPointsAndMessages();

                //Not a fucking clue
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
            #endregion
            
            try
            {
                ParsedCommand command;
                #region DM Handling
                if (parameterMessage.Channel is SocketDMChannel)
                {
                    command = new Command("t").ParseMessage(parameterMessage);

                    Core.Logger.LogGenericMessage($"Recieved DM: {parameterMessage.Content}");

                    //Check if registered command and executes, if not logs DM to the webhook channel (if configured)
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
                        
                    }
                }
                #endregion
                #region Normal Channel Command Handling
                else
                {
                    //Looks like command handling?
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
                #endregion 
            }
            catch (Exception ex)
            {
                //Log all errors.
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

        }

        public static async Task MessageDeleted(Cacheable<IMessage, ulong> arg, ISocketMessageChannel channel)
        {
            
        }
    }
}