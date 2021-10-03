using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;

namespace SaturnBot.CommandModules.AirLock
{
    public class Utility
    {
        public static Embed GetIntroEmbed(User user, Discord.IUser discordUser)
        {
            var builder = new EmbedBuilder()
                .WithAuthor(discordUser)
                .WithColor(Color.Red)
                .WithDescription(discordUser.Mention);
            builder.AddField("Intro: ", user.IntoContent);
            return builder.Build();
        }
    }
}
