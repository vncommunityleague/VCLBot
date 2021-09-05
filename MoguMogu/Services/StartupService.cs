using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MoguMogu.Database;
using MoguMogu.Database.Models;
using MoguMogu.Schedule;

namespace MoguMogu.Services
{
    public class StartupService
    {
        public static DiscordSocketClient Discord;
        private readonly CommandService _commands;
        private readonly IServiceProvider _provider;

        public StartupService(IServiceProvider provider, DiscordSocketClient discord, CommandService commands)
        {
            _provider = provider;
            Discord = discord;
            _commands = commands;
            Discord.Ready += DiscordOnReady;
            Discord.JoinedGuild += DiscordOnJoinedGuild;
        }

        private async Task DiscordOnJoinedGuild(SocketGuild guild)
        {
            await guild.DefaultChannel.SendMessageAsync("mogu mogu amogus");
            await using var db = new DBContext();
            if (db.Servers.FirstOrDefault(s => s.ServerId == guild.Id) == null)
                await db.Servers.AddAsync(new Config {ServerId = guild.Id});
            await db.SaveChangesAsync();
        }

        private async Task DiscordOnReady()
        {
            Logger.Log($"Logged as: '{Discord.CurrentUser.Username}#{Discord.CurrentUser.Discriminator}'",
                m: "Discord");
            await Discord.SetGameAsync(BotConfig.config.BotActivity);
            await using var db = new DBContext();
            foreach (var guild in Discord.Guilds)
                if (db.Servers.FirstOrDefault(s => s.ServerId == guild.Id) == null)
                    await db.Servers.AddAsync(new Config {ServerId = guild.Id});
            await db.SaveChangesAsync();
            new CleanTask().Start();
            new AutoTask().Start();
        }

        public async Task StartAsync()
        {
            await Discord.LoginAsync(TokenType.Bot, BotConfig.config.BotToken);
            await Discord.StartAsync();
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }
    }
}