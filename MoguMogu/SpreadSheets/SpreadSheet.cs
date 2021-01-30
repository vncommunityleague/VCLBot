using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Util.Store;

namespace MoguMogu.SpreadSheets
{
    public class SpreadSheet
    {
        private static readonly string[] Scopes = {SheetsService.Scope.SpreadsheetsReadonly};
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

        public static void ReadMatch()
        {
            var matches = GetMatches();
            foreach (var match in matches)
                try
                {
                    var matchId = match.Id;
                    var response = _service.Spreadsheets.Values.Get(BotConfig.config.Sheets_Id, $"{matchId}!B1:L")
                        .Execute();
                    var values = response.Values;
                    var mpLink = GetValue(values, 3, 1);
                    var blueTeam = GetValue(values, 1, 4);
                    var redTeam = GetValue(values, 1, 6);
                    var blueScore = GetValue(values, 2, 4);
                    var redScore = GetValue(values, 2, 6);
                    var blueBan1 = GetValue(values, 2, 9);
                    var blueBan2 = GetValue(values, 2, 10);
                    var redBan1 = GetValue(values, 3, 9);
                    var redBan2 = GetValue(values, 3, 10);
                    var currentScore = GetValue(values, 10, 9);
                    var blueRoll = GetValue(values, 14, 9);
                    var redRoll = GetValue(values, 14, 10);
                    var isMatchDone = currentScore.Contains("Congratulations to") &&
                                      currentScore.Contains("for winning the match");
                }
                catch
                {
                    Logger.Log($"Error when parse sheet: {match.Id}!", LogLevel.Error, "Sheet");
                }
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

        private static IEnumerable<Match> GetMatches()
        {
            var matches = new List<Match>();
            var response = _service.Spreadsheets.Values.Get(BotConfig.config.Sheets_Id, "Schedule!B5:D").Execute();
            var values = response.Values;
            foreach (var row in values)
                try
                {
                    matches.Add(new Match(int.Parse(row[0].ToString()!),
                        DateTime.ParseExact($"{row[1]} {row[2]}", "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)));
                }
                catch
                {
                    Logger.Log($"Can't parse: {row[0]} {row[1]} {row[2]}", LogLevel.Error, "Sheet");
                }

            return matches;
        }
    }
}