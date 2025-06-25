using CompetitionApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CompetitionApp.Data
{
    public class CompetitionDbContext : DbContext
    {
        public CompetitionDbContext(DbContextOptions<CompetitionDbContext> options) : base(options)
        {
        }

        // DbSets usando os modelos sem conflito
        public DbSet<Competition> Competitions { get; set; }
        public DbSet<ParticipantModel> Participants { get; set; }
        public DbSet<CompetitionParticipant> CompetitionParticipants { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<FinalResultModel> FinalResults { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar Competition
            modelBuilder.Entity<Competition>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.CompetitionDate).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasIndex(e => e.CreatedAt);
            });

            // Configurar ParticipantModel
            modelBuilder.Entity<ParticipantModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Email).HasMaxLength(200);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Configurar CompetitionParticipant
            modelBuilder.Entity<CompetitionParticipant>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RegisteredAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(e => e.Competition)
                    .WithMany(c => c.CompetitionParticipants)
                    .HasForeignKey(e => e.CompetitionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Participant)
                    .WithMany(p => p.CompetitionParticipants)
                    .HasForeignKey(e => e.ParticipantId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.CompetitionId, e.ParticipantId }).IsUnique();
            });

            // Configurar Result
            modelBuilder.Entity<Result>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TimeInSeconds).HasColumnType("decimal(10,3)");
                entity.Property(e => e.TotalTime).HasColumnType("decimal(10,3)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(e => e.Competition)
                    .WithMany(c => c.Results)
                    .HasForeignKey(e => e.CompetitionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Participant)
                    .WithMany(p => p.Results)
                    .HasForeignKey(e => e.ParticipantId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.CompetitionId, e.ParticipantId, e.RoundNumber }).IsUnique();
                entity.HasIndex(e => e.CompetitionId);
            });

            // Configurar FinalResultModel
            modelBuilder.Entity<FinalResultModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Round1Time).HasColumnType("decimal(10,3)");
                entity.Property(e => e.Round2Time).HasColumnType("decimal(10,3)");
                entity.Property(e => e.BestTime).HasColumnType("decimal(10,3)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(e => e.Competition)
                    .WithMany(c => c.FinalResults)
                    .HasForeignKey(e => e.CompetitionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Participant)
                    .WithMany(p => p.FinalResults)
                    .HasForeignKey(e => e.ParticipantId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.CompetitionId, e.ParticipantId }).IsUnique();
                entity.HasIndex(e => e.CompetitionId);
            });
        }
    }
}

