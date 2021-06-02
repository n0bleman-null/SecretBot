using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace SecretHitlerBot
{
    public partial class SechitContext : DbContext
    {
        public SechitContext()
        {
        }

        public SechitContext(DbContextOptions<SechitContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ChatDB> Chats { get; set; }
        public virtual DbSet<GameDB> Games { get; set; }
        public virtual DbSet<PlayerDB> Players { get; set; }
        public virtual DbSet<PlayergameDb> Playergames { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // lazy proxies?
                optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=sechit;Username=tim;Password=pass");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "ru_RU.UTF-8");

            modelBuilder.Entity<ChatDB>(entity =>
            {
                entity.ToTable("chat");

                entity.HasIndex(e => e.ChatId, "chat_chatid_key")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.ChatId).HasColumnName("chatid");
            });

            modelBuilder.Entity<GameDB>(entity =>
            {
                entity.ToTable("game");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.ChatId).HasColumnName("chatid");

                entity.Property(e => e.Winner)
                    .IsRequired()
                    .HasColumnType("boolean")
                    .HasColumnName("winner");

                entity.HasOne(d => d.Chat)
                    .WithMany(p => p.Games)
                    .HasForeignKey(d => d.ChatId)
                    .HasConstraintName("game_chatid_fkey");
            });

            modelBuilder.Entity<PlayerDB>(entity =>
            {
                entity.ToTable("player");

                entity.HasIndex(e => e.PlayerId, "player_playerid_key")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.PlayerId).HasColumnName("playerid");
            });

            modelBuilder.Entity<PlayergameDb>(entity =>
            {
                entity.ToTable("playergame");

                entity.HasIndex(e => new {Playerid = e.PlayerId, Gameid = e.GameId }, "oneplayerpergame")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.GameId).HasColumnName("gameid");

                entity.Property(e => e.PlayerId).HasColumnName("playerid");

                entity.Property(e => e.Role)
                    .IsRequired()
                    .HasColumnType("boolean")
                    .HasColumnName("role");

                entity.HasOne(d => d.Game)
                    .WithMany(p => p.Playergames)
                    .HasForeignKey(d => d.GameId)
                    .HasConstraintName("playergame_gameid_fkey");

                entity.HasOne(d => d.Player)
                    .WithMany(p => p.Playergames)
                    .HasForeignKey(d => d.PlayerId)
                    .HasConstraintName("playergame_playerid_fkey");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
