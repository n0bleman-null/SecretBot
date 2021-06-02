using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace SecretHitlerBot
{
    public partial class sechitContext : DbContext
    {
        public sechitContext()
        {
        }

        public sechitContext(DbContextOptions<sechitContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ChatDB> Chats { get; set; }
        public virtual DbSet<GameDB> Games { get; set; }
        public virtual DbSet<PlayerDB> Players { get; set; }
        public virtual DbSet<PlayergameDB> Playergames { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=sechit;Username=tim;Password=pass");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "ru_RU.UTF-8");

            modelBuilder.Entity<ChatDB>(entity =>
            {
                entity.ToTable("chat");

                entity.HasIndex(e => e.Chatid, "chat_chatid_key")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.Chatid).HasColumnName("chatid");
            });

            modelBuilder.Entity<GameDB>(entity =>
            {
                entity.ToTable("game");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.Chatid).HasColumnName("chatid");

                entity.Property(e => e.Winner)
                    .IsRequired()
                    .HasColumnType("bit(1)")
                    .HasColumnName("winner");

                entity.HasOne(d => d.Chat)
                    .WithMany(p => p.Games)
                    .HasForeignKey(d => d.Chatid)
                    .HasConstraintName("game_chatid_fkey");
            });

            modelBuilder.Entity<PlayerDB>(entity =>
            {
                entity.ToTable("player");

                entity.HasIndex(e => e.Playerid, "player_playerid_key")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.Playerid).HasColumnName("playerid");
            });

            modelBuilder.Entity<PlayergameDB>(entity =>
            {
                entity.ToTable("playergame");

                entity.HasIndex(e => new { e.Playerid, e.Gameid }, "oneplayerpergame")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.Gameid).HasColumnName("gameid");

                entity.Property(e => e.Playerid).HasColumnName("playerid");

                entity.Property(e => e.Role)
                    .IsRequired()
                    .HasColumnType("bit(1)")
                    .HasColumnName("role");

                entity.HasOne(d => d.Game)
                    .WithMany(p => p.Playergames)
                    .HasForeignKey(d => d.Gameid)
                    .HasConstraintName("playergame_gameid_fkey");

                entity.HasOne(d => d.Player)
                    .WithMany(p => p.Playergames)
                    .HasForeignKey(d => d.Playerid)
                    .HasConstraintName("playergame_playerid_fkey");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
