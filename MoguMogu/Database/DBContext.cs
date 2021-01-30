using System;
using Microsoft.EntityFrameworkCore;
using MoguMogu.Database.Models;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace MoguMogu.Database
{
    public sealed class DBContext : DbContext
    {
        public DBContext()
        {
            Database.EnsureCreated();
        }

        public DbSet<Verification> Verification { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Config> Servers { get; set; }
        public DbSet<Result> Results { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured) return;
            optionsBuilder.UseMySql(BotConfig.config.ConnectionString, builder =>
            {
                builder.EnableRetryOnFailure(177013, TimeSpan.FromSeconds(30), null!);
                if (BotConfig.config.UseMariaDB)
                    builder.ServerVersion(new Version(10, 1, 41), ServerType.MariaDb);
            }).EnableSensitiveDataLogging();
            base.OnConfiguring(optionsBuilder);
        }
    }
}