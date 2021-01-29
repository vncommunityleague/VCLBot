using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using MoguMogu.Database;
using MoguMogu.IRC.Irc;
using MoguMogu.Services;
using OsuSharp;

//TODO Chay duoc la duoc bo deo can biet
namespace MoguMogu
{
    internal class Program
    {
        public static OsuClient OsuClient;
        public static OsuIrcClient IRC;

        private static void Main(string[] args)
        {
            OsuClient = new OsuClient(new OsuSharpConfiguration {ApiKey = BotConfig.config.OsuApi});
            LoadIrc().ConfigureAwait(false);
            LoadDiscord().ConfigureAwait(false);
            using var db = new DBContext();
            Thread.Sleep(-1);
        }

        private static async Task LoadIrc()
        {
            IRC = new OsuIrcClient();
            IRC.OnWelcomeMessageReceived += (s, e) => Logger.Log("Welcome message received", m: "Irc");
            IRC.OnPrivateMessageReceived += IrcOnOnPrivateMessageReceived;
            IRC.OnPrivateBanchoMessageReceived += (s, e) => Logger.Log($"Bancho message: {e.Message}", m: "Irc");
            await IRC.ConnectAsync(reconnectDelay: TimeSpan.FromMinutes(60)).ConfigureAwait(false);
            await IRC.LoginAsync(BotConfig.config.IrcUsername, BotConfig.config.IrcPassword).ConfigureAwait(false);
            Logger.Log("Loaded Irc", m: "Irc");
        }

        private static void IrcOnOnPrivateMessageReceived(object sender, IrcPrivateMessageEventArgs e)
        {
            Logger.Log($"Message from {e.Sender}: {e.Message}", m: "Irc");
            //why bro idk??
            var msg = e.Message;
            if (!msg.StartsWith(BotConfig.config.IrcPrefix)) return;
            msg = msg.Substring(BotConfig.config.IrcPrefix.Length);
            var array = msg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            switch (array[0].ToLower())
            {
                case "info":
                    IRC.SendMessageAsync(e.Sender, "Mogu mogu okayuuuuuuuuu");
                    break;
                case "verify":
                    if (array.Length < 2)
                    {
                        IRC.SendMessageAsync(e.Sender, "Please input verify token!!");
                        return;
                    }

                    IRC.SendMessageAsync(e.Sender, "Pending verification, please wait.....");
                    break;
            }
        }

        private static async Task LoadDiscord()
        {
            var services = new ServiceCollection();
            services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Verbose,
                    MessageCacheSize = 1000
                }))
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    LogLevel = LogSeverity.Verbose,
                    DefaultRunMode = RunMode.Async
                }))
                .AddSingleton<CommandHandler>()
                .AddSingleton<StartupService>()
                .AddSingleton<LoggingService>()
                .AddSingleton(OsuClient);

            var provider = services.BuildServiceProvider();
            provider.GetRequiredService<LoggingService>();
            provider.GetRequiredService<CommandHandler>();

            await provider.GetRequiredService<StartupService>().StartAsync();
            Logger.Log("Loaded Discord", m: "Irc");
        }
    }
}