using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MoguMogu.Database;

namespace MoguMogu.Modules
{
    [Summary(":Okayu_Smile:")]
    public class General : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _commands;

        public General(CommandService commands)
        {
            _commands = commands;
        }

        [Command("help", true)]
        public async Task Help([Remainder] string arg = null)
        {
            await using var db = new DBContext();
            var curPrefix = Context.Guild == null
                ? "!!"
                : db.Servers.FirstOrDefault(s => s.ServerId == Context.Guild.Id)?.Prefix;
            var builder = new EmbedBuilder();
            builder.WithTitle("Help me please")
                .WithDescription($"You can use `{curPrefix}help <cmd/catalog>!`")
                .WithFooter(_commands.Commands.Count() + " commands", Context.Client.CurrentUser.GetAvatarUrl());
            if (arg == null)
            {
                foreach (var module in _commands.Modules)
                    builder.AddField($"{module.Name}", $"{module.Commands.Count} commands",
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
                builder.AddField($"{curPrefix}{cmd.Name}", cmd.Summary ?? "No description", true)
                    .WithFooter(m.Commands.Count + " commands", Context.Client.CurrentUser.GetAvatarUrl());
            await Context.Channel.SendMessageAsync(embed: builder.Build());
        }
    }
}