using GenericBot.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenericBot.Database
{
    /// <summary>
    /// Implementation of <see cref="IDatabaseEngine"/> for MongoDB
    /// </summary>
    public class MongoEngine : IDatabaseEngine
    {
        private MongoClient mongoClient;

        public MongoEngine()
        {
            if (!Core.GlobalConfig.DbConnectionString.StartsWith("mongodb"))
            {
                throw new Exception("Connection string is not of type mongo");
            }
            mongoClient = new MongoClient(Core.GlobalConfig.DbConnectionString);
        }

        ///<inheritdoc cref="IDatabaseEngine.GetGuildConfig(ulong)"/>
        public GuildConfig GetGuildConfig(ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] GOT GuildConfig FROM {guildId}");
            var _configDb = GetDatabaseFromGuildId(guildId);
            var _collection = _configDb.GetCollection<GuildConfig>("config");

            if (_collection.Find(c => c.Id == guildId).Any())
                return _collection.Find(c => c.Id == guildId).First();
            else
                return new GuildConfig(guildId);
        }

        ///<inheritdoc cref="IDatabaseEngine.SaveGuildConfig(GuildConfig)"/>
        public GuildConfig SaveGuildConfig(GuildConfig guildConfig)
        {
            Core.Logger.LogGenericMessage($"[Mongo] SAVED GuildConfig TO {guildConfig.Id}");

            var _configDb = GetDatabaseFromGuildId(guildConfig.Id);
            var _collection = _configDb.GetCollection<GuildConfig>("config");
            if (_collection.Find(c => c.Id == guildConfig.Id).Any())
                _collection.ReplaceOne(c => c.Id == guildConfig.Id, guildConfig);
            else _collection.InsertOne(guildConfig);

            return guildConfig;
        }

        ///<inheritdoc cref="IDatabaseEngine.SaveCustomCommand(CustomCommand, ulong)"/>
        public CustomCommand SaveCustomCommand(CustomCommand command, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] SAVED CustomComand {command.Name} TO {guildId}");

            var _configDb = GetDatabaseFromGuildId(guildId);
            var _collection = _configDb.GetCollection<CustomCommand>("customCommands");
            if (_collection.Find(c => c.Name == command.Name).Any())
                _collection.ReplaceOne(c => c.Name == command.Name, command);
            else _collection.InsertOne(command);
            return command;
        }

        ///<inheritdoc cref="IDatabaseEngine.GetCustomCommands(ulong)"/>
        public List<CustomCommand> GetCustomCommands(ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] GOT CustomCommands FROM {guildId}");

            var _configDb = GetDatabaseFromGuildId(guildId);
            var _collection = _configDb.GetCollection<CustomCommand>("customCommands");
            var list = _collection.Find(new BsonDocument()).ToList();
            return list;
        }

        ///<inheritdoc cref="IDatabaseEngine.DeleteCustomCommand(string, ulong)"/>
        public void DeleteCustomCommand(string name, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] DELETE CustomCommand {name} FROM {guildId}");

            var _configDb = GetDatabaseFromGuildId(guildId);
            var _collection = _configDb.GetCollection<CustomCommand>("customCommands");

            if(_collection.Find(c => c.Name == name).Any())
                _collection.FindOneAndDelete(c => c.Name == name);
        }

        ///<inheritdoc cref="IDatabaseEngine.GetUserFromGuild(ulong, ulong, bool)"/>
        public DatabaseUser GetUserFromGuild(ulong userId, ulong guildId, bool log = true)
        {
            if(log)
                Core.Logger.LogGenericMessage($"[Mongo] GOT User {userId} FROM {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<DatabaseUser>("users");
            if (_collection.Find(u => u.Id == userId).Any())
                return _collection.Find(u => u.Id == userId).First();
            else return new DatabaseUser(userId);
        }

        ///<inheritdoc cref="IDatabaseEngine.SaveUserToGuild(DatabaseUser, ulong, bool)"/>
        public DatabaseUser SaveUserToGuild(DatabaseUser user, ulong guildId, bool log = true)
        {
            if (log)
                Core.Logger.LogGenericMessage($"[Mongo] SAVED User {user.Id} TO {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<DatabaseUser>("users");
            if (_collection.Find(u => u.Id == user.Id).Any())
                _collection.FindOneAndReplace(u => u.Id == user.Id, user);
            else _collection.InsertOne(user);
            return user;
        }

        ///<inheritdoc cref="IDatabaseEngine.GetAllUsers(ulong)"/>
        public List<DatabaseUser> GetAllUsers(ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] GOT AllUsers FROM {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<DatabaseUser>("users");

            return _collection.Find(new BsonDocument()).ToList();
        }

        ///<inheritdoc cref="IDatabaseEngine.SaveBanToGuild(GenericBan, ulong)"/>
        public GenericBan SaveBanToGuild(GenericBan ban, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] SAVED Ban {ban.Id} TO {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<GenericBan>("bans");
            if (_collection.Find(u => u.Id == ban.Id).Any())
                _collection.FindOneAndReplace(u => u.Id == ban.Id, ban);
            else _collection.InsertOne(ban);
            return ban;
        }

        ///<inheritdoc cref="IDatabaseEngine.GetBansFromGuild(ulong, bool)"/>
        public List<GenericBan> GetBansFromGuild(ulong guildId, bool log = true)
        {
            if (log)
                Core.Logger.LogGenericMessage($"[Mongo] GOT AllBans FROM {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<GenericBan>("bans");
            if (_collection.Find(new BsonDocument()).Any())
                return _collection.Find(new BsonDocument()).ToList();
            else return new List<GenericBan>();
        }

        ///<inheritdoc cref="IDatabaseEngine.RemoveBanFromGuild(ulong, ulong)"/>
        public void RemoveBanFromGuild(ulong banId, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] DELETE Ban {banId} FROM {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<GenericBan>("bans");
            if (_collection.Find(u => u.Id == banId).Any())
                _collection.DeleteOne(u => u.Id == banId);
        }

        ///<inheritdoc cref="IDatabaseEngine.AddToAuditLog(ParsedCommand, ulong)"/>
        public void AddToAuditLog(ParsedCommand command, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] SAVED AuditLog TO {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<AuditCommand>("auditlog");

            _collection.InsertOne(new AuditCommand(command));
        }

        ///<inheritdoc cref="IDatabaseEngine.GetAuditLog(ulong)"/>
        public List<AuditCommand> GetAuditLog(ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] GOT AuditLog FROM {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<AuditCommand>("auditlog");

            return _collection.Find(new BsonDocument()).ToList();
        }

        ///<inheritdoc cref="IDatabaseEngine.AddToCommandLog(ParsedCommand, ulong)"/>
        public void AddToCommandLog(ParsedCommand command, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] SAVED CommandLog TO {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<AuditCommand>("commandlog");

            _collection.InsertOne(new AuditCommand(command));
        }

        ///<inheritdoc cref="IDatabaseEngine.GetCommandLog(ulong)"/>
        public List<AuditCommand> GetCommandLog(ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] GOT CommandLog FROM {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<AuditCommand>("commandlog");

            return _collection.Find(new BsonDocument()).ToList();
        }

        ///<inheritdoc cref="IDatabaseEngine.CreateGiveaway(Giveaway, ulong)"/>
        public Giveaway CreateGiveaway(Giveaway giveaway, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] CREATE GIVEAWAY {giveaway.Id} FOR {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<Giveaway>("giveaways");

            while(_collection.Find(new BsonDocument()).ToList().HasElement(g => g.Id == giveaway.Id, out Giveaway output))
            {
                giveaway.Id += new Random().Next(0, 10).ToString();
            }

            _collection.InsertOne(giveaway);
            return giveaway;
        }

        ///<inheritdoc cref="IDatabaseEngine.UpdateOrCreateGiveaway(Giveaway, ulong)"/>
        public Giveaway UpdateOrCreateGiveaway(Giveaway giveaway, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] UPDATE GIVEAWAY {giveaway.Id} FOR {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<Giveaway>("giveaways");

            if (_collection.Find(new BsonDocument()).ToList().Any(g => g.Id == giveaway.Id))
            {
                _collection.FindOneAndReplace(g => g.Id == giveaway.Id, giveaway);
            }
            else _collection.InsertOne(giveaway);

            return giveaway;
        }

        ///<inheritdoc cref="IDatabaseEngine.GetGiveaways(ulong)"/>
        public List<Giveaway> GetGiveaways(ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] GOT Giveaways FROM {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<Giveaway>("giveaways");

            return _collection.Find(new BsonDocument()).ToList();
        }

        ///<inheritdoc cref="IDatabaseEngine.DeleteGiveaway(Giveaway, ulong)"/>
        public void DeleteGiveaway(Giveaway giveaway, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] GOT Giveaways FROM {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<Giveaway>("giveaways");

            _collection.FindOneAndDelete(g => g.Id == giveaway.Id);
        }

        ///<inheritdoc cref="IDatabaseEngine.AddStatus(Status)"/>
        public void AddStatus(Status status)
        {
            var _db = mongoClient.GetDatabase("global");
            var _collection = _db.GetCollection<Status>("statusLog");
            _collection.InsertOne(status);
        }

        ///<inheritdoc cref="IDatabaseEngine.AddOrUpdateExceptionReport(ExceptionReport)"/>
        public ExceptionReport AddOrUpdateExceptionReport(ExceptionReport report)
        {
            var _db = mongoClient.GetDatabase("global");
            var _collection = _db.GetCollection<ExceptionReport>("exceptionReports");
            ExceptionReport foundReport;
            if (_collection.Find(new BsonDocument()).ToList()
                .HasElement(r => r.Equals(report), out foundReport))
            {
                foundReport.Count++;
                if (report.Reported)
                    foundReport.Reported = true;
                _collection.FindOneAndReplace(r => r.Message.Equals(foundReport.Message) && r.StackTrace.Equals(foundReport.StackTrace), foundReport);
            }
            else
            {
                _collection.InsertOne(report);
                foundReport = report;
            }

            return foundReport;
        }

        ///<inheritdoc cref="IDatabaseEngine.AddVerification(ulong, ulong)"/>
        public void AddVerification(ulong userId, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] ADD VERIFICATION {userId} TO {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<VerificationEvent>("verifications");

            var _event = new VerificationEvent(guildId, userId);
            _collection.InsertOne(_event);
        }

        public List<string> GetGuildIdsFromDb()
        {
            return mongoClient.ListDatabaseNames().ToList();
        }
        private IMongoDatabase GetDatabaseFromGuildId(ulong GuildId)
        {
            return mongoClient.GetDatabase(GuildId.ToString());
        }
    }
}
