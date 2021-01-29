using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MoguMogu.Database;
using MoguMogu.Database.Models;
using OsuSharp;

namespace MoguMogu.Modules
{
    public class General : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _commands;
        private readonly OsuClient _osuClient;
        private readonly DiscordSocketClient _discord;

        public General(DiscordSocketClient discord, CommandService commands, OsuClient osuClient)
        {
            _discord = discord;
            _commands = commands;
            _osuClient = osuClient;
        }

        [Command("help", true)]
        public async Task Help([Remainder] string arg = null)
        {
            await Context.Channel.DeleteMessageAsync(Context.Message).ConfigureAwait(false);
            var builder = new EmbedBuilder();
            builder.WithTitle("Help me please")
                .WithDescription($"You can use `{BotConfig.config.BotPrefix}help <cmd/catalog>!`")
                .WithFooter(_commands.Commands.Count() + " commands", Context.Client.CurrentUser.GetAvatarUrl());
            if (arg == null)
            {
                foreach (var module in _commands.Modules)
                    builder.AddField($"{module.Summary.ToLower()} {module.Name}", $"{module.Commands.Count} commands",
                        true);
                await Context.Channel.SendMessageAsync(embed: builder.Build());
                return;
            }

            arg = arg.ToLower();
            var m = _commands.Modules.FirstOrDefault(c => c.Name.ToLower().Equals(arg));
            if (m == null)
            {
                var cmd = _commands.Commands.FirstOrDefault(c => c.Name.ToLower().Equals(arg));
                if (cmd == null)
                {
                    await Context.Channel.SendMessageAsync(
                        $"Not found command/catalog `{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(arg)}`");
                    return;
                }

                var desc = "Description: " + (cmd.Summary ?? "No description");
                var parameter = cmd.Parameters.Aggregate<ParameterInfo, string>(null,
                    (current, parameterInfo) =>
                        $"{(current == null ? null : current + "\n")}'{parameterInfo.Name}' [{parameterInfo.Type.Name}] - {parameterInfo.Summary ?? "No description"}");
                builder.WithDescription(desc).WithTitle($"Help for: {cmd.Name}");
                if (!string.IsNullOrEmpty(parameter)) builder.AddField("Parameters", parameter);
                await Context.Channel.SendMessageAsync(embed: builder.Build());
                return;
            }

            foreach (var cmd in m.Commands)
                builder.AddField($"{BotConfig.config.BotPrefix}{cmd.Name}", cmd.Summary ?? "No description", true)
                    .WithFooter(m.Commands.Count + " commands", Context.Client.CurrentUser.GetAvatarUrl());
            await Context.Channel.SendMessageAsync(embed: builder.Build());
        }

        [Command("activity")]
        [Summary("Dit me may")]
        public async Task activity([Remainder] string activity)
        {
            if (BotConfig.config.OwnerID.Any(a => a == Context.User.Id))
            {
                await _discord.SetGameAsync(activity);
                await Context.Channel.SendMessageAsync($"Changed bot activity to: `{activity}`");
            }
        }
        
        [Command("username")]
        [Summary("Dit me may")]
        public async Task username([Remainder] string username)
        {
            if (BotConfig.config.OwnerID.Any(a => a == Context.User.Id))
            {
                await _discord.CurrentUser.ModifyAsync(_177013 => _177013.Username = username);
                await Context.Channel.SendMessageAsync($"Changed bot username to: `{username}`");
            }
        }
        
        [Command("avatar")]
        [Summary("Dit me may")]
        public async Task avatar([Remainder] string url = null)
        {
            if (BotConfig.config.OwnerID.Any(a => a == Context.User.Id))
            {
                var a = Context.Message.Attachments.Count == 0 ? url : Context.Message.Attachments.ToList()[0].Url;
                using var client = new WebClient();
                await using var stream = new MemoryStream(client.DownloadData(string.IsNullOrEmpty(a) ? Context.User.GetAvatarUrl(size: 2048) : a));
                await _discord.CurrentUser.ModifyAsync(_177013 => _177013.Avatar = new Image(stream));
                await Context.Channel.SendMessageAsync($"Changed bot avatar!!!");
            }
        }

        [Command("verify", true)]
        [Alias("verification")]
        public async Task verification()
        {
            await using var db = new DBContext();
            if (db.Users.FirstOrDefault(u => u.DiscordId == Context.User.Id) != null)
            {
                await Context.Channel.SendMessageAsync("You're verified!");
                return;
            }

            //verify
            var dm = await Context.User.GetOrCreateDMChannelAsync();
            var token = GenUniqueString();
            while (db.Verification.FirstOrDefault(t => t.Token.Equals(token)) != null)
                token = GenUniqueString();
            await db.Verification.AddAsync(new Verification
            {
                Token = token,
                Timestamp = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
            await dm.SendMessageAsync($"{BotConfig.config.IrcPrefix}verify {token}");
        }

        private string GenUniqueString()
        {
            var bytes = new byte[40];
            using (var crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetBytes(bytes);
            }

            return Regex.Replace(Guid.NewGuid().ToString("N") + Convert.ToBase64String(bytes), "[^A-Za-z0-9]", "");
        }
    }
}