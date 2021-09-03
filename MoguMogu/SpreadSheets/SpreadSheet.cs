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
        private static readonly string[] Scopes =
        {
            SheetsService.Scope.Spreadsheets
        };
        
        private static readonly SheetsService _service;

        static SpreadSheet()
        {
            var result = GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
            {
                ClientId = BotConfig.config.Client_Id,
                ClientSecret = BotConfig.config.Client_Secret
            }, Scopes, "user", CancellationToken.None, new FileDataStore("token.json", true)).Result;
            _service = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = result,
                ApplicationName = "mogu mogu amogus"
            });
        }

        public static Embed GetMatchEmbed(string matchId, string sheetsId, bool check = false)
        {
            try
            {
                var valueRange = _service.Spreadsheets.Values.Get(sheetsId, matchId + "!B2:J").Execute();
                var values = valueRange.Values;
                var value = GetValue(values, 1, 7);
                var embedJson = JsonConvert.DeserializeObject<EmbedJson>(GetValue(values, 2, 7));
                return embedJson != null ? embedJson.Embed.GetEmbed() : null;
            }
            catch (Exception ex)
            {
                Logger.Log(string.Concat("Error when parse sheet: ", matchId, "! (", ex.Message, ")"), LogLevel.Error,
                    "Sheet");
            }

            return null;
        }
        
        private static string GetValue(IList<IList<object>> values, int a, int b)
        {
            string result;
            try
            {
                result = values[a][b].ToString();
            }
            catch
            {
                result = string.Empty;
            }

            return result;
        }

        public static string ResolveUsername(string name, DBContext db)
        {
            var osuClient = Program.OsuClient;
            var oUser = osuClient.GetUserByUsernameAsync(name, GameMode.Standard).Result;
            var flag = oUser == null;
            string result;
            if (flag)
            {
                result = string.Empty;
            }
            else
            {
                var user = db.Users.FirstOrDefault(u => u.OsuId == oUser.UserId);
                result = user == null ? "`" + oUser.Username + "`" : string.Format("<@{0}>", user.DiscordId);
            }

            return result;
        }
        
        public static IEnumerable<Match> GetMatches(string sheetsId)
        {
            var list = new List<Match>();
            try
            {
                var valueRange = _service.Spreadsheets.Values.Get(sheetsId, "Schedule!D4:I").Execute();
                var values = valueRange.Values;
                for (var i = 0; i < values.Count; i++)
                    try
                    {
                        var list2 = values[i];
                        list.Add(new Match(list2[0].ToString(),
                            DateTime.ParseExact(string.Format("{0} {1}", list2[1], list2[2]).Replace("`", ""), "dd/MM/yyyy HH:mm",
                                CultureInfo.InvariantCulture), GetValue(values, i, 3), GetValue(values, i, 4),
                            GetValue(values, i, 5)));
                    }
                    catch (Exception e)
                    {
                        Console.Write(e);
                        Logger.Log(string.Format("Can't parse match id {0}", values[i][0]), LogLevel.Error, "Sheet");
                    }
            }
            catch
            {
            }

            return list;
        }
    }
}