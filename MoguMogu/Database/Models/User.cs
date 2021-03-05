using System.ComponentModel.DataAnnotations.Schema;

namespace MoguMogu.Database.Models
{
    public class User
    {
        [Column("id")] public long Id { get; set; }
        [Column("discord_id")] public ulong DiscordId { get; set; }
        [Column("osu_id")] public long OsuId { get; set; }
    }
}