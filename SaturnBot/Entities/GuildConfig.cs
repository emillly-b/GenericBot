using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace SaturnBot.Entities
{
    public class GuildConfig
    {
        [Flags]
        public enum AntiSpamLevel
        {
            None = 1,
            Basic = 2, 
            Advanced = 4, /* Username filtering */
            Aggressive = 8,
            ActiveRaid = 16
        }
        [BsonId]
        public ulong Id { get; set; }
        public string Prefix { get; set; }
        public List<ulong> AdminRoleIds { get; set; }
        public List<ulong> ModRoleIds { get; set; }
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<string, List<ulong>> UserRoles { get; set; }
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<ulong, List<ulong>> RequiresRoles { get; set; }
        public ulong MutedRoleId { get; set; }
        public List<ulong> MutedUsers { get; set; }
        public List<ulong> AutoRoleIds { get; set; }
        public ulong LoggingChannelId { get; set; }
        public List<ulong> MessageLoggingIgnoreChannels { get; set; }
        public ulong VerifiedRole { get; set; }
        public string VerifiedMessage { get; set; }
        public string JoinMessage { get; set; }
        public ulong JoinMessageChannelId { get; set; }
        public AntiSpamLevel AntispamLevel { get; set; }
        public ulong TrustedRoleId { get; set; }

        public GuildConfig(ulong id)
        {
            Id = id;
            VerifiedRole = 0;
            AntispamLevel = AntiSpamLevel.None;
            MessageLoggingIgnoreChannels = new List<ulong>();
            MutedUsers = new List<ulong>();
            AdminRoleIds = new List<ulong>();
            ModRoleIds = new List<ulong>();
            UserRoles = new Dictionary<string, List<ulong>>();
            AutoRoleIds = new List<ulong>();
            TrustedRoleId = 0;
        }
    }

    public class GenericBan
    {
        public ulong Id;
        public ulong GuildId;
        public DateTimeOffset BannedUntil;
        public string Reason;

        public GenericBan(ulong userid, ulong guildid, string reason, DateTimeOffset time)
        {
            this.Id = userid;
            this.GuildId = guildid;
            this.BannedUntil = time;
            this.Reason = reason;
        }
    }
}
