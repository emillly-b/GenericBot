using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace GenericBot.Entities
{
    /// <summary>
    /// Raw Command object to build
    /// </summary>
    public class Command
    {
        public enum PermissionLevels
        {
            User,
            Moderator,
            Admin,
            GuildOwner,
            GlobalAdmin,
            BotOwner
        }

        public string Name;
        public List<string> Aliases = new List<string>();
        public string Description = "Not Available";
        public string Usage;
        public bool Delete = false;
        public bool SendTyping = true;
        public bool WorksInDms = false;
        public PermissionLevels RequiredPermission = PermissionLevels.User;

        public Command(string n)
        {
            this.Name = n.ToLower();
            this.Usage = this.Name;
        }

        public delegate Task ExecuteDelegate(ParsedCommand command);

        public ExecuteDelegate ToExecute = null;

        public async Task ExecuteCommand(ParsedCommand command)
        {
            if(Core.GlobalConfig.OwnerOnly)
            {
                if(command.Author.Id != Core.GetOwnerId())
                return;
            }

            if (GetPermissions(command) < RequiredPermission)
            {
                await command.Message.ReplyAsync($"I'm sorry, {command.Author.GetDisplayName()}. I'm afraid I can't do that.");
                return;
            }

            if (this.RequiredPermission >= PermissionLevels.Moderator && this.RequiredPermission < PermissionLevels.GlobalAdmin && this.Name != "audit")
                Core.AddToAuditLog(command, command.Guild.Id);

            if (SendTyping) await command.Message.Channel.TriggerTypingAsync();
            if (Delete)
            {
                try
                {
                    await command.Message.DeleteAsync();
                }
                catch (Discord.Net.HttpException ex)
                { 
                    await Core.Logger.LogErrorMessage(ex, command);
                }
            }

            try
            {
                await ToExecute(command);
            }
            catch (Exception ex)
            {
                if (command.Message.Author.Id == Core.GetOwnerId() && Core.GlobalConfig.PrintStackTrace)
                {
                    await (command.Message as SocketMessage).ReplyAsync("```\n" + $"{ex.Message}\n{ex.StackTrace}".SafeSubstring(1600) +
                                                      "\n```");
                }
                await Core.Logger.LogErrorMessage(ex, command);
            }
        }

        public PermissionLevels GetPermissions(ParsedCommand context)
        {
            try {
                if (context.Channel is SocketDMChannel)
                    return PermissionLevels.User;
                else if (context.Author.Id.Equals(Core.GetOwnerId()))
                    return PermissionLevels.BotOwner;
                else if (Core.CheckGlobalAdmin(context.Author.Id))
                    return PermissionLevels.GlobalAdmin;
                else if(IsGuildAdmin(context.Author, context.Guild.Id))
                    return PermissionLevels.GuildOwner;
                else if (((SocketGuildUser)context.Author).Roles.Select(r => r.Id).Intersect(Core.GetGuildConfig(context.Guild.Id).AdminRoleIds).Any())
                    return PermissionLevels.Admin;
                else if (((SocketGuildUser)context.Author).Roles.Select(r => r.Id).Intersect(Core.GetGuildConfig(context.Guild.Id).ModRoleIds).Any())
                    return PermissionLevels.Moderator;
                else
	            return PermissionLevels.User;
            }
            catch (Exception exep) {
	        Console.WriteLine(exep);
                return PermissionLevels.User;
            }
        }
        private bool IsGuildAdmin(SocketUser user, ulong guildId)
        {
            try
            {
                var guild = Core.GetGuid(guildId);
                if (guild.Owner.Id == user.Id)
                    return true;
                else if (guild.GetUser(user.Id).Roles.Any(r => r.Permissions.Administrator))
                    return true;
                return false;
            }
            catch
            {
                return false;
            }
        }

        public ParsedCommand ParseMessage(SocketMessage msg)
        {
            ParsedCommand parsedCommand = new ParsedCommand();
            parsedCommand.Message = msg;
            string message = msg.Content;
            string pref = Core.GetPrefix(parsedCommand);

            if (!message.StartsWith(pref)) return null;
            message = message.Substring(pref.Length);
            string commandId = message.Split(' ')[0].ToLower();
            parsedCommand.Name = commandId;

            if (Core.Commands.HasElement(c => commandId.Equals(c.Name) || c.Aliases.Any(a => commandId.Equals(a)), out Command cmd))
            {
                parsedCommand.RawCommand = cmd;
            }
            else
            {
                parsedCommand.RawCommand = null;
            }

            try
            {
                string param = message.Substring(commandId.Length);
                parsedCommand.ParameterString = param.Trim();
                parsedCommand.Parameters = param.Split().Where(p => !string.IsNullOrEmpty(p.Trim())).ToList();
            }
            catch (Exception ex)
            {
                Core.Logger.LogErrorMessage(ex, parsedCommand);
            }

            return parsedCommand;
        }
    }
}
