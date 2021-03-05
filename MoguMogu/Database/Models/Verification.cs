using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoguMogu.Database.Models
{
    public class Verification
    {
        [Column("id")] public long Id { get; set; }
        [Column("token")] public string Token { get; set; }
        [Column("discord_id")] public ulong DiscordId { get; set; }
        [Column("timestamp")] public DateTime Timestamp { get; set; }
    }
}