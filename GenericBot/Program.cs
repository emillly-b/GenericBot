using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Discord.WebSocket;
using GenericBot.Entities;

namespace GenericBot
{
    public class Program
    {
        public static string BuildId = string.Empty;
        public static List<ulong> ClearedMessageIds = new List<ulong>();

        static void Main(string[] args)
        {
            if (File.Exists("version.txt"))
            {
                Core.Logger.LogGenericMessage($"Build {File.ReadAllText("version.txt").Trim()}");
                BuildId = File.ReadAllText("version.txt").Trim();
            }

            Timer cycleTimer = new Timer();
            cycleTimer.Interval = 60 * 1000;
            cycleTimer.AutoReset = true;
            cycleTimer.Elapsed += ExecuteCycle;
            cycleTimer.Start();

            Start(args).GetAwaiter().GetResult();
        }

        private static async Task Start(string[] args)
        {
            try
            {
                await Core.DiscordClient.LoginAsync(TokenType.Bot, Core.GlobalConfig.DiscordToken);
                await Core.DiscordClient.StartAsync();
            }
            catch (Exception e)
            {
                await Core.Logger.LogGenericMessage(e.ToString());
                return;
            }

            // Block until exited
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                    .UseUrls("http://localhost:6969");
                });

        private static void ExecuteCycle(object sender, ElapsedEventArgs e)
        {
            //var status = new Status();
            //Core.AddStatus(status);

            // Check for unbans
            foreach (var gid in Core.DiscordClient.Guilds.Select(g => g.Id))
            {
                var bans = Core.GetBansFromGuild(gid, false);
                foreach (var ban in bans.Where(b => b.BannedUntil < DateTime.UtcNow))
                {
                    try
                    {
                        var user = Core.DiscordClient.GetGuild(ban.GuildId).GetBansAsync().Result
                        .First(b => b.User.Id == ban.Id).User;
                        Core.DiscordClient.GetGuild(gid).RemoveBanAsync(ban.Id);

                        var builder = new EmbedBuilder()
                            .WithTitle("User Unbanned")
                            .WithDescription($"Banned for: {ban.Reason}")
                            .WithColor(new Color(0xFFFF00))
                            .WithFooter(footer => {
                                footer
                                    .WithText($"{DateTime.UtcNow.ToString(@"yyyy-MM-dd HH:mm tt")} GMT");
                            })
                            .WithAuthor(author => {
                                author
                                    .WithName(user.ToString())
                                    .WithIconUrl(user.GetAvatarUrl());
                            })
                            .AddField(new EmbedFieldBuilder().WithName("All Warnings").WithValue(
                                Core.GetUserFromGuild(ban.Id, gid).Warnings.SumAnd()));
                        ((SocketTextChannel)Core.DiscordClient.GetChannel(Core.GetGuildConfig(gid).LoggingChannelId))
                            .SendMessageAsync("", embed: builder.Build());
                    }
                    catch (Exception ex)
                    {
                        Core.Logger.LogErrorMessage(ex, null);
                    }
                    try
                    {
                        Core.RemoveBanFromGuild(ban.Id, gid);
                    }
                    catch (Exception ex)
                    {
                        Core.Logger.LogErrorMessage(ex, null);
                    }
                }
            }
        }
    }
}
