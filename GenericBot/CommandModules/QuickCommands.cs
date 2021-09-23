using System.Collections.Generic;
using GenericBot.Entities;

namespace GenericBot.CommandModules
{
    public class QuickCommands
    {
        protected class QuickCommand
        {
            private string _name;
            private string _returnValue;
            private string _description;
            public QuickCommand(string name, string returnValue, string description = "")
            {
                this._name = name;
                this._returnValue = returnValue;
                this._description = string.IsNullOrEmpty(description) ? null : description;
            }

            public Command GetCommand()
            {
                Command cmd = new Command(_name);
                cmd.SendTyping = false;
                cmd.WorksInDms = false;
                cmd.Description = _description;
                cmd.ToExecute += async (context) =>
                {
                    await context.Message.ReplyAsync(_returnValue);
                };
                return cmd;
            }
        }
        public List<Command> GetQuickCommands()
        {
            List<Command> quickCommands = new List<Command>();

            //quickCommands.Add(new QuickCommand("justask", " If you have a question, don't ask if you can ask it. Just ask it, and someone will be along to help you as soon as they can!", "Just ask copypaste").GetCommand());
            quickCommands.Add(new QuickCommand("github", "https://github.com/emillly-b/Saturn-bot", "Link the bot's github repo").GetCommand());

            return quickCommands;
        }
    }
}
