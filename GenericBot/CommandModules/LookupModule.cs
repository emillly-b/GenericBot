using Discord.WebSocket;
using GenericBot.Database;
using GenericBot.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;

namespace GenericBot.CommandModules
{
    class LookupModule : Module
    {
        public List<Command> Load()
        {
            List<Command> commands = new List<Command>();

            Command find = new Command("find");
            find.Description = "Get information about a user currently on the server from a ID or Mention";
            find.Usage = "whois @user";
            find.RequiredPermission = Command.PermissionLevels.Moderator;
            find.Aliases = new List<string> { "whois" };
            find.ToExecute += async (context) =>
            {
                List<DatabaseUser> foundUsers = new List<DatabaseUser>();

                if (context.Message.MentionedUsers.Any())
                {
                    foundUsers.AddRange(context.Message.MentionedUsers.Select(m =>
                    Core.GetUserFromGuild(m.Id, context.Guild.Id)));
                }
                else if ((context.ParameterString.Length > 16 && context.ParameterString.Length < 19) && ulong.TryParse(context.ParameterString, out ulong id))
                {
                    foundUsers.Add(Core.GetUserFromGuild(id, context.Guild.Id));
                }
                else if (context.ParameterString.Length < 3)
                {
                    await context.Message.ReplyAsync($"I can't search for that, it's dangerously short and risks a crash.");
                    return;
                }

                else
                {
                    foreach (var user in Core.GetAllUsers(context.Guild.Id))
                    {
                        try
                        {
                            if (!user.Nicknames.IsEmpty())
                            {
                                if (user.Nicknames.Any(n => n.ToLower().Contains(context.ParameterString.ToLower())))
                                {
                                    foundUsers.Add(user);
                                }
                            }

                            if (!user.Usernames.IsEmpty())
                            {
                                if (user.Usernames.Any(n => n.ToLower().Contains(context.ParameterString.ToLower())))
                                {
                                    foundUsers.Add(user);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (new Command("test").GetPermissions(context) >= Command.PermissionLevels.GlobalAdmin)
                                await context.Message.ReplyAsync($"```\n{ex.Message}\n{ex.StackTrace}\n{user.Id} : {user.Usernames.Count} | {user.Nicknames.Count}\n```");
                        }
                    }
                    foundUsers = foundUsers.Distinct().ToList();
                }

                if (foundUsers.Count > 5)
                {
                    string info =
                        $"Found `{foundUsers.Count}` users. Their first stored usernames are:\n{foundUsers.Select(u => $"{u.Usernames.First().Escape()} (`{u.Id}`)").ToList().SumAnd()}" +
                        $"\nTry using more precise search parameters";

                    foreach (var str in info.MessageSplit(','))
                    {
                        await context.Message.ReplyAsync(str.TrimStart(','));
                    }

                    return;
                }
                else if (foundUsers.Count == 0)
                {
                    await context.Message.ReplyAsync($"No users found");
                }

                foreach (var dbUser in foundUsers)
                {
                    string nicks = "", usernames = "";
                    if (dbUser.Usernames != null && !dbUser.Usernames.IsEmpty())
                    {
                        usernames = dbUser.Usernames.Distinct().Select(n => $"`{n.Replace("`", "'")}`").ToList().SumAnd();
                    }

                    if (dbUser.Nicknames != null && !dbUser.Nicknames.IsEmpty())
                    {
                        nicks = dbUser.Nicknames.Distinct().Select(n => $"`{n.Replace("`", "'")}`").ToList().SumAnd();
                    }


                    string info = $"User: <@!{dbUser.Id}>\nUser Id:  `{dbUser.Id}`\n";
                    info += $"Past Usernames: {usernames}\n";
                    info += $"Past Nicknames: {nicks}\n";
                    SocketGuildUser user = context.Message.GetGuild().GetUser(dbUser.Id);
                    if (user != null && user.Id != context.Message.Author.Id)
                    {
                        info +=
                            $"Created At: `{string.Format("{0:yyyy-MM-dd HH\\:mm\\:ss zzzz}", user.CreatedAt.LocalDateTime)}GMT` " +
                            $"(about {(DateTime.UtcNow - user.CreatedAt).Days} days ago)\n";
                    }

                    if (user != null && user.Id != context.Message.Author.Id && user.JoinedAt.HasValue)
                        info +=
                            $"Joined At: `{string.Format("{0:yyyy-MM-dd HH\\:mm\\:ss zzzz}", user.JoinedAt.Value.LocalDateTime)}GMT`" +
                            $"(about {(DateTime.UtcNow - user.JoinedAt.Value).Days} days ago)\n";
                    if (dbUser.Warnings != null && !dbUser.Warnings.IsEmpty())
                        info += $"`{dbUser.Warnings.Count}` Warnings: {dbUser.Warnings.SumAnd()}";

                    foreach (var str in info.MessageSplit(','))
                    {
                        await context.Message.ReplyAsync(str.TrimStart(','));
                    }
                }
            };
            commands.Add(find);
            
            Command repairDb = new Command("repairdb");
            repairDb.RequiredPermission = Command.PermissionLevels.GlobalAdmin;
            repairDb.ToExecute += async (context) =>
            {
                int i = 0;
                foreach(var user in Core.GetAllUsers(context.Guild.Id))
                {
                    user.Usernames = user.Usernames.Where(n => n != null).ToList();
                    user.Nicknames = user.Nicknames.Where(n => n != null).ToList();
                    Core.SaveUserToGuild(user, context.Guild.Id, false);
                    i++;
                }
                await context.Message.ReplyAsync($"Updated {i} users");
            };
            commands.Add(repairDb);

            Command updateDb = new Command("updatedb");
            updateDb.RequiredPermission = Command.PermissionLevels.GlobalAdmin;
            updateDb.ToExecute += async (context) =>
            {
                await context.Guild.DownloadUsersAsync();
                int i = 0;
                foreach(var user in context.Guild.Users)
                {
                    var dbUser = Core.GetUserFromGuild(user.Id, context.Guild.Id);
                    dbUser.AddNickname(user);
                    dbUser.AddUsername(user.Username);
                    Core.SaveUserToGuild(dbUser, context.Guild.Id, false);
                    i++;
                }
                await context.Message.ReplyAsync($"Updated `{i}` users.");
            };
            commands.Add(updateDb);

            return commands;
        }
    }
}
