using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;


namespace SaturnBot.CommandModules.AirLock
{
    public class Guild
    {
        [JsonPropertyName("guildid")]
        public ulong Id { get; set; }

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

        [JsonPropertyName("admin-role-id")]
        public ulong AdminRoleId { get; set; }

        [JsonPropertyName("mod-role-id")]
        public ulong ModRoleId { get; set; }

        [JsonPropertyName("members")]
        public List<User> Members { get; set; }

        public Guild(ulong id)
        {
            Id = id;
            Members = new List<User>();
        }

        public Guild()
        {
            //
        }

        public User GetUser(ulong id)
        {
            foreach(User cursor in Members)
            {
                var UsertoFind = new User(id);
                if (cursor == UsertoFind)
                    return cursor;
            }
            throw new Exception("User not found");
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is Guild)) return false;
            return ((Guild)obj).Id == this.Id;
        }
    }
}