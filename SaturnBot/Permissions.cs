using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaturnBot
{
    public static class Permissions
    {
        public enum PermissionLevels
        { 
            None,
            User,
            Moderator,
            Admin,
            GuildOwner,
            GlobalAdmin,
            BotOwner
        }

        public static PermissionLevels GetPermissions(ulong userId, ulong guildId)
        { 
            var guild = Core.DiscordClient.GetGuild(guildId);
            if (guild == null)
                return PermissionLevels.None;
            SocketGuildUser sUser = null;
            RestGuildUser rUser = null;

            try{ sUser = Core.DiscordClient.GetGuild(guildId).GetUser(userId); } catch { }
            if(sUser == null)
                rUser = Core.DiscordClient.GetShardFor(guild).Rest.GetGuildUserAsync(guildId, userId).Result;

            if (Core.CheckBlacklisted(userId))
                return PermissionLevels.None;
            else if (userId.Equals(Core.GetOwnerId()))
                return PermissionLevels.BotOwner;
            else if (Core.CheckGlobalAdmin(userId))
                return PermissionLevels.GlobalAdmin;
            if (sUser != null) 
            {
                if (SocketIsGuildAdmin(sUser, guild))
                    return PermissionLevels.GuildOwner;
                else if (sUser.Roles.Select(r => r.Id).Intersect(Core.GetGuildConfig(guildId).AdminRoleIds).Any())
                    return PermissionLevels.Admin;
                else if (sUser.Roles.Select(r => r.Id).Intersect(Core.GetGuildConfig(guildId).ModRoleIds).Any())
                    return PermissionLevels.Moderator;
            }
            else
            {
                if (RestIsGuildAdmin(rUser, guild))
                    return PermissionLevels.GuildOwner;
                else if (rUser.RoleIds.Intersect(Core.GetGuildConfig(guildId).AdminRoleIds).Any())
                    return PermissionLevels.Admin;
                else if (rUser.RoleIds.Intersect(Core.GetGuildConfig(guildId).ModRoleIds).Any())
                    return PermissionLevels.Moderator;
            }
            return PermissionLevels.User;
        }

        public static bool IsPermitted(ulong userId, ulong guildId, PermissionLevels level)
        {
            try
            {
                return GetPermissions(userId, guildId) >= level;
            }
            catch
            {
                return false;
            }
        }

        private static bool SocketIsGuildAdmin(SocketGuildUser user, SocketGuild guild)
        {
            if (guild.Owner.Id == user.Id)
                return true;
            else if (user.Roles.Any(r => r.Permissions.Administrator))
                return true;
            return false;
        }
        private static bool RestIsGuildAdmin(RestGuildUser user, SocketGuild guild)
        {
            if (guild.Owner.Id == user.Id)
                return true;
            else if (user.RoleIds.Intersect(guild.Roles.Where(r => r.Permissions.Administrator).Select(r => r.Id)).Any())
                return true;
            return false;
        }
    }
}
