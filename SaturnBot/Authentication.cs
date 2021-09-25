using SaturnBot.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SaturnBot.Web_Api
{
    public static class Authentication
    {
        private static Dictionary<ulong, object> BasicAuthCache;
        public static Permissions.PermissionLevels GetPermissions(string Token, ulong GuildId)
        {
            if (string.IsNullOrEmpty(Token))
                return Permissions.PermissionLevels.None;

            try
            {
                WebClient client = new WebClient();
                client.Headers.Add("Authorization", $"Bearer {Token}");
                string userData = client.DownloadString("https://discordapp.com/api/v6/users/@me");

                PartialUser pUser = JsonConvert.DeserializeObject<PartialUser>(userData);
                return Permissions.GetPermissions(pUser.id, GuildId);
            }
            catch
            {
                return Permissions.PermissionLevels.None;
            }
        }
    }
}
