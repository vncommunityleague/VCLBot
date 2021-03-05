using System.ComponentModel.DataAnnotations.Schema;

namespace MoguMogu.Database.Models
{
    public class Config
    {
        [Column("id")] public int Id { set; get; }
        [Column("server_id")] public ulong ServerId { set; get; } = 177013;
        [Column("prefix")] public string Prefix { set; get; } = BotConfig.config.BotPrefix;
        [Column("enable_tour")] public bool EnableTour { set; get; } = false;
        [Column("ref_role")] public ulong RefRoleId { set; get; } = 177013;
        [Column("host_role")] public ulong HostRoleId { set; get; } = 177013;
        [Column("sheets_id")] public string SheetsId { set; get; } = string.Empty;
        [Column("verify_role_name")] public string VerifyRoleName { set; get; } = "Verified";
        [Column("auto_result")] public bool AutoResult { set; get; } = false;
        [Column("result_channel_id")] public ulong ResultChannelId { set; get; } = 177013;
        [Column("auto_reminder")] public bool AutoReminder { set; get; } = false;
        [Column("reminder_channel_id")] public ulong ReminderChannelId { set; get; } = 177013;
        [Column("time_offset")] public int TimeOffset { set; get; } = 0;
    }
}