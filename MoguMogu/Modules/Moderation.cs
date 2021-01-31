using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MoguMogu.Database;

namespace MoguMogu.Modules
{
    [Summary(":Okayu_Smile:")]
    public class Moderation : ModuleBase<SocketCommandContext>
    {
        [Command("config")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Config([Remainder] string str = "")
        {
            if (Context.IsPrivate) return;
            var array = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            await using var db = new DBContext();
            var builder = new EmbedBuilder().WithCurrentTimestamp().WithColor(Color.Purple).WithTitle("Current config");
            var config = db.Servers.FirstOrDefault(s => s.ServerId == Context.Guild.Id);
            if (array.Length == 0)
            {
                foreach (var method in config.GetType().GetMethods())
                    if (method.Name.StartsWith("get_") &&
                        !(method.Name.Equals("get_Id") || method.Name.Equals("get_ServerId")))
                    {
                        var value = method.Invoke(config, null);
                        builder.AddField(
                            string.Concat(
                                    method.Name.Substring(4).Select(x => char.IsUpper(x) ? " " + x : x.ToString()))
                                .TrimStart(' '), string.IsNullOrEmpty(value?.ToString()) ? "Empty" : value.ToString(),
                            true);
                    }

                await ReplyAsync(null, false, builder.Build());
                return;
            }

            var cmd = array[0].ToLower();
            foreach (var method in config.GetType().GetMethods())
            {
                if (!Regex.IsMatch(method.Name, "^(set|get)_") ||
                    Regex.IsMatch(method.Name, "(set|get)_(ServerId|Id)") ||
                    !string.Concat(method.Name.Substring(4).Select(x => char.IsUpper(x) ? "_" + x : x.ToString()))
                        .TrimStart('_').ToLower().Equals(cmd)) continue;
                if (array.Length > 1)
                    try
                    {
                        var m = config.GetType().GetMethod("set_" + method.Name.Substring(4));
                        m.Invoke(config,
                            new[] {Convert.ChangeType(array[1], m.GetParameters()[0].ParameterType)});
                        await ReplyAsync($"Change `{cmd}` value to **{array[1]}**");
                        await db.SaveChangesAsync();
                        return;
                    }
                    catch
                    {
                        await ReplyAsync($"Can't change `{cmd}` value to **{array[1]}**");
                        return;
                    }

                await ReplyAsync(
                    $"`{cmd}` current value is **{config.GetType().GetMethod("get_" + method.Name.Substring(4)).Invoke(config, null)}**");
                return;
            }

            await ReplyAsync($"Not found `{cmd}`");
        }
    }
}