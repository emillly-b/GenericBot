using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaturnBot.Entities
{
    public class GuildStats
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string OwnerName { get; set; }
        public string OwnerId { get; set; }
        public int TotalUserCount { get; set; }
        public int BotCount { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public int RoleCount { get; set; }
        public ulong ExpiresAt { get; set; }
    }
}
