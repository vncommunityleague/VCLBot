using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace MoguMogu.Services
{
    public class LoggingService
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;

        public LoggingService(DiscordSocketClient discord, CommandService commands)
        {
            _discord = discord;
            _commands = commands;

            _discord.Log += OnLogAsync;
            _commands.Log += OnLogAsync;
        }

        private Task OnLogAsync(LogMessage msg)
        {
            Logger.Log(msg.Exception?.ToString() ?? msg.Message, m: "Discord");
            return Task.FromResult(0);
        }
    }
}