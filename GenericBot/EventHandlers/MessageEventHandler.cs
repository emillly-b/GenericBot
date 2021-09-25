using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using SaturnBot.Entities;
using Newtonsoft.Json;

namespace SaturnBot
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
            // Ignore Webhooks
            if (parameterMessage.Author.IsWebhook)
                return;
            
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
        }

        public static async Task HandleEditedCommand(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3)
        {

        }

        public static async Task MessageDeleted(Cacheable<IMessage, ulong> arg, ISocketMessageChannel channel)
        {
            
        }
    }
}