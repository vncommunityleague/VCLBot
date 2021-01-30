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

        public override void Start()
        {
            base.Start();
            Task.Run(OnTask);
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
                    var reminderChan = g.GetChannel(cfg.ReminderChannelId);
                    if (cfg.AutoResult && resultChan != null)
                        try
                        {
                            foreach (var match in SpreadSheet.GetMatches(cfg.SheetsId))
                            {
                                if (db.Results.FirstOrDefault(r =>
                                    r.SheetsId.Equals(cfg.SheetsId) && r.MatchId.Equals(match.Id) &&
                                    r.ServerId == g.Id) != null) continue;
                                var embed = SpreadSheet.GetMatchEmbed(match.Id, cfg.SheetsId);
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

                    //TODO auto reminder
                }
            }
            catch
            {
            }
        }
    }
}