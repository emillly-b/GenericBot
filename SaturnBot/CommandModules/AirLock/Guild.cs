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
        
        public bool Equals(Guild a, Guild b)
        {
            if (a.Id == b.Id)
                return true;
            return false;
        }
    }
}
