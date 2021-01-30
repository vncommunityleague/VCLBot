using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MoguMogu.Data;
using MoguMogu.Database;
using MoguMogu.Database.Models;
using MoguMogu.SpreadSheets;
using Newtonsoft.Json;
using OsuSharp;

namespace MoguMogu.Modules
{
    [Summary(":Okayu_Smile:")]
    public class Tournament : ModuleBase<SocketCommandContext>
    {
        [Command("postresult")]
        public async Task PostResult(ulong channelId, [Remainder] string matchId)
        {
            if (Context.IsPrivate) return;
            await using var db = new DBContext();
            var config = db.Servers.FirstOrDefault(s => s.ServerId == Context.Guild.Id);
            if (!Context.Guild.Roles.Any(a => a.Id == config.HostRoleId || a.Id == config.RefRoleId) &&
                !((SocketGuildUser) Context.User).GuildPermissions.Administrator)
                return;

            if (string.IsNullOrEmpty(config.SheetsId))
            {
                await ReplyAsync("Please config sheets id!");
                return;
            }

            var channel = Context.Guild.GetChannel(channelId);
            if (channel == null)
            {
                await ReplyAsync($"Channel ID not found! `{channelId}`");
                return;
            }

            var embed = SpreadSheet.GetMatchEmbed(matchId, config.SheetsId);
            if (embed == null)
            {
                await ReplyAsync($"Match not found or not finish! `{matchId}`");
                return;
            }

            if (db.Results.FirstOrDefault(r =>
                    r.SheetsId.Equals(config.SheetsId) && r.MatchId.Equals(matchId) &&
                    r.ServerId == Context.Guild.Id) ==
                null)
            {
                await db.Results.AddAsync(new Result
                {
                    SheetsId = config.SheetsId,
                    MatchId = matchId,
                    ServerId = Context.Guild.Id
                });
                await db.SaveChangesAsync();
            }

            await ((ISocketMessageChannel) channel).SendMessageAsync(null, false, embed);
            await ReplyAsync($"<@{Context.User.Id}> Sent result!").ContinueWith(
                async m =>
                {
                    await Task.Delay(7000);
                    await m.Result.DeleteAsync();
                });
        }

        [Command("upcoming", true)]
        [Summary("Dit me may")]
        public async Task UpCumming()
        {
            if (Context.IsPrivate) return;
            await using var db = new DBContext();
            var config = db.Servers.FirstOrDefault(s => s.ServerId == Context.Guild.Id);
            if (string.IsNullOrEmpty(config.SheetsId)) {
                await ReplyAsync("Please config sheets id!");
                return;
            }
            var desc = SpreadSheet.GetMatches(config.SheetsId)
                .Where(match => match.DateTime > DateTime.UtcNow.AddHours(config.TimeOffset) && match.DateTime < DateTime.UtcNow.AddDays(-1 * (int) DateTime.UtcNow.DayOfWeek + 7).AddHours(config.TimeOffset))
                .Aggregate("", (c, match) => c + $"{(string.IsNullOrEmpty(c) ? "" : "\n")}- Match **{match.Id}**: **{match.Team1}** vs **{match.Team2}** | **Time:** `{match.DateTime:dd/MM HH:mm}` | **Referee:** `{match.Referee}`");

            await ReplyAsync(null, false, new EmbedBuilder().WithTitle("Upcoming Match").WithCurrentTimestamp().WithDescription(desc).Build());
        }
        [Command("embed", true)]
        [Summary("Dit me may")]
        public async Task Embed(ulong channelId, [Remainder]string url)
        {
            if (Context.IsPrivate) return;
            await using var db = new DBContext();
            var config = db.Servers.FirstOrDefault(s => s.ServerId == Context.Guild.Id);
            if (!Context.Guild.Roles.Any(a => a.Id == config.HostRoleId || a.Id == config.RefRoleId) &&
                !((SocketGuildUser) Context.User).GuildPermissions.Administrator)
                return;
            var channel = Context.Guild.GetChannel(channelId);
            if (channel == null)
            {
                await ReplyAsync($"Channel ID not found! `{channelId}`");
                return;
            }
            var parse = JsonConvert.DeserializeObject<EmbedJson>(new WebClient().DownloadString(url));
            await ((ISocketMessageChannel) channel).SendMessageAsync(parse.Content, false, parse.Embed.GetEmbed());
            await ReplyAsync($"<@{Context.User.Id}> Sent result!").ContinueWith(
                async m =>
                {
                    await Task.Delay(7000);
                    await m.Result.DeleteAsync();
                });
        }

        [Command("verify", true)]
        [Alias("verification")]
        public async Task verification()
        {
            await using var db = new DBContext();
            var config = db.Servers.FirstOrDefault(s => s.ServerId == Context.Guild.Id);
            if (!config.EnableTour) return;
            var user = db.Users.FirstOrDefault(u => u.DiscordId == Context.User.Id);
            if (user != null)
            {
                await ReplyAsync("Bạn đã verify rồi!");
                if (Context.IsPrivate) return;
                var gUser = Context.Guild.GetUser(Context.User.Id);
                var role = Context.Guild.Roles.FirstOrDefault(r => r.Name.ToLower().Equals("verified")) ??
                           (IRole) Context.Guild.CreateRoleAsync(config.VerifyRoleName, new GuildPermissions(37084736),
                               null, false, false).Result;
                await gUser.ModifyAsync(_177013 =>
                    _177013.Nickname = Program.OsuClient.GetUserByUserIdAsync(user.OsuId, GameMode.Standard).Result
                        .Username);
                await gUser.AddRoleAsync(role);
                return;
            }

            //verify
            var dm = await Context.User.GetOrCreateDMChannelAsync();
            var token = GenUniqueString();
            var tmp = db.Verification.FirstOrDefault(t => t.DiscordId == Context.User.Id);
            if (tmp != null)
                db.Verification.Remove(tmp);

            while (db.Verification.FirstOrDefault(t => t.Token.Equals(token)) != null)
                token = GenUniqueString();
            await db.Verification.AddAsync(new Verification
            {
                Token = token,
                DiscordId = Context.User.Id,
                Timestamp = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
            await dm.SendMessageAsync(
                $"Hướng dẫn lấy role Verify:\n1. Trong chat osu! in-game hoặc chat trên web hoặc osu! IRC, copy đoạn lệnh ở dưới và gửi cho `IntelliJ IDEA` (`/query IntelliJ_IDEA`) \n2. Chờ xác nhận.\n\n**Lưu ý:** Mã xác thực sẽ hết hạn sau 5 phút. Sau thời gian đó, bạn sẽ cần thực hiện lại quá trình verify.\n\nTrong trường hợp gặp lỗi, liên hệ `hoaq#6054` hoặc `Kinue#8888`\n\n`{BotConfig.config.IrcPrefix}verify {token}`");
        }

        private static string GenUniqueString()
        {
            var bytes = new byte[40];
            using (var crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetBytes(bytes);
            }

            return Regex.Replace(Guid.NewGuid().ToString("N") + Convert.ToBase64String(bytes), "[^A-Za-z0-9]", "").Substring(0, 10);
        }
    }
}