using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord;
using SaturnBot.Entities;

namespace SaturnBot
{
    public class Logger
    {
        public readonly string SessionId;
        public readonly string BasePath = @"./Data/Logs/";

        public Logger()
        {
            SessionId = $"{DateTime.UtcNow.ToString(@"yyyy-MM-dd_HH-mm")}_{new Random().Next(1000, 9999)}";
            Directory.CreateDirectory(BasePath);
            LogGenericMessage($"New Logger created with SessionID of {SessionId}");
        }

        public Task LogClientMessage(LogMessage msg)
        {
            string message = $"[{msg.Severity}] {DateTime.UtcNow.ToString(@"yyyy-MM-dd_HH-mm")}: {msg.Message}";
            if (msg.Severity != LogSeverity.Debug)
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
            }
            if ((msg.Severity == LogSeverity.Warning || msg.Severity == LogSeverity.Error) && msg.Exception != null)
            {
                message += "\n" + msg.Exception.Message;
                message += "\n" + msg.Exception.StackTrace;
            }
            Console.WriteLine(message);
            File.AppendAllText($"{BasePath}{SessionId}.log", message + "\n");
            if (message.Contains("Server missed last heartbeat"))
                Environment.Exit(1);
            return Task.FromResult(1);
        }
        public Task LogGenericWarningMessage(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            string message = $"[Warning] {DateTime.UtcNow.ToString(@"yyyy-MM-dd_HH-mm")}: {msg}";
            Console.WriteLine(message);
            File.AppendAllText($"{BasePath}{SessionId}.log", message + "\n");
            return Task.FromResult(1);
        }
        public Task LogDebugWarningMessage(string msg)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            string message = $"[Debug] {DateTime.UtcNow.ToString(@"yyyy-MM-dd_HH-mm")}: {msg}";
            Console.WriteLine(message);
            File.AppendAllText($"{BasePath}{SessionId}.log", message + "\n");
            return Task.FromResult(1);
        }

        public Task LogGenericMessage(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            string message = $"[Generic] {DateTime.UtcNow.ToString(@"yyyy-MM-dd_HH-mm")}: {msg}";
            Console.WriteLine(message);
            File.AppendAllText($"{BasePath}{SessionId}.log", message + "\n");
            return Task.FromResult(1);
        }
        public Task LogErrorMessage(Exception exception, ParsedCommand context)
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            string message = $"[Error] {DateTime.UtcNow.ToString(@"yyyy-MM-dd_HH-mm")}: {exception}";
            Console.WriteLine(message);
            File.AppendAllText($"{BasePath}{SessionId.Substring(0, 8)}.log", message + "\n");
            ExceptionReport report = Core.AddOrUpdateExceptionReport(new ExceptionReport(exception));

            if (!string.IsNullOrEmpty(Core.GlobalConfig.CriticalLoggingWebhookUrl))
            {

                var webhook = new Discord.Webhook.DiscordWebhookClient(Core.GlobalConfig.CriticalLoggingWebhookUrl);
                var builder = new EmbedBuilder()
                    .WithColor(255, 0, 0)
                    .WithCurrentTimestamp()
                    .AddField(new EmbedFieldBuilder()
                        .WithName("Error Message")
                        .WithValue(exception.GetType() + ": " + exception.Message));
                if (!string.IsNullOrEmpty(exception.StackTrace))
                    builder.AddField(new EmbedFieldBuilder()
                        .WithName("Stack Trace")
                        .WithValue(exception.StackTrace.Length > 1000 ? exception.StackTrace.Substring(exception.StackTrace.Length - 1000, 1000) : exception.StackTrace));
                
                if (context != null)
                {
                    builder.AddField(new EmbedFieldBuilder()
                        .WithName($"Location")
                        .WithValue($"{context.Guild.Name} ({context.Guild.Id}) - #{context.Channel.Name} ({context.Channel.Id})"));
                    builder.AddField(new EmbedFieldBuilder()
                        .WithName($"Author")
                        .WithValue($"{context.Author.Username}#{context.Author.Discriminator} ({context.Author.Id})"));
                    builder.AddField(new EmbedFieldBuilder()
                        .WithName($"Message")
                        .WithValue(context.Message.Content));
                }

                builder.AddField(new EmbedFieldBuilder()
                    .WithName("Count")
                    .WithValue(report.Count).WithIsInline(true))
                    .AddField(new EmbedFieldBuilder()
                    .WithName("Reported")
                    .WithValue(report.Reported)
                    .WithIsInline(true));
                    
                webhook.SendMessageAsync("", embeds: new List<Embed> { builder.Build() });

            }

            return Task.FromResult(1);
        }
    }
}