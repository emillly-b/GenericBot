using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using SaturnBot.Entities;

namespace SaturnBot.CommandModules.AirLock
{
    public class Configuration
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        public Configuration()
        {
            Enabled = true;
        }
    }
}
