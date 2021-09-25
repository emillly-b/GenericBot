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
        [JsonPropertyName("intro-url")]
        public string IntroURL { get; set; }
        
        public User(ulong id)
        {
            UserID = id;
            Authorized = false;
            IntroURL = string.Empty;
        }

        public User ()
        {
            //
        }
    }
}
