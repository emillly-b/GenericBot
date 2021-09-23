using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using GenericBot.Database;
using GenericBot.Entities;

namespace GenericBot.CommandModules
{
    public class RoleModule : Module
    {
        public List<Command> Load()
        {
            List<Command> commands = new List<Command>();

            Command UserRoles = new Command("userroles");
            UserRoles.Description = $"Show all user roles on this server";
            UserRoles.Usage = "userroles";
            UserRoles.ToExecute += async (context) =>
            {
                string prefix = Core.GetPrefix(context);
                string message = $"You can use `{prefix}iam` and `{prefix}iamnot` with any of these roles:\n _\\*(Pro tip: You can add/remove more than one role at a time by seperating each role with a comma like `{prefix}iam role0, role1, role2, etc`)*_\n";
                var config = Core.GetGuildConfig(context.Guild.Id);
                if(config.UserRoles.Count < 1 || 
                    config.UserRoles
                        .Sum(kvp => kvp.Value
                            .Count(r => context.Guild.Roles.Any(gr => gr.Id == r))) < 1)
                {
                    await context.Message.ReplyAsync("It looks like there are no available user roles on this server. If you believe this is in error, please talk to your server administrators!");
                    return;
                }
                foreach (var group in config.UserRoles.Where(g => !string.IsNullOrEmpty(g.Key) && g.Value.Count > 0).OrderBy(g => g.Key))
                {
                    message += $"**{group.Key}:** ";
                    foreach (var role in context.Guild.Roles
                        .Where(r => group.Value.Contains(r.Id))
                        .OrderBy(r => r.Name))
                    {
                        if ((context.Author as SocketGuildUser).Roles.Contains(role))
                            message += "\\✔ ";
                        else
                            message += "✘";
                        message += $"`{role.Name}`, ";
                    }
                    message += "\n";
                }

                if (config.UserRoles.Keys.Contains("") && config.UserRoles[""].Count > 0)
                {
                    message += $"**Ungrouped:** ";
                    foreach (var role in context.Guild.Roles
                        .Where(r => config.UserRoles[""].Contains(r.Id))
                        .OrderBy(r => r.Name))
                    {
                        if ((context.Author as SocketGuildUser).Roles.Contains(role))
                            message += "\\✔ ";
                        else
                            message += "✘";
                        message += $"`{role.Name}`, ";
                    }
                    message += "\n";
                }

                message = message.Trim(' ', ',', '\n');
                message += $"\n You can also use `{prefix}rolestore save` to backup your assigned roles";

                foreach (var str in message.MessageSplit())
                {
                    await context.Message.ReplyAsync(str);
                }
            };
            commands.Add(UserRoles);

            Command requiresRoles = new Command("requiresRoles");
            requiresRoles.Description = "list all available roles that have required roles";
            requiresRoles.WorksInDms = false;
            requiresRoles.ToExecute += async (context) =>
            {
                string prefix = Core.GetPrefix(context);
                string message = $"You can use `{prefix}iam` and `{prefix}iamnot` with any of these roles:\n _\\*(Pro tip: You can add/remove more than one role at a time by seperating each role with a comma like `{prefix}iam role0, role1, role2, etc`)*_\n";
                var config = Core.GetGuildConfig(context.Guild.Id);
                // check that any are available
                if (!config.RequiresRoles.Any(rr => context.Guild.Roles.Any(r => r.Id == rr.Key)))
                {
                    await context.Message.ReplyAsync("It looks like there are no roles with prerequisites on this server. If you believe this is in error, please talk to your server administrators!");
                    return;
                }
                foreach (var roleWithReqs in config.RequiresRoles)
                {
                    if (!context.Guild.Roles.Any(r => r.Id == roleWithReqs.Key))
                        continue;
                    var actuallyRequiredRoles = roleWithReqs.Value.Where(id => context.Guild.Roles.Any(r => r.Id == id));
                    if (actuallyRequiredRoles.Any())
                    {
                        message += $"\n**{context.Guild.Roles.First(r => r.Id == roleWithReqs.Key).Name}**: requires {actuallyRequiredRoles.Select(r => $"{(context.Guild.GetUser(context.Author.Id).Roles.Any(ur => ur.Id == r) ? "\\✔" : "✘")} {context.Guild.Roles.First(role => role.Id == r).Name}").ToList().SumAnd()}";
                    }
                    else
                    {
                        message += $"\n**{context.Guild.Roles.First(r => r.Id == roleWithReqs.Key).Name}**: requires no roles... this should probably be a UserRole!";
                    }
                }

                foreach (var str in message.MessageSplit())
                {
                    await context.Message.ReplyAsync(str);
                }
            };
            commands.Add(requiresRoles);

            Command iam = new Command("iam");
            iam.Description = "Join a User Role";
            iam.Usage = "iam <role name>";
            iam.Aliases = new List<string> { "join" };
            iam.ToExecute += async (context) =>
            {
                List<IMessage> messagesToDelete = new List<IMessage>();
                DatabaseUser dbUser = Core.GetUserFromGuild(context.Author.Id, context.Guild.Id);

                if (context.Parameters.IsEmpty())
                {
                    messagesToDelete.Add(context.Channel.SendMessageAsync("", embed: new EmbedBuilder().WithDescription("Please select a role to join").WithColor(new Color(0xFFFF00)).Build()).Result);
                }

                foreach (var roleName in context.ParameterString.Trim(',', ' ').Split(',').Select(r => r.Trim('"')))
                {
                    if (string.IsNullOrWhiteSpace(roleName))
                        continue;

                    var roles = context.Guild.Roles.Where(r => r.Name.ToLower().Contains(roleName.ToLower().Trim()))
                        .Where(r => Core.GetGuildConfig(context.Guild.Id).UserRoles.Any(rg => rg.Value.Contains(r.Id))).ToList();

                    List<SocketRole> reqRoles = new List<SocketRole>();
                    if (Core.GetGuildConfig(context.Guild.Id).RequiresRoles != null)
                        reqRoles = context.Guild.Roles.Where(r => r.Name.ToLower().Contains(roleName.ToLower().Trim()))
                            .Where(r => Core.GetGuildConfig(context.Guild.Id).RequiresRoles.ContainsKey(r.Id)).ToList();
                    roles.AddRange(reqRoles);

                    if (!roles.Any())
                    {
                        messagesToDelete.Add(context.Channel.SendMessageAsync("", embed: new EmbedBuilder().WithDescription($"Could not find any assignable matching `{roleName}`").WithColor(new Color(0xFFFF00)).Build()).Result);
                    }
                    else
                    {
                        try
                        {
                            var role = roles.Any(r => r.Name.ToLower() == roleName.ToLower())
                                ? roles.First(r => r.Name.ToLower() == roleName.ToLower())
                                : roles.First();
                            if (Core.GetGuildConfig(context.Guild.Id).RequiresRoles != null && 
                                Core.GetGuildConfig(context.Guild.Id).RequiresRoles.ContainsKey(role.Id))
                            {
                                if (!Core.GetGuildConfig(context.Guild.Id).RequiresRoles[role.Id].All(r => context.Guild.GetUser(context.Author.Id).Roles.Any(ur => ur.Id == r)))
                                {
                                    messagesToDelete.Add(context.Channel.SendMessageAsync("", embed: new EmbedBuilder().WithDescription($"You don't have the requires roles for `{role.Name}`").WithColor(new Color(0xFFFF00)).Build()).Result);
                                    continue;
                                }
                            }
                            if (context.Guild.GetUser(context.Author.Id).Roles.Any(r => r.Id == role.Id))
                            {
                                messagesToDelete.Add(context.Channel.SendMessageAsync("", embed: new EmbedBuilder().WithDescription($"You already have that role!").WithColor(new Color(0xFFFF00)).Build()).Result);
                            }
                            else
                            {
                                await context.Guild.GetUser(context.Author.Id).AddRoleAsync(role);
                                dbUser.AddStoredRole(role.Id);
                                messagesToDelete.Add(context.Channel.SendMessageAsync("", embed: new EmbedBuilder().WithDescription($"Assigned you `{role.Name}`").WithColor(new Color(0x9B00)).Build()).Result);
                            }
                        }
                        catch (Exception e)
                        {
                            await Core.Logger.LogErrorMessage(e, context);
                            messagesToDelete.Add(context.Channel.SendMessageAsync("", embed: new EmbedBuilder().WithDescription($"I may not have permissions to do that").WithColor(new Color(0xFFFF00)).Build()).Result);
                        }
                    }
                }
                Core.SaveUserToGuild(dbUser, context.Guild.Id);

                await Task.Delay(15 * 1000);
                try
                {
                    messagesToDelete.ForEach(m => Program.ClearedMessageIds.Add(m.Id));
                    messagesToDelete.Add(context.Message);
                    await (context.Channel as ITextChannel).DeleteMessagesAsync(messagesToDelete);
                }
                catch (Exception ex)
                {
                    await Core.Logger.LogErrorMessage(ex, context);
                    try
                    {
                        foreach (var m in messagesToDelete)
                        {
                            await m.DeleteAsync();
                        }
                    }
                    catch (Exception e)
                    {
                        await Core.Logger.LogErrorMessage(e, context);
                    }
                }
            };
            commands.Add(iam);

            Command iamnot = new Command("iamnot");
            iamnot.Description = "Leave a User Role";
            iamnot.Usage = "iamnot <role name>";
            iamnot.Aliases = new List<string> { "leave" };
            iamnot.ToExecute += async (context) =>
            {
                List<IMessage> messagesToDelete = new List<IMessage>();
                DatabaseUser dbUser = Core.GetUserFromGuild(context.Author.Id, context.Guild.Id);

                if (context.Parameters.IsEmpty())
                {
                    messagesToDelete.Add(context.Channel.SendMessageAsync("", embed: new EmbedBuilder().WithDescription("Please select a role to remove").WithColor(new Color(0xFFFF00)).Build()).Result);
                }

                foreach (var roleName in context.ParameterString.Trim(',', ' ').Split(',').Select(r => r.Trim('"')))
                {
                    if (string.IsNullOrWhiteSpace(roleName))
                        continue;
                    var roles = context.Guild.Roles.Where(r => r.Name.ToLower().Contains(roleName.ToLower().Trim()))
                        .Where(r => Core.GetGuildConfig(context.Guild.Id).UserRoles.Any(rg => rg.Value.Contains(r.Id))).ToList();
                    if (Core.GetGuildConfig(context.Guild.Id).RequiresRoles != null)
                    {
                        var reqRoles = context.Guild.Roles.Where(r => r.Name.ToLower().Contains(roleName.ToLower().Trim()))
                            .Where(r => Core.GetGuildConfig(context.Guild.Id).RequiresRoles.ContainsKey(r.Id)).ToList();
                        roles.AddRange(reqRoles);
                    }
                    if (!roles.Any())
                    {
                        messagesToDelete.Add(context.Channel.SendMessageAsync("", embed: new EmbedBuilder().WithDescription($"Could not find any user roles matching `{roleName}`").WithColor(new Color(0xFFFF00)).Build()).Result);
                    }
                    else
                    {
                        try
                        {
                            var role = roles.Any(r => r.Name.ToLower() == roleName.ToLower())
                                ? roles.First(r => r.Name.ToLower() == roleName.ToLower())
                                : roles.First();
                            dbUser.RemoveStoredRole(role.Id);
                            if (!context.Guild.GetUser(context.Author.Id).Roles.Any(r => r.Id == role.Id))
                            {
                                messagesToDelete.Add(context.Channel.SendMessageAsync("", embed: new EmbedBuilder().WithDescription($"You don't have that role!").WithColor(new Color(0xFFFF00)).Build()).Result);
                            }
                            else
                            {
                                await context.Guild.GetUser(context.Author.Id).RemoveRoleAsync(role);
                                messagesToDelete.Add(context.Channel.SendMessageAsync("", embed: new EmbedBuilder().WithDescription($"Removed `{role.Name}`").WithColor(new Color(0x9B00)).Build()).Result);
                            }
                        }
                        catch (Exception e)
                        {
                            await Core.Logger.LogErrorMessage(e, context);
                            messagesToDelete.Add(context.Channel.SendMessageAsync("", embed: new EmbedBuilder().WithDescription($"I may not have permissions to do that").WithColor(new Color(0xFFFF00)).Build()).Result);
                        }
                    }
                }
                Core.SaveUserToGuild(dbUser, context.Guild.Id);

                await Task.Delay(15 * 1000);
                try
                {
                    messagesToDelete.ForEach(m => Program.ClearedMessageIds.Add(m.Id));
                    messagesToDelete.Add(context.Message);
                    await (context.Channel as ITextChannel).DeleteMessagesAsync(messagesToDelete);
                }
                catch (Exception ex)
                {
                    await Core.Logger.LogErrorMessage(ex, context);
                    try
                    {
                        foreach (var m in messagesToDelete)
                        {
                            await m.DeleteAsync();
                        }
                    }
                    catch (Exception e)
                    {
                        await Core.Logger.LogErrorMessage(e, context);
                    }
                }
            };
            commands.Add(iamnot);

            Command getrole = new Command("getrole");
            getrole.Description = "Get the ID of a role";
            getrole.Usage = "getrole <role name>";
            getrole.RequiredPermission = Command.PermissionLevels.Moderator;
            getrole.ToExecute += async (context) =>
            {
                string message = $"Roles matching `{context.ParameterString}`:\n";
                foreach (var role in context.Guild.Roles.Where(r => Regex.IsMatch(r.Name, context.ParameterString, RegexOptions.IgnoreCase)).OrderBy(r => r.Name))
                {
                    message += $"{role.Name} (`{role.Id}`)\n";
                }

                foreach (var str in message.MessageSplit())
                {
                    await context.Message.ReplyAsync(str);
                }
            };
            commands.Add(getrole);

            Command membersOf = new Command("membersof");
            membersOf.Description = "List all members of a role";
            membersOf.Usage = "membersof <rolename>";
            membersOf.RequiredPermission = Command.PermissionLevels.Moderator;
            membersOf.ToExecute += async (context) =>
            {
                if (context.Parameters.IsEmpty())
                {
                    await context.Message.ReplyAsync($"You need to specify a role");
                    return;
                }
                string result = "";
                foreach (var role in context.Guild.Roles.OrderByDescending(r => r.Position).Where(r => new Regex(context.ParameterString, RegexOptions.IgnoreCase).IsMatch(r.Name) && r.Name != "@everyone"))
                {
                    result += $"\n**`{role.Name}` ({role.Members.Count()} Members)**\n";
                    foreach (var user in role.Members.OrderBy(u => u.Username))
                    {
                        if (!string.IsNullOrEmpty(user.Nickname)) result += $"{user.Nickname.Replace('`', '\'').Replace("_", "\\_")} ";
                        else result += $"{user.Username.Replace('`', '\'').Replace("_", "\\_")} ";
                        result += $"(`{user.ToString().Replace('`', '\'')}`)\n";
                    }
                }

                foreach (var str in result.MessageSplit('\n'))
                {
                    await context.Message.ReplyAsync(str);
                }

            };
            commands.Add(membersOf);

            Command createRole = new Command("createRole");
            createRole.Description = "Create a new role with default permissions";
            createRole.Usage = "createRole <name>";
            createRole.RequiredPermission = Command.PermissionLevels.Admin;
            createRole.ToExecute += async (context) =>
            {
                RestRole role;

                role = context.Guild.CreateRoleAsync(name: context.ParameterString, permissions: GuildPermissions.None, null, false, null).Result;
                await context.Message.ReplyAsync($"Created new role `{role.Name}` with ID `{role.Id}`");
            };
            commands.Add(createRole);

            Command createUserRole = new Command("createUserRole");
            createUserRole.Description = "Create a new role with default permissions and add it to the public role list";
            createUserRole.Usage = "createUserRole <name>";
            createUserRole.RequiredPermission = Command.PermissionLevels.Admin;
            createUserRole.ToExecute += async (context) =>
            {
                RestRole role;

                role = context.Guild.CreateRoleAsync(context.ParameterString, GuildPermissions.None, null, false, null).Result;
                var gc = Core.GetGuildConfig(context.Guild.Id);
                if (gc.UserRoles.ContainsKey(""))
                    gc.UserRoles[""].Add(role.Id);
                else
                    gc.UserRoles.Add("", new List<ulong> { role.Id });
                Core.SaveGuildConfig(gc);
                await context.Message.ReplyAsync($"Created new role `{role.Name}` with ID `{role.Id}` and added it to the user roles");
            };
            commands.Add(createUserRole);


            Command verify = new Command("verify");
            verify.RequiredPermission = Command.PermissionLevels.User;
            verify.Description = "Send the verification DM to the user who ran the command or was mentioned, if enabled";
            verify.ToExecute += async (context) =>
            {
                List<SocketUser> users = new List<SocketUser>();
                var guildConfig = Core.GetGuildConfig(context.Guild.Id);

                if (context.Parameters.IsEmpty())
                {
                    if ((context.Author as SocketGuildUser).Roles.Any(r => r.Id == guildConfig.VerifiedRole))
                    {
                        await context.Message.ReplyAsync("You're already verified");
                        return;
                    }
                    users.Add(context.Author);
                }
                else
                {
                    foreach (var user in context.Message.GetMentionedUsers())
                    {
                        if ((user as SocketGuildUser).Roles.Any(r => r.Id == guildConfig.VerifiedRole))
                        {
                            await context.Message.ReplyAsync($"{user.Username} is already verified");
                        }
                        else
                        {
                            users.Add(user);
                        }
                    }
                }


                if (guildConfig.VerifiedRole == 0)
                {
                    await context.Message.ReplyAsync($"Verification is disabled on this server");
                    return;
                }

                if ((string.IsNullOrEmpty(guildConfig.VerifiedMessage) || guildConfig.VerifiedMessage.Split().Length < 32 || !context.Guild.Roles.Any(r => r.Id == guildConfig.VerifiedRole)))
                {
                    await context.Message.ReplyAsync(
                        $"It looks like verifiction is configured improperly (either the message is too short or the role does not exist.) Please contact your server administrator to resolve it.");
                    return;
                }

                List<SocketUser> failed = new List<SocketUser>();
                List<SocketUser> success = new List<SocketUser>();
                foreach (var user in users)
                {
                    string message = $"Hey {user.Username}! To get verified on **{context.Guild.Name}** reply to this message with the hidden code in the message below\n\n"
                                     + Core.GetGuildConfig(context.Guild.Id).VerifiedMessage;

                    string verificationMessage =
                        VerificationEngine.InsertCodeInMessage(message, VerificationEngine.GetVerificationCode(user.Id, context.Guild.Id));

                    try
                    {
                        await user.CreateDMChannelAsync().Result.SendMessageAsync(verificationMessage);
                        success.Add(user);
                    }
                    catch (Exception ex)
                    {
                        await Core.Logger.LogErrorMessage(ex, context);
                        failed.Add(user);
                    }
                }

                string reply = "";
                if (success.Any())
                {
                    reply += $"I've sent {success.Select(u => u.Username).ToList().SumAnd()} instructions!";
                }
                if (failed.Any())
                {
                    reply += $" {failed.Select(u => u.Username).ToList().SumAnd()} could not be messaged.";
                }
                await context.Message.ReplyAsync(reply);
            };
            commands.Add(verify);

            Command verifyall = new Command("verifyall");
            verifyall.RequiredPermission = Command.PermissionLevels.Admin;
            verifyall.Description = "send the verification DM to all users who don't have the verified role";
            verifyall.ToExecute += async (context) =>
            {
                var guildConfig = Core.GetGuildConfig(context.Guild.Id);

                if (guildConfig.VerifiedRole == 0)
                {
                    await context.Message.ReplyAsync($"Verification is disabled on this server");
                    return;
                }
                if (string.IsNullOrEmpty(guildConfig.VerifiedMessage) || guildConfig.VerifiedMessage.Split().Length < 32 || !context.Guild.Roles.Any(r => r.Id == guildConfig.VerifiedRole))
                {
                    await context.Message.ReplyAsync(
                        $"It looks like verifiction is configured improperly (either the message is too short or the role does not exist.) Please contact your server administrator to resolve it.");
                    return;
                }

                List<SocketGuildUser> users = context.Guild.Users.Where(u => !u.Roles.Any(r => r.Id == guildConfig.VerifiedRole)).ToList();

                List<SocketUser> failed = new List<SocketUser>();
                List<SocketUser> success = new List<SocketUser>();
                foreach (var user in users)
                {
                    string message = $"Hey {user.Username}! To get verified on **{context.Guild.Name}** reply to this message with the hidden code in the message below\n\n"
                                     + Core.GetGuildConfig(context.Guild.Id).VerifiedMessage;

                    string verificationMessage =
                        VerificationEngine.InsertCodeInMessage(message, VerificationEngine.GetVerificationCode(user.Id, context.Guild.Id));

                    try
                    {
                        await user.CreateDMChannelAsync().Result.SendMessageAsync(verificationMessage);
                        success.Add(user);
                    }
                    catch (Exception ex)
                    {
                        await Core.Logger.LogErrorMessage(ex, context);
                        failed.Add(user);
                    }
                }

                string reply = "";
                if (success.Any())
                {
                    reply += $"I've sent {success.Count} users instructions!";
                }
                if (failed.Any())
                {
                    reply += $" {failed.Select(u => u.Username).ToList().SumAnd()} ({failed.Count} users) could not be messaged.";
                }

                foreach(var message in reply.MessageSplit())
                    await context.Message.ReplyAsync(reply);
            };
            commands.Add(verifyall);

            Command roleeveryone = new Command("roleeveryone");
            roleeveryone.Aliases = new List<string> { "roleveryone" };
            roleeveryone.Description = "Give or remove a role from everyone";
            roleeveryone.Usage = "roleveryone [add|remove] <roleID>";
            roleeveryone.RequiredPermission = Command.PermissionLevels.Admin;
            roleeveryone.ToExecute += async (context) =>
            {
                if (!(context.Parameters[0].ToLower().Equals("add") || context.Parameters[0].ToLower().Equals("remove")))
                {
                    await context.Message.ReplyAsync($"Invalid option `{context.Parameters[0]}`");
                }
                ulong id;
                if (ulong.TryParse(context.Parameters[1], out id) && context.Guild.Roles.Any(r => r.Id == id))
                {
                    int i = 0;
                    await context.Guild.DownloadUsersAsync();
                    var role = context.Guild.GetRole(id);
                    foreach (var u in context.Guild.Users)
                    {
                        if (context.Parameters[0].ToLower().Equals("remove") && u.Roles.Any(r => r.Id == id))
                        {
                            await u.RemoveRoleAsync(role);
                            i++;
                        }
                        if (context.Parameters[0].ToLower().Equals("add") && !u.Roles.Any(r => r.Id == id))
                        {
                            await u.AddRoleAsync(role);
                            i++;
                        }
                    }
                    string addrem = context.Parameters[0].ToLower().Equals("add") ? "Added" : "Removed";
                    string tofrom = context.Parameters[0].ToLower().Equals("add") ? "to" : "from";
                    await context.Message.ReplyAsync($"{addrem} `{role.Name}` {tofrom} `{i}` users.");
                }
                else await context.Message.ReplyAsync("Invalid Role Id");
            };
            commands.Add(roleeveryone);

            Command rolestore = new Command("rolestore");
            rolestore.Description = "Save or restore all of your userroles";
            rolestore.Usage = "roleveryone [save|restore]";
            rolestore.ToExecute += async (context) =>
            {
                var dbUser = Core.GetUserFromGuild(context.Author.Id, context.Guild.Id);
                if (Core.GetGuildConfig(context.Guild.Id).UserRoles == null || Core.GetGuildConfig(context.Guild.Id).UserRoles.Count < 1
                    || Core.GetGuildConfig(context.Guild.Id).UserRoles.Values.Aggregate((a, b) => a.Concat(b).ToList()).Count < 1)
                {
                    await context.Message.ReplyAsync("This server has no user-assignable roles!");
                    return;
                }
                var userRoles = Core.GetGuildConfig(context.Guild.Id).UserRoles.Values.Aggregate((a, b) => a.Concat(b).ToList());

                if (context.Parameters.Count == 0)
                {
                    var storedRoles = dbUser.GetStoredRoles()
                        .Intersect(context.Guild.Roles.Select(r => r.Id));
                    var restoreableRoles = storedRoles
                        .Where(r => !(context.Author as SocketGuildUser).Roles.Any(u => u.Id == r))
                        .Intersect(userRoles);
                    if (storedRoles.Any())
                    {
                        if (restoreableRoles.Any())
                        {
                            await context.Message
                                .ReplyAsync($"You have `{storedRoles.Count()}` saved roles, and `{restoreableRoles.Count()}` that can be restored! " +
                                $"The restoreable roles are: {restoreableRoles.Select(r => $"`{context.Guild.GetRole(r).Name}`").ToList().SumAnd()}");
                        }
                        else
                        {
                            await context.Message
                                .ReplyAsync($"You have `{storedRoles.Count()}` saved roles, but you already have them all, so none of them can be restored!");
                        }
                    }
                    else
                    {
                        await context.Message.ReplyAsync("You have no saved roles!");
                    }
                }
                else if (context.Parameters.Count == 1)
                {
                    if (context.Parameters[0].ToLower().Equals("save"))
                    {
                        foreach(var role in (context.Author as SocketGuildUser).Roles.Select(r => r.Id))
                        {
                            dbUser.AddStoredRole(role);
                        }
                        var storedRoles = dbUser.GetStoredRoles()
                            .Intersect(context.Guild.Roles.Select(r => r.Id))
                            .Intersect(userRoles);
                        await context.Message.ReplyAsync($"You have `{storedRoles.Count()}` roles you will be able to restore now");
                        Core.SaveUserToGuild(dbUser, context.Guild.Id);
                    }
                    else if (context.Parameters[0].ToLower().Equals("restore"))
                    {
                        var storedRoles = dbUser.GetStoredRoles()
                            .Intersect(context.Guild.Roles.Select(r => r.Id))
                            .Where(r => !(context.Author as SocketGuildUser).Roles.Any(u => u.Id == r))
                            .Intersect(userRoles);
                        if(storedRoles.Count() == 0)
                        {
                            await context.Message.ReplyAsync($"You are missing no saved roles");
                        }
                        else
                        {
                            try
                            {
                                await (context.Author as SocketGuildUser).AddRolesAsync(storedRoles.Select(r => context.Guild.GetRole(r)));
                                await context.Message.ReplyAsync($"Successfully restored `{storedRoles.Count()}` roles for you!");
                            }
                            catch
                            {

                                var missingRoles = dbUser.GetStoredRoles()
                                    .Intersect(context.Guild.Roles.Select(r => r.Id))
                                    .Where(r => !(context.Author as SocketGuildUser).Roles.Any(u => u.Id == r))
                                    .Intersect(userRoles);
                                await context.Message.ReplyAsync($"Uh oh. You had `{storedRoles.Count()}` roles that could be restored, but `{missingRoles.Count()}` couldn't be restored.");
                            }
                        }
                    }
                    else
                            {
                        await context.Message.ReplyAsync("Invalid parameter. Use rolestore with no parameters to see available roles, or `save` or `restore`");
                    }
                }
                else
                {
                    await context.Message.ReplyAsync("Use rolestore with no parameters to see available roles, or `save` or `restore`");
                }
            };
            commands.Add(rolestore);

            return commands;
        }
    }
}
