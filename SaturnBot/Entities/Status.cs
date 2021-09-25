using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SaturnBot.Entities
{
    [BsonIgnoreExtraElements]
    public class Status
    {
        public DateTimeOffset Time { get; set; }
        public int ConnectedGuilds { get; set; }
        public int LoadedUsers { get; set; }
        public int Messages { get; set; }
        public Status()
        {
            this.Time = DateTimeOffset.UtcNow;
            this.ConnectedGuilds = Core.DiscordClient.Guilds.Count;
            this.LoadedUsers = Core.DiscordClient.Guilds.Sum(g => g.MemberCount);
            this.Messages = Core.Messages;
            Core.Messages = 0;
        }
    }
}
