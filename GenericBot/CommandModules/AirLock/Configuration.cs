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

        [JsonPropertyName("application-channel-id")]
        public ulong ApplicationChannelID { get; set; }

        [JsonPropertyName("intro-channel-id")]
        public ulong IntroChannelID { get; set; }

    }
}
