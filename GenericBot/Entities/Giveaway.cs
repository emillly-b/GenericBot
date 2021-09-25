using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SaturnBot.Entities
{
    [BsonIgnoreExtraElements]
    public class Giveaway
    {
        public ulong OwnerId { get; set; }
        public string Description { get; set; }
        public string Id { get; set; }
        public List<ulong> EnteredUsers { get; set; }
        public bool IsActive { get; set; }

        public Giveaway()
        {

        }

        public Giveaway(ParsedCommand context, string description)
        {
            this.OwnerId = context.Author.Id;
            this.Description = description;
            this.Id = VerificationEngine.GetVerificationCode(context.Message.Id, context.Author.Id);
            this.EnteredUsers = new List<ulong>();
            this.IsActive = true;
        }
    }
}
