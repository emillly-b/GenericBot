using Discord.WebSocket;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenericBot.Database
{
    public class DatabaseUser
    {
        [BsonId]
        public ulong Id { get; set; }
        public List<string> Usernames { get; set; }
        public List<string> Nicknames { get; set; }
        public List<string> Warnings { get; set; }
        public List<ulong> RoleStore { get; set; }
        public ulong Messages { get; set; }
        public bool IsPresent { get; set; }

        public DatabaseUser(ulong id)
        {
            this.Id = id;
            this.Usernames = new List<string>();
            this.Nicknames = new List<string>();
            this.Warnings = new List<string>();
            this.RoleStore = new List<ulong>();
            this.Messages = 0;
            this.IsPresent = true;
        }

        public DatabaseUser AddUsername(string username)
        {
            if (Usernames == null) Usernames = new List<string>();
            if (!Usernames.Contains(username))
            {
                Usernames.Add(username);
            }
            return this;
        }
        public DatabaseUser AddNickname(string username)
        {
            if (Nicknames == null) Nicknames = new List<string>();
            if (!Nicknames.Contains(username))
            {
                Nicknames.Add(username);
            }
            return this;
        }

        public DatabaseUser AddNickname(SocketGuildUser user)
        {
            if (user == null) return this;
            if (user.Nickname != null && !string.IsNullOrEmpty(user.Nickname) && !Nicknames.Contains(user.Nickname))
            {
                AddNickname(user.Nickname);
            }
            return this;
        }

        public DatabaseUser AddWarning(string warning)
        {
            if (Warnings == null) Warnings = new List<string>();
            Warnings.Add(warning);
            return this;
        }

        public DatabaseUser RemoveWarning(bool allWarnings = false)
        {
            if (Warnings.IsEmpty())
            {
                throw new DivideByZeroException("User has no warnings");
            }
            if (!allWarnings)
            {
                Warnings.RemoveAt(Warnings.Count - 1);
            }
            else
            {
                Warnings.RemoveRange(0, Warnings.Count);
            }
            return this;
        }

        public List<ulong> GetStoredRoles()
        {
            if (this.RoleStore == null)
                return new List<ulong>();
            else return this.RoleStore;
        }
        public void AddStoredRole(ulong roleId)
        {
            if (this.RoleStore == null)
                this.RoleStore = new List<ulong>();
            if (!this.RoleStore.Contains(roleId))
                this.RoleStore.Add(roleId);
        }
        public void RemoveStoredRole(ulong roleId)
        {
            if (this.RoleStore != null && this.RoleStore.Contains(roleId))
                this.RoleStore.Remove(roleId);
        }
    }
}
