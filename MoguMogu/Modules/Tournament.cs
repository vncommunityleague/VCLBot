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
using User = MoguMogu.Database.Models.User;

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
                await ReplyAsync("Sheet ID not found!");
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

        [Command("sync", true)]
        [Summary("Dit me may")]
        public async Task Sync(string id)
        {
            if (Context.IsPrivate) return;
            await using var db = new DBContext();
            var cfg = db.Servers.FirstOrDefault(s => s.ServerId == Context.Guild.Id);
            if (!Context.Guild.Roles.Any(a => a.Id == cfg.HostRoleId || a.Id == cfg.RefRoleId) &&
                !((SocketGuildUser) Context.User).GuildPermissions.Administrator)
                return;
            if (id.ToLower().Equals("all"))
            {
                foreach (var gUser in Context.Guild.Users) {
                    var user = db.Users.FirstOrDefault(u => u.DiscordId == gUser.Id);
                    if (user == null) continue;
                    await blahzzz(gUser, user, cfg);
                }
                
                await ReplyAsync($"<@{Context.User.Id}> Done!");
            } else if (!string.IsNullOrEmpty(id)) {
                if (!ulong.TryParse(id, out var uid)) {
                    await ReplyAsync($"<@{Context.User.Id}> Invalid user id!");
                    return;
                }

                var gUser = Context.Guild.GetUser(uid);
                var user = db.Users.FirstOrDefault(u => u.DiscordId == uid);
                if (gUser == null || user == null) {
                    await ReplyAsync($"<@{Context.User.Id}> User not found!!");
                    return;
                }
                

                await blahzzz(gUser, user, cfg);
                await ReplyAsync($"<@{Context.User.Id}> Done!");
            }
        }
        
        

        [Command("csay", true)]
        [Summary("Dit me may")]
        public async Task Csay(ulong channelId, [Remainder]string message)
        {
            if (Context.IsPrivate) return;
            await using var db = new DBContext();
            var config = db.Servers.FirstOrDefault(s => s.ServerId == Context.Guild.Id);
            if (!Context.Guild.Roles.Any(a => a.Id == config.HostRoleId || a.Id == config.RefRoleId) &&
                !((SocketGuildUser) Context.User).GuildPermissions.Administrator)
                return;

            var channel = Context.Guild.GetChannel(channelId);
            if (channel == null || string.IsNullOrEmpty(message))
            {
                await ReplyAsync($"Channel ID or Message is empty!");
                return;
            }
            await ((ISocketMessageChannel) channel).SendMessageAsync(message);
            await ReplyAsync($"<@{Context.User.Id}> Sent message!").ContinueWith(
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
            if (string.IsNullOrEmpty(config.SheetsId))
            {
                await ReplyAsync("Please config sheets id!");
                return;
            }

            var desc = SpreadSheet.GetMatches(config.SheetsId)
                .Where(match => match.DateTime > DateTime.UtcNow.AddHours(config.TimeOffset) && match.DateTime <
                    DateTime.UtcNow.AddDays(-1 * (int) DateTime.UtcNow.DayOfWeek + 7).AddHours(config.TimeOffset))
                .Aggregate("",
                    (c, match) =>
                        c +
                        $"{(string.IsNullOrEmpty(c) ? "" : "\n")}- Match **{match.Id}**: **{match.TeamA}** vs **{match.TeamB}** | **Time:** `{match.DateTime:dd/MM HH:mm}` | **Referee:** `{match.Referee}`");

            await ReplyAsync(null, false,
                new EmbedBuilder().WithTitle("Upcoming Match").WithCurrentTimestamp().WithDescription(desc).Build());
        }

        [Command("embed", true)]
        [Summary("Dit me may")]
        public async Task Embed(ulong channelId, [Remainder] string url)
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
        public async Task Verification()
        {
            await using var db = new DBContext();
            var user = db.Users.FirstOrDefault(u => u.DiscordId == Context.User.Id);
            if (user != null && !Context.IsPrivate)
            {
                var cfg = db.Servers.FirstOrDefault(s => s.ServerId == Context.Guild.Id);
                if (!cfg.EnableTour) return;
                await ReplyAsync("Bạn đã verify rồi!");
                if (Context.IsPrivate) return;
                var gUser = Context.Guild.GetUser(Context.User.Id);
                await blahzzz(gUser, user, cfg);
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
                $":flag_vn: Hướng dẫn lấy role Verify:\n1. Trong chat osu! in-game hoặc chat trên web hoặc osu! IRC, copy đoạn lệnh ở dưới và gửi cho `IntelliJ IDEA` (`/query IntelliJ_IDEA`) \n2. Chờ xác nhận.\n\n**Lưu ý:** Mã xác thực sẽ hết hạn sau 5 phút. Sau thời gian đó, bạn sẽ cần thực hiện lại quá trình verify.\n\nTrong trường hợp gặp lỗi, liên hệ `hoaq#6054` hoặc `Kinue#8888`\n\n:flag_gb: Verification process guide:\n1. Using osu! in-game / web / IRC chat, copy the verify string below and send it to `IntelliJ Idea` (For IRC users, use command `/query IntelliJ_Idea`)\n2. Just wait until you receive a confirmation message from the bot.\n\n**Notice**: Verification request will be timed out after 5 minutes of inactivity. After that, you will have to start the process again.\n\nIf you encounter any errors, please contact `hoaq#6054` or `Kinue#8888`.\n\n`{BotConfig.config.IrcPrefix}verify {token}`");
        }
        // ? name died brain
        private async Task blahzzz(IGuildUser gUser, User user, Config cfg)
        {
            if (gUser == null)
            {
                await ReplyAsync("User not found!");
                return;
            }
            var role = Context.Guild.Roles.FirstOrDefault(r => r.Name.ToLower().Equals("verified")) ??
                       (IRole) Context.Guild.CreateRoleAsync(cfg.VerifyRoleName, new GuildPermissions(37084736),
                           null, false, false).Result;
            await gUser.ModifyAsync(_177013 =>
                _177013.Nickname = Program.OsuClient.GetUserByUserIdAsync(user.OsuId, GameMode.Standard).Result
                    .Username);
            await gUser.AddRoleAsync(role);
        }

        private static string GenUniqueString()
        {
            var bytes = new byte[40];
            using (var crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetBytes(bytes);
            }

            return Regex.Replace(Guid.NewGuid().ToString("N") + Convert.ToBase64String(bytes), "[^A-Za-z0-9]", "")
                .Substring(0, 10);
        }
    }
}
