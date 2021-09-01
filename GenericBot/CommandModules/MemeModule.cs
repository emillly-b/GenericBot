using GenericBot.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace GenericBot.CommandModules
{
    class MemeModule : Module
    {
        public List<Command> Load()
        {
            List<Command> commands = new List<Command>();
            
            #region "UWU Command"
            Command uwu = new Command("uwu");
            uwu.WorksInDms = true;
            uwu.Description = "Uwu-ify text";
            uwu.Usage = "uwu <text>";
            uwu.Delete = true;
            uwu.ToExecute += async (context) =>
            {
                string uwuified = context.ParameterString;
                if (context.Parameters.Count == 0)
                    uwuified = "You need to give me a message to uwu-ify!";
                uwuified = new Regex("(?:r|l)").Replace(uwuified, "w");
                uwuified = new Regex("(?:R|L)").Replace(uwuified, "W");
                uwuified = new Regex("n([aeiou])").Replace(uwuified, "ny$1");
                uwuified = new Regex("N([aeiou])").Replace(uwuified, "Ny$1");
                uwuified = new Regex("N([AEIOU])").Replace(uwuified, "Ny$1");
                uwuified = new Regex("ove").Replace(uwuified, "uv");
                uwuified = new Regex("th").Replace(uwuified, "d");
                uwuified = new Regex("Th").Replace(uwuified, "D");

                await context.Message.ReplyAsync(uwuified);
            };
            commands.Add(uwu);
            #endregion

            #region "Testing Command"
            
            Command TestCommand = new Command("test");
            TestCommand.Description = "Does stuff i guess?";
            TestCommand.Usage = "test";
            TestCommand.Delete = true;
            TestCommand.ToExecute += async (context) =>
            {
                await context.Message.ReplyAsync("This is a test, Eli is gay.");
            };
            commands.Add(TestCommand);
            #endregion

            return commands;
        }
    }
}
