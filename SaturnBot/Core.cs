using Discord;
using Discord.WebSocket;
using SaturnBot.CommandModules;
using SaturnBot.Database;
using SaturnBot.Entities;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SaturnBot.CommandModules.AirLock;

namespace SaturnBot
{
    /// <summary>
    /// The Core client, responsible for loading everything on startup and wrapping
    /// any methods that interact with the database or configuration
    /// </summary>
    public static class Core
    {
        /// <summary>
        /// The shared configuration for the entire bot
        /// </summary>
        public static GlobalConfiguration GlobalConfig { get; private set; }
        public static DiscordShardedClient DiscordClient { get; private set; }
        public static List<Command> Commands { get; set; }
        public static Dictionary<ulong, List<CustomCommand>> CustomCommands;
        public static int Messages { get; set; }
        public static Logger Logger { get; private set; }
        public static IDatabaseEngine DatabaseEngine { get; set; }

        public static AirLockCore Airlock { get; set; }

        private static List<GuildConfig> LoadedGuildConfigs;

        static Core()
        {
            // Load the global configuration
            GlobalConfig = new GlobalConfiguration().Load();
            // Initialize a new logger with the current data and time
            Logger = new Logger();
            // Intialize a new, empty list of commands and custom commands
            Commands = new List<Command>();
            // CustomCommands are structured as GuildID -> List<CustomCommand>
            CustomCommands = new Dictionary<ulong, List<CustomCommand>>();
            // Load custom commands from enabled modules
            LoadCommands(GlobalConfig.CommandsToExclude);
            // Create the database engine
            DatabaseEngine = new MongoEngine();
            LoadedGuildConfigs = new List<GuildConfig>();
            Messages = 0;           

            // Configure Client
            DiscordClient = new DiscordShardedClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Verbose,
                AlwaysDownloadUsers = true,
                MessageCacheSize = 100,
            });

            Airlock = new AirLockCore();
            Airlock.Load(DiscordClient);

            // Set up event handlers
            DiscordClient.Log += Logger.LogClientMessage;
            DiscordClient.MessageReceived += MessageEventHandler.MessageRecieved;
            DiscordClient.MessageUpdated += MessageEventHandler.HandleEditedCommand;
            //DiscordClient.MessageDeleted += MessageEventHandler.MessageDeleted;
            DiscordClient.UserJoined += UserEventHandler.UserJoined;
            DiscordClient.UserLeft += UserEventHandler.UserLeft;
            DiscordClient.UserUpdated += UserEventHandler.UserUpdated;
            DiscordClient.GuildAvailable += GuildEventHandler.GuildLoaded;
            DiscordClient.ShardReady += ShardReady;            
        }

        /// <summary>
        /// Set the playing message on each sharded instance once the bot reports it as Ready
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private static async Task ShardReady(DiscordSocketClient arg)
        {
            if (File.Exists("version.txt"))
            {
                await arg.SetGameAsync($"v. {File.ReadAllText("version.txt")}", type: ActivityType.Watching);
            }
            else
            {
                await arg.SetGameAsync(GlobalConfig.PlayingStatus);
            }
        }

        /// <summary>
        /// Load all enabled modules into the Commands list
        /// </summary>
        /// <param name="CommandsToExclude"></param>
        private static void LoadCommands(List<string> CommandsToExclude = null)
        {
            Commands.Clear();
            Commands.AddRange(new ConfigModule().Load());
            Commands.AddRange(new GetGuildModule().Load());
            Commands.AddRange(new InfoModule().Load());
            Commands.AddRange(new LookupModule().Load());
            Commands.AddRange(new QuickCommands().GetQuickCommands());
            //Commands.AddRange(new TestModule().Load());
            Commands.AddRange(new Commands().Load());

            if (CommandsToExclude == null)
                return;
            Commands = Commands.Where(c => !CommandsToExclude.Contains(c.Name)).ToList();
        }

        public static void DropCaches()
        {
            CustomCommands = new Dictionary<ulong, List<CustomCommand>>();
            LoadedGuildConfigs = new List<GuildConfig>();
        }

        /// <summary>
        /// Check if a user is blacklisted from running bot commands globally
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public static bool CheckBlacklisted(ulong UserId) => 
            GlobalConfig.BlacklistedIds != null && GlobalConfig.BlacklistedIds.Contains(UserId);
        /// <summary>
        /// Return the UserID of the bot
        /// </summary>
        /// <returns></returns>
        public static ulong GetCurrentUserId() => DiscordClient.CurrentUser.Id;
        /// <summary>
        /// Return the UserID of the owner of the bot account
        /// </summary>
        /// <returns></returns>
        //public static ulong GetOwnerId() => DiscordClient.GetApplicationInfoAsync().Result.Owner.Id;
        public static ulong GetOwnerId() => 341275030941859850;
        public static string GetGlobalPrefix() => GlobalConfig.DefaultPrefix;
        /// <summary>
        /// Return the appropriate prefix for a command, based on where the comand was run
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetPrefix(ParsedCommand context)
        {
            // TODO: This if check seems weird, the second statement looks very wrong
            if (!(context.Message.Channel is SocketDMChannel) 
                && context.Guild != null 
                && !string.IsNullOrEmpty(GetGuildConfig(context.Guild.Id).Prefix))
                return GetGuildConfig(context.Guild.Id).Prefix;
            return GetGlobalPrefix();
        }
        public static bool CheckGlobalAdmin(ulong UserId) => GlobalConfig.GlobalAdminIds.Contains(UserId);
        // TODO: Fix name, whoops
        public static SocketGuild GetGuid(ulong GuildId) => DiscordClient.GetGuild(GuildId);

        // TODO: Caching for the next two methods (GetGuildConfig and SaveGuildConfig) looks inconsistent and may cause issues
        /// <summary>
        /// Get the config for a guild by ID
        /// </summary>
        /// <param name="GuildId"></param>
        /// <returns></returns>
        public static GuildConfig GetGuildConfig(ulong GuildId)
        {
            if (LoadedGuildConfigs.Any(c => c.Id == GuildId))
            {
                return LoadedGuildConfigs.Find(c => c.Id == GuildId);
            }
            else
            {
                LoadedGuildConfigs.Add(DatabaseEngine.GetGuildConfig(GuildId));
            }
            return DatabaseEngine.GetGuildConfig(GuildId);
        }
        /// <summary>
        /// Write a GuildConfig to the database
        /// </summary>
        /// <param name="guildConfig"></param>
        /// <returns></returns>
        public static GuildConfig SaveGuildConfig(GuildConfig guildConfig)
        {
            //if (LoadedGuildConfigs.Any(c => c.Id == guildConfig.Id))
            //    LoadedGuildConfigs.RemoveAll(c => c.Id == guildConfig.Id);
            //LoadedGuildConfigs.Add(guildConfig);

            return DatabaseEngine.SaveGuildConfig(guildConfig);
        }

        /// <summary>
        /// Retrieve a list of custom commands for a guild
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public static List<CustomCommand> GetCustomCommands(ulong guildId)
        {
            if (CustomCommands.ContainsKey(guildId))
                return CustomCommands[guildId];
            else
            {
                var cmds = DatabaseEngine.GetCustomCommands(guildId);
                CustomCommands.Add(guildId, cmds);
                return cmds;
            }
        }

        // TODO: Ensure database/cache consistency for custom commands
        /// <summary>
        /// Add or overrwite a custom command to the cache and database
        /// </summary>
        /// <param name="command"></param>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public static CustomCommand SaveCustomCommand(CustomCommand command, ulong guildId)
        {
            if (CustomCommands.ContainsKey(guildId))
            {
                if (CustomCommands[guildId].Any(c => c.Name == command.Name))
                {
                    CustomCommands[guildId].RemoveAll(c => c.Name == command.Name);
                }
                CustomCommands[guildId].Add(command);
            }
            else
            {
                CustomCommands.Add(guildId, new List<CustomCommand> { command });
            }
            DatabaseEngine.SaveCustomCommand(command, guildId);
            return command;
        }

        /// <summary>
        /// Delete a custom command from the database by name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="guildId"></param>
        public static void DeleteCustomCommand(string name, ulong guildId)
        {
            if (CustomCommands.ContainsKey(guildId))
            {
                CustomCommands[guildId].RemoveAll(c => c.Name == name);
            }
            DatabaseEngine.DeleteCustomCommand(name, guildId);
        }

        public static DatabaseUser SaveUserToGuild(DatabaseUser user, ulong guildId, bool log = true) =>
            DatabaseEngine.SaveUserToGuild(user, guildId, log);
        public static DatabaseUser GetUserFromGuild(ulong userId, ulong guildId, bool log = true) =>
            DatabaseEngine.GetUserFromGuild(userId, guildId, log);
        public static List<DatabaseUser> GetAllUsers(ulong guildId) =>
            DatabaseEngine.GetAllUsers(guildId);

        public static GenericBan SaveBanToGuild(GenericBan ban, ulong guildId) =>
            DatabaseEngine.SaveBanToGuild(ban, guildId);
        public static List<GenericBan> GetBansFromGuild(ulong guildId, bool log = true) =>
            DatabaseEngine.GetBansFromGuild(guildId, log);
        public static void RemoveBanFromGuild(ulong banId, ulong guildId) =>
            DatabaseEngine.RemoveBanFromGuild(banId, guildId);

        public static void AddToAuditLog(ParsedCommand command, ulong guildId) =>
            DatabaseEngine.AddToAuditLog(command, guildId);
        public static List<AuditCommand> GetAuditLog(ulong guildId) =>
            DatabaseEngine.GetAuditLog(guildId);

        public static void AddToCommandLog(ParsedCommand command, ulong guildId) =>
            DatabaseEngine.AddToCommandLog(command, guildId);
        public static List<AuditCommand> GetCommandLog(ulong guildId) =>
            DatabaseEngine.GetCommandLog(guildId);

        public static void AddStatus(Status status) =>
            DatabaseEngine.AddStatus(status);

        public static Giveaway CreateGiveaway(Giveaway giveaway, ulong guildId) =>
            DatabaseEngine.CreateGiveaway(giveaway, guildId);

        public static Giveaway UpdateOrCreateGiveaway(Giveaway giveaway, ulong guildId) =>
            DatabaseEngine.UpdateOrCreateGiveaway(giveaway, guildId);

        public static List<Giveaway> GetGiveaways(ulong guildId) =>
            DatabaseEngine.GetGiveaways(guildId);

        public static void DeleteGiveaway(Giveaway giveaway, ulong guildId) =>
            DatabaseEngine.DeleteGiveaway(giveaway, guildId);
        
        // TODO: Figure out what to do with this
        /// <summary>
        /// Create a VerificationEvent and add it to the database
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="guildId"></param>
        public static void AddVerificationEvent(ulong userId, ulong guildId) =>
            DatabaseEngine.AddVerification(userId, guildId);

        /// <summary>
        /// Add or update an ExceptionReport to the database, and open an issue on GitHub if possible
        /// </summary>
        /// <param name="report"></param>
        /// <returns></returns>
        public static ExceptionReport AddOrUpdateExceptionReport(ExceptionReport report)
        {
            report = DatabaseEngine.AddOrUpdateExceptionReport(report);
            if(!report.Reported && report.Count > 5)
            {
                Logger.LogGenericWarningMessage("Failed to report error to webhook or github!");
            }
            if (!report.Reported && report.Count >= GlobalConfig.GitHubIssueCount && !string.IsNullOrEmpty(GlobalConfig.GithubToken))
            {
                try
                {
                    string trace = report.StackTrace;
                    string message = report.Message;
                    if (Core.GlobalConfig.GitHubFilteredWords.Count > 0)
                    {
                        foreach(string BannedWord in Core.GlobalConfig.GitHubFilteredWords)
                        {
                            trace = trace.Replace(BannedWord, "*removed*");
                            message = message.Replace(BannedWord, "*removed*");
                        }
                    }
                    var githubTokenAuth = new Credentials(GlobalConfig.GithubToken);
                    var client = new GitHubClient(new ProductHeaderValue("SaturnBot"));
                    client.Credentials = githubTokenAuth;
                    var issueToCreate = new NewIssue($"AUTOMATED: {message}");
                    issueToCreate.Body = $"Stacktrace:\n" +
                        $"{trace}\n";
                    issueToCreate.Labels.Add("bug");
                    var issue = client.Issue.Create(client.User.Current().Result.Login, "SaturnBot", issueToCreate).Result;
                    report.Reported = true;
                    report = DatabaseEngine.AddOrUpdateExceptionReport(report);
                }
                catch (Exception ex)
                {
                    Logger.LogGenericMessage("An error occured reporting to github.");
                    Logger.LogGenericMessage(ex.Message + "\n" + ex.StackTrace);
                }
            }

            return report;
        }
    }
}
