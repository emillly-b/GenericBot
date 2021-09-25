using System;
using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;

namespace SaturnBot
{
    public static class VerificationEngine
    {
        public static string GetVerificationCode(ulong userId, ulong guildId)
        {
            var pid = int.Parse(userId.ToString().Substring(7, 6));
            var gid = int.Parse(guildId.ToString().Substring(7, 6));

            return HexToString((gid + pid).ToString("X").ToLower());
        }

        public static SocketGuild GetGuildFromCode(string code, ulong userId)
        {
            try
            {
                var pid = int.Parse(userId.ToString().Substring(7, 6));
                var sum = Convert.ToInt32(StringToHex(code.ToLower()), 16);
                var gid = sum - pid;

                if (Core.DiscordClient.Guilds.HasElement(g => g.Id.ToString().Contains(gid.ToString()),
                    out SocketGuild guild))
                {
                    Core.AddVerificationEvent(userId, guild.Id);
                    return guild;
                }
                return null;
            }
            catch
            {
                // This log throws way too much. It's fine to ignore errors, 
                // because it means there was a string parsing error. There's no 
                // better way to detect them easily
                //Core.Logger.LogErrorMessage(ex, null);
                return null;
            }
        }

        public static string InsertCodeInMessage(string message, string code)
        {
            int wc = message.Length;

            int sPos = new Random().Next((wc / 2), wc);
            for (int i = sPos; i < wc; i++)
            {
                if (message[i].Equals(' '))
                    break;
                sPos++;
            }

            message = message.Substring(0, sPos) + $" *(the secret is: {code})* " + message.Substring(sPos);

            return message;
        }

        private static string HexToString(string str)
        {
            str = str.Replace('0', 'h');
            str = str.Replace('1', 'j');
            str = str.Replace('2', 'k');
            str = str.Replace('3', 'p');
            str = str.Replace('4', 'r');
            str = str.Replace('5', 's');
            str = str.Replace('6', 'g');
            str = str.Replace('7', 'x');
            str = str.Replace('8', 'z');
            str = str.Replace('9', 'v');
            return str;
        }

        private static string StringToHex(string str)
        {
            str = str.Replace('h', '0');
            str = str.Replace('j', '1');
            str = str.Replace('k', '2');
            str = str.Replace('p', '3');
            str = str.Replace('r', '4');
            str = str.Replace('s', '5');
            str = str.Replace('g', '6');
            str = str.Replace('x', '7');
            str = str.Replace('z', '8');
            str = str.Replace('v', '9');
            return str;
        }

        public static string ConstructWelcomeMessage(string message, IUser user)
        {
            message = Regex.Replace(message, "{{username}}", user.Username, RegexOptions.IgnoreCase);
            message = Regex.Replace(message, "{{usermention}}", user.Mention, RegexOptions.IgnoreCase);
            message = Regex.Replace(message, "{{user}}", user.Mention, RegexOptions.IgnoreCase);
            return message;
        }
    }

    public class VerificationEvent
    {
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public DateTimeOffset DateTime { get; set; }
        public bool IsRevoked { get; set; }

        public VerificationEvent(ulong guildId, ulong userId)
        {
            this.GuildId = guildId;
            this.UserId = userId;
            this.DateTime = DateTimeOffset.UtcNow;
            this.IsRevoked = false;
        }
        public VerificationEvent()
        {

        }
    }
}
