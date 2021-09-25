using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace SaturnBot.Entities
{
    public class ParsedCommand
    {
        public Command RawCommand;
        public string Name;
        public List<string> Parameters;
        public string ParameterString;
        public SocketMessage Message;

        private SocketUser _author = null;
        public SocketUser Author { get { return _author != null ? _author : Message.Author; } set { _author = value; } }
        public SocketGuild Guild { get
            {
                if (Message.Channel.GetType().ToString().Contains("DM")) return null;
                else return (Message.Channel as SocketGuildChannel).Guild;
            }
        }
        public ISocketMessageChannel Channel { get { return Message.Channel; } }

        public Task Execute()
        {
            return RawCommand.ExecuteCommand(this);
        }
    }
}
