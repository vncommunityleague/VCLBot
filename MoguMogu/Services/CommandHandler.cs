using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MoguMogu.Database;
using OsuSharp;

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
            _discord.UserJoined += DiscordOnUserJoined;
        }

        private async Task DiscordOnUserJoined(SocketGuildUser gUser)
        {
            await using var db = new DBContext();
            var config = db.Servers.FirstOrDefault(s => s.ServerId == gUser.Guild.Id);
            var user = db.Users.FirstOrDefault(u => u.DiscordId == gUser.Id);
            if (!config.EnableTour || user == null) return;
            var role = gUser.Guild.Roles.FirstOrDefault(r => r.Name.ToLower().Equals("verified")) ??
                       (IRole) gUser.Guild.CreateRoleAsync(config.VerifyRoleName, new GuildPermissions(37084736),
                           null, false, false).Result;
            await gUser.AddRoleAsync(role);
            await gUser.ModifyAsync(_177013 =>
                _177013.Nickname = Program.OsuClient.GetUserByUserIdAsync(user.OsuId, GameMode.Standard).Result
                    .Username);
        }

        private async Task OnMessageReceivedAsync(SocketMessage s)
        {
            if (!(s is SocketUserMessage msg) || msg.Author.IsBot || s.Author.IsWebhook) return;

            var context = new SocketCommandContext(_discord, msg);

            var argPos = 0;
            await using var db = new DBContext();
            var curPrefix = msg.Channel is SocketGuildChannel
                ? db.Servers.FirstOrDefault(s => s.ServerId == ((SocketGuildChannel) msg.Channel).Guild.Id)?.Prefix
                : BotConfig.config.BotPrefix;
            if (msg.HasStringPrefix(curPrefix, ref argPos) ||
                msg.HasMentionPrefix(_discord.CurrentUser, ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _provider);

                if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                    await context.Channel.SendMessageAsync(result.ToString());
            }
        }
    }
}