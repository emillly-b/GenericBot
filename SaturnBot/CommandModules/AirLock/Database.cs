using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace SaturnBot.CommandModules.AirLock
{
    public class Database
    {
        public static readonly string dbPath = "./Data/Airlock/";
        public static bool Exists => CheckForDB();

        public static JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };

        public static void CreateDatabase()
        {
            Directory.CreateDirectory("./Data");
            Directory.CreateDirectory("./Data/Airlock");
            Directory.CreateDirectory("./Data/Airlock/Guilds");
            File.WriteAllText(dbPath + "Configuration.json", JsonSerializer.Serialize(GetConfiguration(), options));
        }

        public static bool CheckForDB()
        {
            try
            {
                Directory.GetFiles("./Data");
                Directory.GetFiles("./Data/Airlock");
                Directory.GetFiles("./Data/Airlock/Guilds");
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static Configuration GetConfiguration()
        {
            string json = GetFile(dbPath + "Configuration.json");
            Configuration config = JsonSerializer.Deserialize<Configuration>(json);
            return config;
        }
        public static void SaveConfiguration(Configuration currentConfig)
        {
            string _configJson = JsonSerializer.Serialize(currentConfig, options);
            File.WriteAllText(dbPath + "Configuration.json", _configJson);
        }

        public static List<Guild> GetGuilds()
        {
            string guildDirectory = dbPath + "/Guilds/";
            List<Guild> guilds = new List<Guild>();
            string[] _guildFiles = Directory.GetFiles(guildDirectory, "*.json");
            foreach(string _guildPath in _guildFiles)
            {                
                guilds.Add(JsonSerializer.Deserialize<Guild>(File.ReadAllText(_guildPath)));
            }
            return guilds;
        }

        public static void SaveGuilds(List<Guild> guilds)
        {
            foreach (Guild guild in guilds)
            {
                string _guildJson = JsonSerializer.Serialize(guild, options);
                File.WriteAllText(dbPath + "/Guilds/" + guild.Id + ".json", _guildJson);
            }
        }

        private static string GetFile(string path)
        {
            return File.ReadAllText(path);
        }
    }
}
