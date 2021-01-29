using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

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
            Discord.JoinedGuild += guild => guild.DefaultChannel.SendMessageAsync("Mogu mogu okayuuuuuuuuuuuuuu!");
        }

        private async Task DiscordOnReady()
        {
            Logger.Log($"Logged as: '{Discord.CurrentUser.Username}#{Discord.CurrentUser.Discriminator}'",
                m: "Discord");
            await Discord.SetGameAsync(BotConfig.config.BotActivity);
        }

        public async Task StartAsync()
        {
            await Discord.LoginAsync(TokenType.Bot, BotConfig.config.BotToken);
            await Discord.StartAsync();
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }
    }
}