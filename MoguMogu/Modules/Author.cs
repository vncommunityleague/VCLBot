using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MoguMogu.Database;
using OsuSharp;
using User = MoguMogu.Database.Models.User;

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


        [Command("merge")]
        [Summary("Dit me may")]
        public async Task merge(ulong role)
        {
            if (BotConfig.config.OwnerID.Any(a => a == Context.User.Id))
            {
                await ReplyAsync($"Starting merge for `{role}`, please wait....");
                await using var db = new DBContext();
                var i = 0;
                foreach (var user in Context.Guild.Users)
                    if (user.Roles.Any(v => v.Id == role) &&
                        db.Users.FirstOrDefault(u => u.DiscordId == user.Id) == null)
                        try
                        {
                            var osuUser = await _osuClient.GetUserByUsernameAsync(user.Nickname ?? user.Username,
                                GameMode.Standard);
                            if (osuUser == null) continue;
                            await db.Users.AddAsync(new User
                            {
                                DiscordId = user.Id,
                                OsuId = osuUser.UserId
                            });
                            i++;
                        }
                        catch
                        {
                        }

                await db.SaveChangesAsync();
                await ReplyAsync($"Merged {i} members");
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