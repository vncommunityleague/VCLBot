using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace MoguMogu.Services
{
    public class CommandHandler
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _provider;

        public CommandHandler(DiscordSocketClient discord, CommandService commands, IServiceProvider provider)
        {
            _discord = discord;
            _commands = commands;
            _provider = provider;
            _discord.MessageReceived += OnMessageReceivedAsync;
        }

        private async Task OnMessageReceivedAsync(SocketMessage s)
        {
            if (!(s is SocketUserMessage msg) || msg.Author.IsBot || s.Author.IsWebhook) return;

            var context = new SocketCommandContext(_discord, msg);

            var argPos = 0;
            if (msg.HasStringPrefix(BotConfig.config.BotPrefix, ref argPos) ||
                msg.HasMentionPrefix(_discord.CurrentUser, ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _provider);

                if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                    await context.Channel.SendMessageAsync(result.ToString());
            }
        }
    }
}