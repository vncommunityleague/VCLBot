using System.ComponentModel.DataAnnotations.Schema;

namespace MoguMogu.Database.Models
{
    public class Reminder
    {
        [Column("id")] public long Id { get; set; }
        [Column("match_id")] public string MatchId { get; set; }
        [Column("server_id")] public ulong ServerId { get; set; }
        [Column("sheets_id")] public string SheetsId { get; set; }
    }
}