using Discord;
using GenericBot.Database;
using GenericBot.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GenericBot.CommandModules
{
    class ConfigModule : Module
    {

        public List<Command> Load()
        {
            List<Command> commands = new List<Command>();

            Command setstatus = new Command("setstatus");
            setstatus.RequiredPermission = Command.PermissionLevels.BotOwner;
            setstatus.ToExecute += async (context) =>
            {
                await Core.DiscordClient.SetGameAsync(context.Parameters[0].ToLower(),type: ActivityType.CustomStatus);
                await context.Message.ReplyAsync("Status set as: " + context.Parameters[0].ToLower());
            };
            commands.Add(setstatus);


            Command config = new Command("config");
            config.RequiredPermission = Command.PermissionLevels.Admin;
            config.Description = "Configure the bot or see current config";
            config.Usage = "config <option> <value>";
            config.ToExecute += async (context) =>
            {
                var _guildConfig = Core.GetGuildConfig(context.Guild.Id);
                if (context.Parameters.IsEmpty())
                {
                    string currentConfig =
                    $"Prefix: `{Core.GetPrefix(context)}`\n" +
                    $"Admin Roles: `{JsonConvert.SerializeObject(_guildConfig.AdminRoleIds)}`\n" +
                    $"Mod Roles: `{JsonConvert.SerializeObject(_guildConfig.ModRoleIds)}`\n" +
                    $"User Roles: `{JsonConvert.SerializeObject(_guildConfig.UserRoles)}`\n" +
                    $"Logging\n" +
                    $"    Channel Id: `{_guildConfig.LoggingChannelId}`\n" +
                    $"    Ignored Channels: `{JsonConvert.SerializeObject(_guildConfig.MessageLoggingIgnoreChannels)}`\n" +
                    $"Muted Role Id: `{_guildConfig.MutedRoleId}`\n" +
                    $"Verification:\n" +
                    $"    Role Id: `{_guildConfig.VerifiedRole}`\n" +
                    $"    Message: Do `{Core.GetPrefix(context)}config verification message` to see the message\n" +
                    $"Points: {_guildConfig.PointsEnabled}\n" +
                    $"Auto Roles: `{JsonConvert.SerializeObject(_guildConfig.AutoRoleIds)}`" +
                    $"Trusted Role Id: `{_guildConfig.TrustedRoleId}`" +
                    $"Trusted Role Point Threshold: `{_guildConfig.TrustedRolePointThreshold}`";

                    await context.Message.ReplyAsync(currentConfig);
                }

                #region AdminRoles
                else if (context.Parameters[0].ToLower().Equals("adminroles"))
                {
                    if (context.Parameters.Count == 1)
                    {
                        await context.Message.ReplyAsync($"Please enter a config option");
                        return;
                    }
                    if (context.Parameters.Count == 2)
                    {
                        await context.Message.ReplyAsync($"Please enter a roleId");
                        return;
                    }
                    ulong id;
                    if (ulong.TryParse(context.Parameters[2], out id) && context.Guild.Roles.Select(r => r.Id).Any(u => u.Equals(id)))
                    {
                        if (context.Parameters[1].ToLower().Equals("add"))
                        {
                            if (!Core.GetGuildConfig(context.Guild.Id).AdminRoleIds.Contains(id))
                            {
                                Core.GetGuildConfig(context.Guild.Id).AdminRoleIds.Add(id);
                                await context.Message.ReplyAsync($"Added {context.Guild.Roles.FirstOrDefault(r => r.Id == id).Name} to Admin Roles");
                            }
                            else { await context.Message.ReplyAsync($"Admin Roles already contains {context.Guild.Roles.FirstOrDefault(r => r.Id == id).Name}"); }
                        }
                        else if (context.Parameters[1].ToLower().Equals("remove"))
                        {
                            if (Core.GetGuildConfig(context.Guild.Id).AdminRoleIds.Contains(id))
                            {
                                Core.GetGuildConfig(context.Guild.Id).AdminRoleIds.Remove(id);
                                await context.Message.ReplyAsync($"Removed {context.Guild.Roles.FirstOrDefault(r => r.Id == id).Name} from Admin Roles");
                            }
                            else
                            {
                                await context.Message.ReplyAsync(
                                    $"Admin Roles doesn't contain {context.Guild.Roles.FirstOrDefault(r => r.Id == id).Name}");
                            }
                        }
                        else { await context.Message.ReplyAsync($"Unknown property `{context.Parameters[1]}`."); }
                    }
                    else { await context.Message.ReplyAsync($"That is not a valid roleId"); }
                }
                #endregion AdminRoles

                #region ModRoles
                else if (context.Parameters[0].ToLower().Equals("moderatorroles") || context.Parameters[0].ToLower().Equals("modroles"))
                {
                    if (context.Parameters.Count == 1)
                    {
                        await context.Message.ReplyAsync($"Please enter a config option");
                        return;
                    }
                    if (context.Parameters.Count == 2)
                    {
                        await context.Message.ReplyAsync($"Please enter a roleId");
                        return;
                    }
                    ulong id;
                    if (ulong.TryParse(context.Parameters[2], out id) && context.Guild.Roles.Select(r => r.Id).Any(u => u.Equals(id)))
                    {
                        if (context.Parameters[1].ToLower().Equals("add"))
                        {
                            if (!_guildConfig.ModRoleIds.Contains(id))
                            {
                                _guildConfig.ModRoleIds.Add(id);
                                await context.Message.ReplyAsync($"Added {context.Guild.Roles.FirstOrDefault(r => r.Id == id).Name} to Moderator Roles");
                            }
                            else
                            {
                                await context.Message.ReplyAsync($"Moderator Roles already contains {context.Guild.Roles.FirstOrDefault(r => r.Id == id).Name}");
                            }
                        }
                        else if (context.Parameters[1].ToLower().Equals("remove"))
                        {
                            if (_guildConfig.ModRoleIds.Contains(id))
                            {
                                {
                                    _guildConfig.ModRoleIds.Remove(id);
                                    await context.Message.ReplyAsync(
                                        $"Removed {context.Guild.Roles.FirstOrDefault(r => r.Id == id).Name} from Moderator Roles");
                                }
                            }
                            else
                            {
                                await context.Message.ReplyAsync(
                                    $"Moderator Roles doesn't contain {context.Guild.Roles.FirstOrDefault(r => r.Id == id).Name}");
                            }
                        }
                        else { await context.Message.ReplyAsync($"Unknown property `{context.Parameters[1]}`."); }
                    }
                    else { await context.Message.ReplyAsync($"That is not a valid roleId"); }
                }
                #endregion ModRoles

                #region UserRoles

                else if (context.Parameters[0].ToLower().Equals("userroles"))
                {
                    if (config.GetPermissions(context) >= Command.PermissionLevels.Admin)
                    {
                        if (context.Parameters.Count == 1)
                        {
                            await context.Message.ReplyAsync($"Please enter a config option");
                            return;
                        }
                        if (context.Parameters.Count == 2)
                        {
                            await context.Message.ReplyAsync($"Please enter a roleId");
                            return;
                        }
                        ulong id;
                        if (ulong.TryParse(context.Parameters[2], out id) && context.Guild.Roles.Select(r => r.Id).Any(u => u.Equals(id)))
                        {
                            if (context.Parameters[1].ToLower().Equals("add"))
                            {
                                if (!_guildConfig.UserRoles.Any(rg => rg.Value.Contains(id)))
                                {
                                    context.Parameters.RemoveRange(0, 3);
                                    if (context.Parameters.Count != 0)
                                    {
                                        if (_guildConfig.UserRoles.Any(rg => rg.Key.ToLower() == context.Parameters.Rejoin().ToLower()))
                                        {
                                            string groupName = _guildConfig.UserRoles.First(rg => rg.Key.ToLower() == context.Parameters.Rejoin().ToLower()).Key;
                                            _guildConfig.UserRoles[groupName].Add(id);
                                            await context.Message.ReplyAsync($"Added {context.Guild.Roles.FirstOrDefault(r => r.Id == id).Name} to User Roles in group {_guildConfig.UserRoles.First(rg => rg.Key.ToLower() == context.Parameters.Rejoin().ToLower()).Key}");
                                        }
                                        else
                                        {
                                            _guildConfig.UserRoles.Add(context.Parameters.Rejoin(), new List<ulong>());
                                            _guildConfig.UserRoles[context.Parameters.Rejoin()].Add(id);
                                            await context.Message.ReplyAsync($"Added {context.Guild.Roles.FirstOrDefault(r => r.Id == id).Name} to User Roles in new group {_guildConfig.UserRoles.First(rg => rg.Key.ToLower() == context.Parameters.Rejoin().ToLower()).Key}");
                                        }
                                    }
                                    else
                                    {
                                        try
                                        {
                                            _guildConfig.UserRoles.Add("", new List<ulong>());
                                        }
                                        catch (ArgumentException ex)
                                        {
                                            await Core.Logger.LogErrorMessage(ex, context);
                                        }

                                        _guildConfig.UserRoles[""].Add(id);
                                        await context.Message.ReplyAsync($"Added {context.Guild.Roles.FirstOrDefault(r => r.Id == id).Name} to User Roles");
                                    }
                                }
                                else
                                {
                                    await context.Message.ReplyAsync($"User Roles already contains {context.Guild.Roles.FirstOrDefault(r => r.Id == id).Name}");
                                }
                            }
                            else if (context.Parameters[1].ToLower().Equals("remove"))
                            {
                                if (_guildConfig.UserRoles.Any(rg => rg.Value.Contains(id)))
                                {
                                    {
                                        _guildConfig.UserRoles.First(rg => rg.Value.Contains(id)).Value.Remove(id);
                                        await context.Message.ReplyAsync(
                                            $"Removed {context.Guild.Roles.FirstOrDefault(r => r.Id == id).Name} from User Roles");
                                    }
                                }
                                else { await context.Message.ReplyAsync($"User Roles doesn't contain {context.Guild.Roles.FirstOrDefault(r => r.Id == id).Name}"); }
                            }
                            else { await context.Message.ReplyAsync($"Unknown property `{context.Parameters[1]}`."); }
                        }
                        else { await context.Message.ReplyAsync($"That is not a valid roleId"); }
                    }
                    else { await context.Message.ReplyAsync($"You don't have the permissions to do that"); }
                }

                #endregion UserRoles

                #region RequiresRoles

                else if (context.Parameters[0].ToLower().Equals("requiresroles"))
                {
                    if (context.Parameters.Count == 1)
                    {
                        await context.Message.ReplyAsync($"Please enter a config option");
                        return;
                    }
                    var checkRegex = new Regex(@"requiresroles (add|remove) \d{17,19} requires \d{17,19}", RegexOptions.IgnoreCase);
                    if (!checkRegex.IsMatch(context.ParameterString.ToLower().Replace("  ", " ")))
                    {
                        await context.Message.ReplyAsync($"Syntax incorrect. Please use the syntax {_guildConfig.Prefix}config requiresRoles <add|remove> [roleid] requires [requiredRoleId]");
                        return;
                    }
                    if (ulong.TryParse(context.Parameters[2], out ulong roleId) && context.Guild.Roles.Select(r => r.Id).Any(u => u.Equals(roleId))
                    && ulong.TryParse(context.Parameters[4], out ulong requiredRoleId) && context.Guild.Roles.Select(r => r.Id).Any(u => u.Equals(requiredRoleId)))
                    {
                        if (_guildConfig.RequiresRoles == null)
                            _guildConfig.RequiresRoles = new Dictionary<ulong, List<ulong>>();
                        if (context.Parameters[1].ToLower().Equals("add"))
                        {
                            if (_guildConfig.RequiresRoles.ContainsKey(roleId))
                            {
                                if (_guildConfig.RequiresRoles[roleId] == null || !_guildConfig.RequiresRoles[roleId].Contains(requiredRoleId))
                                {
                                    if (_guildConfig.RequiresRoles[roleId] == null)
                                    {
                                        _guildConfig.RequiresRoles[roleId] = new List<ulong>();
                                    }
                                    _guildConfig.RequiresRoles[roleId].Add(requiredRoleId);
                                    await context.Message.ReplyAsync($"{context.Guild.Roles.First(r => r.Id == roleId).Name} now requires {context.Guild.Roles.First(r => r.Id == requiredRoleId).Name}");

                                }
                                else
                                {
                                    await context.Message.ReplyAsync($"{context.Guild.Roles.First(r => r.Id == roleId).Name} already requires {context.Guild.Roles.First(r => r.Id == requiredRoleId).Name}");
                                }
                            }
                            else
                            {
                                _guildConfig.RequiresRoles[roleId] = new List<ulong> { requiredRoleId };
                                await context.Message.ReplyAsync($"{context.Guild.Roles.First(r => r.Id == roleId).Name} now requires {context.Guild.Roles.First(r => r.Id == requiredRoleId).Name}");
                            }
                        }
                        else if (context.Parameters[1].ToLower().Equals("remove"))
                        {
                            if (_guildConfig.RequiresRoles.ContainsKey(roleId))
                            {
                                if (_guildConfig.RequiresRoles[roleId] == null || !_guildConfig.RequiresRoles[roleId].Contains(requiredRoleId))
                                {
                                    await context.Message.ReplyAsync($"That requirement does not exist");
                                }
                                else
                                {
                                    _guildConfig.RequiresRoles[roleId].RemoveAll(r => r == requiredRoleId);
                                    if (!_guildConfig.RequiresRoles[roleId].Any())
                                    {
                                        _guildConfig.RequiresRoles.Remove(roleId);
                                    }
                                    await context.Message.ReplyAsync($"{context.Guild.Roles.First(r => r.Id == roleId).Name} no longer requires {context.Guild.Roles.First(r => r.Id == requiredRoleId).Name}");
                                }
                            }
                            else
                            {
                                await context.Message.ReplyAsync($"That requirement does not exist");
                            }
                        }

                    }
                    else { await context.Message.ReplyAsync($"One of those is not a valid roleId"); }
                }

                #endregion RequiresRoles

                #region Prefix

                else if (context.Parameters[0].ToLower().Equals("prefix"))
                {
                    try
                    {
                        context.Parameters.RemoveAt(0);
                        _guildConfig.Prefix = context.Parameters.Rejoin();
                        if (context.Message.Content.EndsWith('"') && context.Parameters[0].ToCharArray()[0].Equals('"'))
                        {
                            _guildConfig.Prefix = new Regex("\"(.*?)\"").Match(context.Message.Content).Value.Trim('"');
                        }
                        await context.Message.ReplyAsync($"The prefix has been set to `{_guildConfig.Prefix}`");
                    }
                    catch (Exception ex)
                    {
                        await Core.Logger.LogErrorMessage(ex, context);
                        _guildConfig.Prefix = "";
                        await context.Message.ReplyAsync($"The prefix has been reset to the default of `{Core.GetGlobalPrefix()}`");
                    }
                }

                #endregion Prefix

                #region Logging

                else if (context.Parameters[0].ToLower().Equals("logging"))
                {
                    if (context.Parameters[1].ToLower().Equals("channelid"))
                    {
                        if (context.Parameters.Count() == 2)
                        {
                            await context.Message.ReplyAsync(
                                $"Current user event channel: <#{_guildConfig.LoggingChannelId}>");
                        }
                        else
                        {
                            ulong cId;
                            if (ulong.TryParse(context.Parameters[2], out cId) && (context.Guild.Channels.Any(c => c.Id == cId) || cId == 0))
                            {
                                _guildConfig.LoggingChannelId = cId;
                                await context.Message.ReplyAsync(
                                    $"User event channel set to <#{_guildConfig.LoggingChannelId}>");
                            }
                            else await context.Message.ReplyAsync("Invalid Channel Id");
                        }
                    }
                    else if (context.Parameters[1].ToLower().Equals("ignorechannel"))
                    {
                        if (context.Parameters.Count == 2)
                        {
                            string m = "Ignored channels:";
                            foreach (var id in _guildConfig.MessageLoggingIgnoreChannels)
                            {
                                m += $"\n<#{id}>";
                            }

                            await context.Message.ReplyAsync(m);
                        }
                        else
                        {
                            if (ulong.TryParse(context.Parameters[2], out ulong cId) &&
                                context.Guild.TextChannels.Any(c => c.Id == cId))
                            {
                                if (!_guildConfig.MessageLoggingIgnoreChannels.Contains(cId))
                                {
                                    _guildConfig.MessageLoggingIgnoreChannels.Add(cId);
                                    await context.Message.ReplyAsync($"No longer logging <#{cId}>");
                                }
                                else
                                {
                                    _guildConfig.MessageLoggingIgnoreChannels.Remove(cId);
                                    await context.Message.ReplyAsync($"Resuming logging <#{cId}>");
                                }
                            }
                            else
                            {
                                await context.Message.ReplyAsync("Invalid Channel Id");
                            }
                        }
                    }
                }
                #endregion Logging

                #region MutedRoleId

                else if (context.Parameters[0].ToLower().Equals("mutedroleid"))
                {
                    context.Parameters.RemoveAt(0);
                    if (context.Parameters.Count != 1)
                    {
                        await context.Message.ReplyAsync(
                            "Incorrect number of arguments. Make sure the command is `voicerole [VoiceChannelId] [RoleId]`");
                        return;
                    }
                    else
                    {
                        ulong roleId;
                        if (ulong.TryParse(context.Parameters[0], out roleId) && (context.Guild.Roles.Any(r => r.Id == roleId) || roleId == 0))
                        {
                            _guildConfig.MutedRoleId = roleId;
                            Core.SaveGuildConfig(_guildConfig);
                            await context.Message.ReplyAsync($"MutedRoleId is now `{roleId}`");
                        }
                        else
                        {
                            await context.Message.ReplyAsync("Invalid Role Id");
                        }
                    }
                }

                #endregion MutedRoleId

                #region Verification

                else if (context.Parameters[0].ToLower().Equals("verification"))
                {
                    if (context.Parameters[1].ToLower().Equals("roleid"))
                    {
                        if (context.Parameters.Count == 2)
                        {
                            var roleId = _guildConfig.VerifiedRole;
                            if (roleId == 0)
                            {
                                await context.Message.ReplyAsync("Verification is disabled on this server");
                            }
                            else
                            {
                                await context.Message.ReplyAsync(
                                    $"Verification role is  `{context.Guild.Roles.First(g => g.Id == roleId).Name}`");
                            }
                        }
                        else if (ulong.TryParse(context.Parameters[2], out ulong roleId) && (context.Guild.Roles.Any(g => g.Id == roleId) || roleId == 0))
                        {
                            _guildConfig.VerifiedRole = roleId;
                            if (roleId != 0)
                            {
                                await context.Message.ReplyAsync(
                                    $"Verification role set to `{context.Guild.Roles.First(g => g.Id == roleId).Name}`");
                            }
                            else
                            {
                                await context.Message.ReplyAsync("Verification role cleared. Verification is off for this server.");
                            }
                        }
                        else
                        {
                            await context.Message.ReplyAsync("Invalid RoleId");
                        }
                    }
                    else if (context.Parameters[1].ToLower().Equals("message"))
                    {
                        string pref = Core.GetPrefix(context);

                        string message = context.ParameterString.Replace("  ", " ").Remove(0, "verification message".Length);

                        if (string.IsNullOrEmpty(message) && string.IsNullOrEmpty(_guildConfig.VerifiedMessage))
                        {
                            await context.Message.ReplyAsync("Verification is disabled on this server. Please make sure you have a roleid and message set.");
                            return;
                        }
                        else if (!string.IsNullOrEmpty(message))
                        {
                            _guildConfig.VerifiedMessage = message;
                        }
                        await context.Message.ReplyAsync("Example verification message:");

                        string vm = $"Hey {context.Author.Username}! To get verified on **{context.Guild.Name}** reply to this message with the hidden code in the message below\n\n"
                                            + _guildConfig.VerifiedMessage;

                        string verificationMessage =
                            VerificationEngine.InsertCodeInMessage(vm, VerificationEngine.GetVerificationCode(context.Author.Id, context.Guild.Id));

                        await context.Message.ReplyAsync(verificationMessage);
                    }
                    else await context.Message.ReplyAsync("Invalid Option");
                }

                #endregion Verification

                #region JoinMessage

                else if (context.Parameters[0].ToLower().Equals("joinmessage"))
                {
                    if (context.Parameters.Count >= 2 && context.Parameters[1].ToLower().Equals("channelid"))
                    {
                        if (context.Parameters.Count == 2)
                        {
                            var channelId = _guildConfig.JoinMessageChannelId;
                            if (channelId == 0)
                            {
                                await context.Message.ReplyAsync("Join messages are disabled on this server");
                            }
                            else
                            {
                                await context.Message.ReplyAsync($"Join message channel is <#{channelId}>");
                            }
                        }
                        else if (ulong.TryParse(context.Parameters[2], out ulong channelId) && (context.Guild.Channels.Any(g => g.Id == channelId) || channelId == 0))
                        {
                            _guildConfig.JoinMessageChannelId = channelId;
                            if (channelId != 0)
                            {
                                await context.Message.ReplyAsync(
                                    $"Join message channel set to <#{channelId}>");
                            }
                            else
                            {
                                await context.Message.ReplyAsync("Join message channel cleared. Join messages are off for this server.");
                            }
                        }
                        else
                        {
                            await context.Message.ReplyAsync("Invalid Channel Id");
                        }
                    }
                    else if (context.Parameters.Count >= 2 && context.Parameters[1].ToLower().Equals("message"))
                    {
                        string message = context.ParameterString.Replace("  ", " ").Remove(0, "joinmessage message".Length);

                        if (string.IsNullOrEmpty(message) && string.IsNullOrEmpty(_guildConfig.VerifiedMessage))
                        {
                            await context.Message.ReplyAsync("Join messages are disabled on this server. Please make sure you have a channelid and message set.");
                            return;
                        }
                        else if (!string.IsNullOrEmpty(message))
                        {
                            _guildConfig.JoinMessage = message;
                        }

                        await context.Message.ReplyAsync("Example join message:");

                        await context.Message.ReplyAsync(VerificationEngine.ConstructWelcomeMessage(_guildConfig.JoinMessage, context.Author));
                    }
                    else await context.Message.ReplyAsync("Invalid Option");
                }

                #endregion JoinMessage

                #region AutoRole

                else if (context.Parameters[0].ToLower().Equals("autoroles") || context.Parameters[0].ToLower().Equals("autorole"))
                {
                    if (context.Parameters.Count == 2)
                    {
                        await context.Message.ReplyAsync($"Please enter a roleId");
                        return;
                    }
                    if (ulong.TryParse(context.Parameters[2], out ulong id))
                    {
                        if (context.Parameters[1].ToLower().Equals("add"))
                        {
                            if (context.Guild.Roles.Select(r => r.Id).Any(u => u.Equals(id)))
                            {
                                if (!_guildConfig.AutoRoleIds.Contains(id))
                                {
                                    _guildConfig.AutoRoleIds.Add(id);
                                    await context.Message.ReplyAsync($"Added {context.Guild.Roles.FirstOrDefault(r => r.Id == id).Name} to autoroles");
                                }
                                else
                                {
                                    await context.Message.ReplyAsync($"Autoroles already contains {context.Guild.Roles.FirstOrDefault(r => r.Id == id).Name}");
                                }
                            }
                            else
                            {
                                await context.Message.ReplyAsync("Invalid RoleId (Not a role)");
                            }
                        }
                        else if (context.Parameters[1].ToLower().Equals("remove"))
                        {
                            if (_guildConfig.AutoRoleIds.Contains(id))
                            {
                                {
                                    _guildConfig.AutoRoleIds.Remove(id);
                                    await context.Message.ReplyAsync(
                                        $"Removed `{id}` from autoroles");
                                }
                            }
                            else
                            {
                                await context.Message.ReplyAsync(
                                    $"The autoroles don't contain `{id}`");
                            }
                        }
                        else { await context.Message.ReplyAsync($"Unknown property `{context.Parameters[1]}`."); }
                    }
                    else { await context.Message.ReplyAsync($"That is not a valid roleId"); }
                }

                #endregion AutoRole

                #region Antispam

                else if (context.Parameters[0].ToLower().Equals("antispam"))
                {
                    if (context.Parameters.Count != 2)
                    {
                        await context.Message.ReplyAsync("Please a single-word option");
                    }
                    else
                    {
                        switch (context.Parameters[1].ToLower())
                        {
                            case "none":
                                _guildConfig.AntispamLevel = GuildConfig.AntiSpamLevel.None;
                                await context.Message.ReplyAsync("Antispam Level **None** has been selected! The following options are enabled: None");
                                break;
                            case "basic":
                                _guildConfig.AntispamLevel = GuildConfig.AntiSpamLevel.Basic;
                                await context.Message.ReplyAsync("Antispam Level **Basic** has been selected! The following options are enabled: None (yet)");
                                break;
                            case "advanced":
                                _guildConfig.AntispamLevel = GuildConfig.AntiSpamLevel.Advanced;
                                await context.Message.ReplyAsync("Antispam Level **Advanced** has been selected! The following options are enabled: Username Link Kicking");
                                break;
                            case "aggressive":
                                _guildConfig.AntispamLevel = GuildConfig.AntiSpamLevel.Aggressive;
                                await context.Message.ReplyAsync("Antispam Level **Aggressive** has been selected! The following options are enabled: Username Link Banning");
                                break;
                            case "activeraid":
                                _guildConfig.AntispamLevel = GuildConfig.AntiSpamLevel.ActiveRaid;
                                await context.Message.ReplyAsync("Antispam Level **ActiveRaid** has been selected! The following options are enabled: Username Link Banning");
                                break;
                            default:
                                await context.Message.ReplyAsync("That is not an available option. You can select: `None`, `Basic`, `Advanced`, `Aggressive`, and `ActiveRaid`");
                                break;
                        }
                    }
                }

                #endregion Antispam

                #region Points

                else if (context.Parameters[0].ToLower().Equals("points"))
                {
                    if (context.Parameters[1].ToLower().Equals("true") || context.Parameters[1].ToLower().Contains("enable"))
                    {
                        _guildConfig.PointsEnabled = true;
                    }
                    else if (context.Parameters[1].ToLower().Equals("false") || context.Parameters[1].ToLower().Contains("disable"))
                    {
                        _guildConfig.PointsEnabled = false;
                    }
                    else
                    {
                         await context.Message.ReplyAsync($"Unknown property `{context.Parameters[1]}`.");
                    }
                    await context.Message.ReplyAsync($"Points Enabled: {_guildConfig.PointsEnabled}");
                }

                #endregion Points

                #region Trusted Role

                else if (context.Parameters.FirstOrDefault().Equals("trustedrole"))
                {
                    context.Parameters.RemoveAt(0);
                    if (context.Parameters.Count() < 2)
                    {
                        await context.Message.ReplyAsync("Please provide an action and a numeric value.");
                    }

                    var command = context.Parameters[0];
                    var argument = context.Parameters[1];
                    if (ulong.TryParse(argument, out ulong argumentAsLong))
                    {
                        switch (command)
                        {
                            case "setrole":
                                _guildConfig.TrustedRoleId = argumentAsLong;
                                break;
                            case "threshold":
                                _guildConfig.TrustedRolePointThreshold = argumentAsLong;
                                break;
                            default:
                                await context.Message.ReplyAsync($"Unknown property `{command}`.");
                                break;
                        }
                    }
                    else
                    {
                        context.Message.ReplyAsync($"Invalid argument `{argument}`.");
                    }
                }
                #endregion

                else await context.Message.ReplyAsync($"Unknown property `{context.Parameters[0]}`.");

                Core.SaveGuildConfig(_guildConfig);
            };
            commands.Add(config);

            Command audit = new Command("audit");
            audit.Description = "Get the audit log of mod commands for the server";
            audit.RequiredPermission = Command.PermissionLevels.Admin;
            audit.ToExecute += async (context) =>
            {
                var log = Core.GetAuditLog(context.Guild.Id);
                ulong uIdToSearch = 0;
                if (!context.Parameters.IsEmpty())
                    if (ulong.TryParse(context.Parameters[0], out uIdToSearch))
                        log = log.Where(l => l.UserId == uIdToSearch).ToList();
                log = log.OrderByDescending(l => l.MessageId).ToList();

                string message = string.Empty;
                int i = 0;
                while(i < log.Count)
                {
                    var cmd = log.ElementAt(i++);
                    if (message.Length + $"{cmd.Message} - <@{cmd.UserId}>\n".Length > 2000)
                        break;
                    message = $"{cmd.Message} - <@{cmd.UserId}>\n" + message;
                }
                var builder = new EmbedBuilder()
                    .WithDescription(message)
                    .WithColor(new Color(0xFFFF00));
                await context.Channel.SendMessageAsync("", embed: builder.Build());
            };
            commands.Add(audit);

            Command runas = new Command("runas");
            runas.RequiredPermission = Command.PermissionLevels.BotOwner;
            runas.ToExecute += async (context) =>
            {
                if (context.Parameters.Count < 2 || context.Message.GetMentionedUsers().Count != 1)
                {
                    await context.Message.ReplyAsync("Please make sure to provide a userid and a command to run");
                    return;
                }
                try
                {
                    // >runas [UserId] [Command] [Params]
                    ParsedCommand runContext = new ParsedCommand();
                    runContext = context;
                    string message = context.ParameterString.Trim();
                    var runAsUser = context.Guild.GetUser(ulong.Parse(context.Parameters[0].Trim("<@! >".ToCharArray())));

                    if (Core.Commands.HasElement(c => context.Parameters[1].Equals(c.Name) || c.Aliases.Any(a => context.Parameters[1].Equals(a)), out Command cmd))
                    {
                        runContext.RawCommand = cmd;
                        // remove first two params (uid and command)
                        message = new Regex(Regex.Escape(context.Parameters[0])).Replace(message, "", 1).TrimStart();
                        message = new Regex(Regex.Escape(context.Parameters[1])).Replace(message, "", 1).Trim();

                        // set new parameters
                        runContext.ParameterString = message;
                        runContext.Parameters = message.Split().Where(p => !string.IsNullOrEmpty(p.Trim())).ToList();
                        runContext.Author = runAsUser;

                        await runContext.Execute();
                    }
                    else
                    {
                        await context.Message.ReplyAsync("Invalid command");
                    }
                }
                catch (Exception ex)
                {
                    await context.Message.ReplyAsync($"```{ex.Message}```");
                    await context.Message.ReplyAsync($"```{ex.StackTrace}```");
                    throw ex;
                }
            };
            commands.Add(runas);

            Command dumpdb = new Command("dumpdb");
            dumpdb.RequiredPermission = Command.PermissionLevels.BotOwner;
            dumpdb.ToExecute += async (context) =>
            {
                File.WriteAllText("dump/guilds.json", JsonConvert.SerializeObject((Core.DatabaseEngine as MongoEngine).GetGuildIdsFromDb(), Formatting.Indented));
                foreach(var guildid in (Core.DatabaseEngine as MongoEngine).GetGuildIdsFromDb())
                {
                    // CustomCommands
                    try
                    {
                        File.WriteAllText($"dump/{guildid}-customCommands.json", JsonConvert.SerializeObject(Core.GetCustomCommands(ulong.Parse(guildid)), Formatting.Indented));
                        //Console.WriteLine(JsonConvert.SerializeObject(Core.GetCustomCommands(ulong.Parse(guildid)), Formatting.Indented));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception {ex.Message} occured dumping customCommands for {guildid}\n{ex.StackTrace}");
                        System.Threading.Thread.Sleep(1);
                    }
                    Console.WriteLine("Dumped CustomCommands");
                    // Config
                    try
                    {
                        File.WriteAllText($"dump/{guildid}-config.json", JsonConvert.SerializeObject(Core.GetGuildConfig(ulong.Parse(guildid)), Formatting.Indented));
                        //Console.WriteLine(JsonConvert.SerializeObject(Core.GetGuildConfig(ulong.Parse(guildid)), Formatting.Indented));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception {ex.Message} occured dumping config for {guildid}\n{ex.StackTrace}");
                        System.Threading.Thread.Sleep(1);
                    }
                    Console.WriteLine("Dumped config");
                    // Bans
                    try
                    {
                        File.WriteAllText($"dump/ {guildid}-bans.json", JsonConvert.SerializeObject(Core.GetBansFromGuild(ulong.Parse(guildid)), Formatting.Indented));
                        //Console.WriteLine(JsonConvert.SerializeObject(Core.GetBansFromGuild(ulong.Parse(guildid)), Formatting.Indented));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception {ex.Message} occured dumping bans for {guildid}\n{ex.StackTrace}");
                        System.Threading.Thread.Sleep(1);
                    }
                    Console.WriteLine("Dumped bans");
                    // Users
                    try
                    {
                        File.WriteAllText($"dump/{guildid}-users.json", JsonConvert.SerializeObject(Core.GetAllUsers(ulong.Parse(guildid)), Formatting.Indented));
                        //Console.WriteLine(JsonConvert.SerializeObject(Core.GetAllUsers(ulong.Parse(guildid)), Formatting.Indented));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception {ex.Message} occured dumping users for {guildid}\n{ex.StackTrace}");
                        System.Threading.Thread.Sleep(1);
                    }
                    Console.WriteLine("Dumped users");
                    // Giveaways
                    try
                    {
                        File.WriteAllText($"dump/{guildid}-giveaways.json", JsonConvert.SerializeObject(Core.GetGiveaways(ulong.Parse(guildid)), Formatting.Indented));
                        //Console.WriteLine(JsonConvert.SerializeObject(Core.GetGiveaways(ulong.Parse(guildid)), Formatting.Indented));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception {ex.Message} occured dumping giveaways for {guildid}\n{ex.StackTrace}");
                        System.Threading.Thread.Sleep(1);
                    }
                    Console.WriteLine("Dumped quotes");
                }
                await context.Message.ReplyAsync("Done!");
            };
            commands.Add(dumpdb);

            return commands;
        }
    }
}
