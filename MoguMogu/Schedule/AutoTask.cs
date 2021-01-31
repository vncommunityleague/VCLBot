using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using MoguMogu.Database;
using MoguMogu.Database.Models;
using MoguMogu.Services;
using MoguMogu.SpreadSheets;

namespace MoguMogu.Schedule
{
    public class AutoTask : IntervalTask
    {
        public AutoTask() : base(TimeSpan.FromMinutes(5))
        {
        }

        protected override void OnTask()
        {
            try
            {
                using var db = new DBContext();
                var discord = StartupService.Discord;
                foreach (var g in discord.Guilds)
                {
                    var cfg = db.Servers.FirstOrDefault(s => s.ServerId == g.Id);
                    if (!cfg.EnableTour || string.IsNullOrEmpty(cfg.SheetsId)) continue;
                    var resultChan = (ISocketMessageChannel) g.GetChannel(cfg.ResultChannelId);
                    var reminderChan = (ISocketMessageChannel) g.GetChannel(cfg.ReminderChannelId);
                    if (cfg.AutoReminder && reminderChan != null)
                        try
                        {
                            foreach (var match in SpreadSheet.GetMatches(cfg.SheetsId))
                            {
                                var diff = Math.Floor((match.DateTime - DateTime.UtcNow.AddHours(cfg.TimeOffset))
                                    .TotalMinutes);
                                if (db.Reminders.FirstOrDefault(r =>
                                    r.SheetsId.Equals(cfg.SheetsId) && r.MatchId.Equals(match.Id) &&
                                    r.ServerId == g.Id) != null || !(diff <= 35) || !(diff >= 0)) continue;
                                var teamA = SpreadSheet.GetTeamMembers(match.TeamA, cfg.SheetsId, db);
                                var teamB = SpreadSheet.GetTeamMembers(match.TeamB, cfg.SheetsId, db);
                                db.Reminders.Add(new Reminder
                                {
                                    SheetsId = cfg.SheetsId,
                                    MatchId = match.Id,
                                    ServerId = g.Id
                                });

                                db.SaveChanges();
                                reminderChan.SendMessageAsync(
                                        $"- **Reminder**: {teamA} (**{match.TeamA}**) vs {teamB} (**{match.TeamB}**) - **Match Id:** `{match.Id}` - **Referee:** {SpreadSheet.ResolveUsername(match.Referee, db)} - **Time:** `{diff}m` left")
                                    .Wait();
                            }
                        }
                        catch
                        {
                        }

                    if (!cfg.AutoResult || resultChan == null) continue;

                    try
                    {
                        foreach (var match in SpreadSheet.GetMatches(cfg.SheetsId))
                        {
                            if (db.Results.FirstOrDefault(r =>
                                r.SheetsId.Equals(cfg.SheetsId) && r.MatchId.Equals(match.Id) &&
                                r.ServerId == g.Id) != null) continue;
                            var embed = SpreadSheet.GetMatchEmbed(match.Id, cfg.SheetsId, true);
                            if (embed == null) continue;
                            resultChan.SendMessageAsync(null, false, embed).Wait();
                            db.Results.Add(new Result
                            {
                                SheetsId = cfg.SheetsId,
                                MatchId = match.Id,
                                ServerId = g.Id
                            });
                            db.SaveChanges();
                        }
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
        }
    }
}