using System.ComponentModel.DataAnnotations.Schema;

namespace MoguMogu.Database.Models
{
    public class Result
    {
        [Column("id")] public long Id { get; set; }
        [Column("match_id")] public ulong MatchId { get; set; }
        [Column("sheets_id")] public long SheetsId { get; set; }
    }
}