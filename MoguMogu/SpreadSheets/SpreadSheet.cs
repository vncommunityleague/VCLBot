using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Util.Store;
using MoguMogu.Data;
using MoguMogu.Database;
using Newtonsoft.Json;
using OsuSharp;
using Embed = Discord.Embed;

namespace MoguMogu.SpreadSheets
{
    public class SpreadSheet
    {
        private static readonly string[] Scopes = {SheetsService.Scope.Spreadsheets};
        private static readonly SheetsService _service;

        static SpreadSheet()
        {
            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = BotConfig.config.Client_Id,
                    ClientSecret = BotConfig.config.Client_Secret
                },
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore("token.json", true)).Result;

            _service = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "177013"
            });
        }

        public static Embed GetMatchEmbed(string matchId, string sheetsId, bool check = false)
        {
            try
            {
                var response = _service.Spreadsheets.Values.Get(sheetsId, $"{matchId}!B1:L")
                    .Execute();
                var values = response.Values;

                /*
                var blueTeam = GetValue(values, 1, 4);
                var redTeam = GetValue(values, 1, 6);
                var blueScore = GetValue(values, 2, 4);
                var redScore = GetValue(values, 2, 6);
                var blueBan1 = GetValue(values, 2, 9);
                var blueBan2 = GetValue(values, 2, 10);
                var redBan1 = GetValue(values, 3, 9);
                var redBan2 = GetValue(values, 3, 10);
                var blueRoll = GetValue(values, 14, 9);
                var redRoll = GetValue(values, 14, 10);
                var refName = GetRefName(matchId, sheetsId);
                var matchTitle = $"{GetValue(values, 5, 1)} - Match {matchId}";
                
                var builder = new EmbedBuilder().WithTitle(matchTitle).WithFooter("Refereed by " + refName)
                    .WithTimestamp(mpRoom.Match.EndTime.Value);
                builder.AddField($":trophy: **{blueTeam} | {blueScore}** - {redScore} | {redTeam}",
                    $"-------------\nRolls: \n**{blueTeam}**: {blueRoll}\n**{redTeam}**: {redRoll}\n*{(int.Parse(blueRoll) < int.Parse(redRoll) ? redTeam : blueTeam)} chọn pick và ban sau*\n-------------\nBans:\n**{redTeam}**:\n> {redBan1}{(string.IsNullOrEmpty(redBan2) ? "" : $"\n> {redBan2}")}\n\n**{blueTeam}**:\n> {blueBan1}{(string.IsNullOrEmpty(blueBan2) ? "" : $"\n> {blueBan2}")}\n-------------\nMP Link: <{mpLink}>");
               */
                var mpLink = GetValue(values, 3, 1);
                var doubleCheck = GetValue(values, 18, 10).Equals("TRUE");
                var isMatchDone = !string.IsNullOrEmpty(mpLink) && Program.OsuClient.GetMultiplayerRoomAsync(long.Parse(mpLink.Substring(mpLink.LastIndexOf("/") + 1))).Result.Match.EndTime != null;
                return !isMatchDone || !doubleCheck && check ? null : JsonConvert.DeserializeObject<EmbedJson>(GetValue(values, 13, 8))?.Embed.GetEmbed();
            }
            catch
            {
                Logger.Log($"Error when parse sheet: {matchId}!", LogLevel.Error, "Sheet");
            }

            return null;
        }

        private static string GetRefName(string matchId, string sheetsId)
        {
            var response = _service.Spreadsheets.Values.Get(sheetsId, "Schedule!B5:G").Execute();
            var values = response.Values;
            foreach (var row in values)
                try
                {
                    if (row[0].ToString().Equals(matchId))
                        return row[5].ToString();
                }
                catch
                {
                }

            return string.Empty;
        }

        private static string GetValue(IList<IList<object>> values, int a, int b)
        {
            try
            {
                return values[a][b].ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string ResolveUsername(string name, DBContext db)
        {
            var osuClient = Program.OsuClient;
            var oUser = osuClient.GetUserByUsernameAsync(name, GameMode.Standard).Result;
            if (oUser == null) return string.Empty;
            var user = db.Users.FirstOrDefault(u => u.OsuId == oUser.UserId);
            return user == null ? $"`{oUser.Username}`" : $"<@{user.DiscordId}>)";
        }


        public static IEnumerable<Match> GetMatches(string sheetsId)
        {
            var matches = new List<Match>();
            var year = DateTime.Now.Year;
            try
            {
                var response = _service.Spreadsheets.Values.Get(sheetsId, "Schedule!B6:G").Execute();
                var values = response.Values;
                for (var i = 0; i < values.Count; i++)
                    try
                    {
                        var row = values[i];
                        matches.Add(new Match(row[0].ToString(), DateTime.ParseExact($"{row[1]}/{year} {row[2]}", "ddd, dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture), GetValue(values, i, 3), GetValue(values, i, 4), GetValue(values, i, 5)));
                    }
                    catch
                    {
                        Logger.Log($"Can't parse match id {values[i][0]}", LogLevel.Error, "Sheet");
                    }
            }
            catch
            {
            }

            return matches;
        }
    }
}