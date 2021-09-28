using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SaturnBot.CommandModules.AirLock
{
    public class User
    {
        [JsonPropertyName("user-id")]
        public ulong UserID { get; set; }
        [JsonPropertyName("authorized")]
        public bool Authorized { get; set; }
        [JsonPropertyName("intro")]
        public ulong IntroID { get; set; }
        
        public User(ulong id)
        {
            UserID = id;
            Authorized = false;
            IntroID = 0;
        }

        public User(ulong id, ulong introId)
        {
            UserID = id;
            Authorized = true;
            IntroID = introId;
        }

        public User ()
        {
            //
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is User)) return false;
            return ((User)obj).UserID == this.UserID;
        }
    }
}
