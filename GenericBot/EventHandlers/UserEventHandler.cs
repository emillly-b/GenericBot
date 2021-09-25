using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using SaturnBot.Database;
using SaturnBot.Entities;

namespace SaturnBot
{
    public static class UserEventHandler
    {
        public static async Task UserUpdated(SocketUser bUser, SocketUser aUser)
        {
            SocketGuildUser beforeUser = bUser as SocketGuildUser;
            SocketGuildUser afterUser = aUser as SocketGuildUser;
            if (beforeUser == null || beforeUser.Username != afterUser.Username || beforeUser.Nickname != afterUser.Nickname)
            {
                var user = Core.GetUserFromGuild(afterUser.Id, afterUser.Guild.Id, log: false);
                if (beforeUser != null)
                {
                    user.AddUsername(beforeUser.Username);
                    user.AddNickname(beforeUser);
                }
                user.AddUsername(afterUser.Username);
                user.AddNickname(afterUser);
                Core.SaveUserToGuild(user, afterUser.Guild.Id, log: false);
            }
        }

        public static async Task UserJoined(SocketGuildUser user)
        {
            var dbUser = Core.GetUserFromGuild(user.Id, user.Guild.Id);
            bool alreadyJoined = dbUser.Usernames != null;
            dbUser.AddUsername(user.Username);
            dbUser.IsPresent = true;
            Core.SaveUserToGuild(dbUser, user.Guild.Id);
        }

        public static async Task UserLeft(SocketGuildUser user)
        {
            var dbUser = Core.GetUserFromGuild(user.Id, user.Guild.Id);
            dbUser.IsPresent = false;
            Core.SaveUserToGuild(dbUser, user.Guild.Id);
        }
    }
}
