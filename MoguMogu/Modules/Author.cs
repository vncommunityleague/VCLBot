using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using OsuSharp;

namespace MoguMogu.Modules
{
    [Summary(":Okayu_Smile:")]
    public class Author : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;
        private readonly OsuClient _osuClient;

        public Author(DiscordSocketClient discord, CommandService commands, OsuClient osuClient)
        {
            _discord = discord;
            _commands = commands;
            _osuClient = osuClient;
        }

        [Command("changeactivity")]
        [Summary("Dit me may")]
        public async Task activity([Remainder] string activity)
        {
            if (BotConfig.config.OwnerID.Any(a => a == Context.User.Id))
            {
                await _discord.SetGameAsync(activity);
                await Context.Channel.SendMessageAsync($"Changed bot activity to: `{activity}`");
            }
        }

        [Command("changeusername")]
        [Summary("Dit me may")]
        public async Task username([Remainder] string username)
        {
            if (BotConfig.config.OwnerID.Any(a => a == Context.User.Id))
            {
                await _discord.CurrentUser.ModifyAsync(_177013 => _177013.Username = username);
                await Context.Channel.SendMessageAsync($"Changed bot username to: `{username}`");
            }
        }

        [Command("changeavatar")]
        [Summary("Dit me may")]
        public async Task avatar([Remainder] string url = null)
        {
            if (BotConfig.config.OwnerID.Any(a => a == Context.User.Id))
            {
                var a = Context.Message.Attachments.Count == 0 ? url : Context.Message.Attachments.ToList()[0].Url;
                using var client = new WebClient();
                await using var stream =
                    new MemoryStream(
                        client.DownloadData(string.IsNullOrEmpty(a) ? Context.User.GetAvatarUrl(size: 2048) : a));
                await _discord.CurrentUser.ModifyAsync(_177013 => _177013.Avatar = new Image(stream));
                await Context.Channel.SendMessageAsync("Changed bot avatar!!!");
            }
        }
    }
}