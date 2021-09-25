using SaturnBot.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaturnBot.Database
{
    // TODO: Come up with a more consistent naming scheme
    /// <summary>
    /// Describes all the methods the database engine should provide. The implementation of the methods is
    /// up to each individual engine. Failure to provide a valid (but not necesarrily correct) object may 
    /// result in unintended behavior or lack of functionality. It is up to the author of the engine to implement
    /// all methods correctly and consistently. All database access should be through the static Core class
    /// to allow for caching and any other sanity checks.
    /// </summary>
    public interface IDatabaseEngine
    {
        /// <summary>
        /// Return the <see cref="GuildConfig"/> for a guild as specified by the <paramref name="guildId"/>, 
        /// or return a new <see cref="GuildConfig"/> if none is found
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public GuildConfig GetGuildConfig(ulong guildId);

        /// <summary>
        /// Write <paramref name="guildConfig"/> to a guild's database and return it. The database to write to should be determined by the Id field on the <see cref="GuildConfig"/>
        /// </summary>
        /// <param name="guildConfig"></param>
        /// <returns></returns>
        public GuildConfig SaveGuildConfig(GuildConfig guildConfig);


        /// <summary>
        /// Add or update a <see cref="CustomCommand"/> to a guild's database, using the Name field to deduplicate
        /// </summary>
        /// <param name="command"></param>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public CustomCommand SaveCustomCommand(CustomCommand command, ulong guildId);

        /// <summary>
        /// Return all <see cref="CustomCommand"/>s from a guild's database
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public List<CustomCommand> GetCustomCommands(ulong guildId);

        /// <summary>
        /// Delete all <see cref="CustomCommand"/>s matching a <paramref name="name"/> from a guild's database
        /// </summary>
        /// <param name="name"></param>
        /// <param name="guildId"></param>
        public void DeleteCustomCommand(string name, ulong guildId);

        /// <summary>
        /// Retrieve a <see cref="DatabaseUser"/> by <paramref name="userId"/> and <paramref name="guildId"/>
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="guildId"></param>
        /// <param name="log">Whether or not to log the request</param>
        /// <returns></returns>
        public DatabaseUser GetUserFromGuild(ulong userId, ulong guildId, bool log = true);

        /// <summary>
        /// Add or update a <see cref="DatabaseUser"/> to a guild's database, using the Id field to deduplicate
        /// </summary>
        /// <param name="user"></param>
        /// <param name="guildId"></param>
        /// <param name="log">Whether or not to log the request</param>
        /// <returns></returns>
        public DatabaseUser SaveUserToGuild(DatabaseUser user, ulong guildId, bool log = true);

        /// <summary>
        /// Retreive all <see cref="DatabaseUsers"/>s from a guild's database
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public List<DatabaseUser> GetAllUsers(ulong guildId);

        /// <summary>
        /// Add or update <see cref="GenericBan"/> to a guild's database, using the Id field for deduplication
        /// </summary>
        /// <param name="ban"></param>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public GenericBan SaveBanToGuild(GenericBan ban, ulong guildId);

        /// <summary>
        /// Retrieve all <see cref="GenericBan"/>s from a guild's database
        /// </summary>
        /// <param name="guildId"></param>
        /// <param name="log">Whether or not to log the request</param>
        /// <returns></returns>
        public List<GenericBan> GetBansFromGuild(ulong guildId, bool log = true);

        /// <summary>
        /// Delete a <see cref="GenericBan"/> from a guild's database 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="guildId"></param>
        public void RemoveBanFromGuild(ulong userId, ulong guildId);

        /// <summary>
        /// Add a <see cref="Quote"/> to a guild's database, without any deduplication
        /// </summary>
        /// <param name="quote"></param>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public void AddToAuditLog(ParsedCommand command, ulong guildId);

        /// <summary>
        /// Retrieve all <see cref="AuditCommand"/>s from a guild's database
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public List<AuditCommand> GetAuditLog(ulong guildId);

        // TODO: use AuditCommand instead of ParsedCommand 
        /// <summary>
        /// Add a <see cref="ParsedCommand"/> to a guild's database, without any deduplication
        /// </summary>
        /// <param name="command"></param>
        /// <param name="guildId"></param>
        public void AddToCommandLog(ParsedCommand command, ulong guildId);

        /// <summary>
        /// Retrieve all <see cref="AuditCommand"/>s from a guild's database
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public List<AuditCommand> GetCommandLog(ulong guildId);

        // TODO: Make this take a VerificationEvent instead of a UserId
        /// <summary>
        /// Add a <see cref="VerificationEvent"/> to a guild's database, without any deduplication
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="guildId"></param>
        public void AddVerification(ulong userId, ulong guildId);

        // TODO: Check responsibilities for the next two methods, as well as simplify them
        /// <summary>
        /// Add a <see cref="Giveaway"/> to a guild's database, using the Id field to deduplicate
        /// </summary>
        /// <param name="giveaway"></param>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public Giveaway CreateGiveaway(Giveaway giveaway, ulong guildId);

        /// <summary>
        /// Add or update a <see cref="Giveaway"/> to a guild's database, using the Id field to deduplicate
        /// </summary>
        /// <param name="giveaway"></param>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public Giveaway UpdateOrCreateGiveaway(Giveaway giveaway, ulong guildId);

        /// <summary>
        /// Retrieve all <see cref="Giveaway"/>s from a guild's database
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public List<Giveaway> GetGiveaways(ulong guildId);

        /// <summary>
        /// Remove a <see cref="Giveaway"/> from a guild's database using the <paramref name="giveaway"/>'s Id field
        /// </summary>
        /// <param name="giveaway"></param>
        /// <param name="guildId"></param>
        public void DeleteGiveaway(Giveaway giveaway, ulong guildId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="status"></param>
        public void AddStatus(Status status);

        /// <summary>
        /// Add or update an <see cref="ExceptionReport"/> to the global database, using the Id field to deduplicate. 
        /// If a matching <see cref="ExceptionReport"/> exists, increment the Seen counter and return the <see cref="ExceptionReport"/>
        /// from the database
        /// </summary>
        /// <param name="report"></param>
        /// <returns>The reported exception with any updated metadata</returns>
        public ExceptionReport AddOrUpdateExceptionReport(ExceptionReport report);
    }
}
