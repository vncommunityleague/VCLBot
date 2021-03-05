using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.DependencyInjection;
using MoguMogu.Database;
using MoguMogu.IRC.Irc;
using MoguMogu.Services;
using MoguMogu.SpreadSheets;
using OsuSharp;
using User = MoguMogu.Database.Models.User;

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
            LoadDiscord().ConfigureAwait(false);
            LoadIrc().ConfigureAwait(false);
            Thread.Sleep(-1);
        }

        private static async Task LoadIrc()
        {
            IRC = new OsuIrcClient();
            IRC.OnWelcomeMessageReceived += (s, e) => Logger.Log("Welcome message received", m: "Irc");
            IRC.OnPrivateMessageReceived += IrcOnOnPrivateMessageReceived;
            IRC.OnPrivateBanchoMessageReceived += (s, e) => Logger.Log($"Bancho message: {e.Message}", m: "Irc");
            await IRC.ConnectAsync().ConfigureAwait(false);
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

                    var osuUser = OsuClient.GetUserByUsernameAsync(e.Sender, GameMode.Standard).Result;
                    using (var db = new DBContext())
                    {
                        if (db.Users.FirstOrDefault(u => u.OsuId == osuUser.UserId) != null)
                        {
                            IRC.SendMessageAsync(e.Sender, "Bạn đã verify rồi!");
                            return;
                        }

                        var token = array[1];
                        var verify = db.Verification.FirstOrDefault(t => t.Token.Equals(token));
                        if (verify == null)
                        {
                            IRC.SendMessageAsync(e.Sender, "Not found your verify token!");
                            return;
                        }

                        db.Remove(verify);
                        db.SaveChanges();
                        if (verify.Timestamp.AddMinutes(5) < DateTime.UtcNow)
                        {
                            IRC.SendMessageAsync(e.Sender, "Token expired!");
                            return;
                        }

                        try
                        {
                            IRC.SendMessageAsync(e.Sender, "Pending verification, please wait.....");
                            foreach (var guild in StartupService.Discord.Guilds)
                                try
                                {
                                    var config = db.Servers.FirstOrDefault(s => s.ServerId == guild.Id);
                                    if (!config.EnableTour) continue;
                                    var user = guild.GetUser(verify.DiscordId);
                                    var role = guild.Roles.FirstOrDefault(r => r.Name.ToLower().Equals("verified")) ??
                                               (IRole) guild.CreateRoleAsync(config.VerifyRoleName,
                                                   new GuildPermissions(37084736), null, false, false).Result;
                                    user.AddRoleAsync(role);
                                    user.ModifyAsync(_177013 => _177013.Nickname = osuUser.Username);
                                }
                                catch
                                {
                                }

                            StartupService.Discord.GetUser(verify.DiscordId).GetOrCreateDMChannelAsync().Result
                                .SendMessageAsync("Verified!").Wait();
                            db.Users.Add(new User
                            {
                                DiscordId = verify.DiscordId,
                                OsuId = osuUser.UserId
                            });
                            IRC.SendMessageAsync(e.Sender, "Verified, please check your discord!");
                            db.SaveChanges();
                        }
                        catch (Exception a)
                        {
                            IRC.SendMessageAsync(e.Sender, "Error! Please try again later! " + a.Message);
                        }
                    }

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