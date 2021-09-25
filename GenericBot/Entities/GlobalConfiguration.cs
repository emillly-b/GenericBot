using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace SaturnBot.Entities
{
    /// <summary>
    /// Configuration settings for the entire bot
    /// </summary>
    public class GlobalConfiguration
    {       
        /// <summary>
        /// Discord API Token
        /// </summary>
        public string DiscordToken { get; set; }
        /// <summary>
        /// String used to connect to MongoDB
        /// </summary>
        public string DbConnectionString { get; set; }
        /// <summary>
        /// Command prefix to use if not set for the server
        /// </summary>
        public string DefaultPrefix { get; set; }
        /// <summary>
        /// List of UserIds who have global admin permissions
        /// </summary>
        public List<ulong> GlobalAdminIds { get; set; }
        /// <summary>
        /// Default string to display as Playing on startup
        /// </summary>
        public string PlayingStatus { get; set; }
        /// <summary>
        /// List of naughty people who aren't allowed to use the bot
        /// </summary>
        public List<ulong> BlacklistedIds { get; set; }
        /// <summary>
        /// Commands to remove from the list on startup
        /// </summary>
        public List<string> CommandsToExclude { get; set; }

        /// <summary>
        /// Webhook URL to send critical messages to
        /// </summary>
        public string CriticalLoggingWebhookUrl { get; set; }
        /// <summary>
        /// Channel ID for sending DM logs to
        /// </summary>
        public ulong CriticalLoggingChannel { get; set; }
        /// <summary>
        /// Token to use to authenticate with GitHub for opening issues
        /// </summary>
        public string GithubToken { get; set; }

        // OAuth secrets for use with web API
        public string OAuthClientId { get; set; }
        public string OAuthClientSecret { get; set; }
        public string CallbackUri { get; set; }

        //Amount of times an exception needs to be thrown for a github issue to be started.
        public int GitHubIssueCount { get; set; }
        
        //Toggle to ensure commands are only processed by the owner
        public bool OwnerOnly {get; set; }

        //Toggle for Bot Owner stack trace responses in channel.
        public bool PrintStackTrace { get; set; }

        //Will remove the list from any stack traces etc.
        //full names, full file paths etc.
        public List<string> GitHubFilteredWords { get; set; }

        /// <summary>
        /// Load the config file if it exists, otherwise create a blank one and save itwein
        /// for the user to edit
        /// </summary>
        /// <param name="filePath">Optionally set a location for the config</param>
        /// <returns></returns>
        public GlobalConfiguration Load(string filePath = "./files/realconfig.json")
        {
            if (!File.Exists(filePath))
            {
                //Populate lists so data structure is deserialized appropriately. 
                var config = new GlobalConfiguration();
                config.BlacklistedIds = new List<ulong>();
                config.BlacklistedIds.Add(0);
                config.GlobalAdminIds = new List<ulong>();
                config.GlobalAdminIds.Add(0);
                config.GitHubFilteredWords = new List<string>();
                config.GitHubFilteredWords.Add("");
                Directory.CreateDirectory(filePath.Substring(0, filePath.LastIndexOf('/')));
                File.WriteAllText(filePath, JsonConvert.SerializeObject(config, Formatting.Indented));
                throw new FileNotFoundException($"Could not find a config file at {filePath}, so one has been created. Please edit it and rerun the bot");
            }
            else
            {
                var config = JsonConvert.DeserializeObject<GlobalConfiguration>(File.ReadAllText(filePath));
                if(string.IsNullOrEmpty(config.DiscordToken) 
                    || string.IsNullOrEmpty(config.DbConnectionString)
                    || string.IsNullOrEmpty(config.DefaultPrefix))
                {
                    throw new Exception($"Config file at {filePath} has required values not set. Please edit them and rerun the bot");
                }
                else
                {
                    return config;
                }
            }
        }
        
        public GlobalConfiguration Save(string filePath = "./files/realconfig.json")
        {
            File.WriteAllText(filePath, JsonConvert.SerializeObject(this, Formatting.Indented));
            return this;
        }
    }
}
