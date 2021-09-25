using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SaturnBot.CommandModules.AirLock
{
    public class Configuration
    {

        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        [JsonPropertyName("delete-intros")]
        public bool DeleteIntros { get; set; }

        [JsonPropertyName("unsafe-channel-id")]
        public ulong UnsafeChannelId { get; set; }

        [JsonPropertyName("safe-channel-id")]
        public ulong SafeChannelId { get; set; }

        [JsonPropertyName("unsafe-role-id")]
        public ulong UnsafeRoleId { get; set; }

        [JsonPropertyName("safe-role-id")]
        public ulong SafeRoleId { get; set; }

        public Configuration()
        {
            Enabled = false;
            DeleteIntros = false;
            UnsafeChannelId = 0;
            SafeChannelId = 0;
        }

        public void UpdateSafeChannel(ulong channelId)
        {
            SafeChannelId = channelId;
            Database.SaveConfiguration(this);
        }
        public void UpdateUnsafeChannel(ulong channelId)
        {
            UnsafeChannelId = channelId;
            Database.SaveConfiguration(this);
        }
        public void UpdateSafeRole(ulong roleId)
        {
            SafeRoleId = roleId;
            Database.SaveConfiguration(this);
        }
        public void UpdateUnsafeRole(ulong roleId)
        {
            UnsafeRoleId = roleId;
            Database.SaveConfiguration(this);
        }
    }
}
