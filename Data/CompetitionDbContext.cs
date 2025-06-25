using Microsoft.EntityFrameworkCore;
using CompetitionApp.Models;

namespace CompetitionApp.Data
{
    public class CompetitionDbContext : DbContext
    {
        public CompetitionDbContext(DbContextOptions<CompetitionDbContext> options) : base(options)
        {
        }

        public DbSet<Competition> Competitions { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<CompetitionParticipant> CompetitionParticipants { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<FinalResult> FinalResults { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurações específicas para PostgreSQL
            modelBuilder.HasDefaultSchema("public");

            // Configurar índices únicos
            modelBuilder.Entity<CompetitionParticipant>()
                .HasIndex(cp => new { cp.CompetitionId, cp.ParticipantId })
                .IsUnique()
                .HasDatabaseName("ix_competition_participants_unique");

            modelBuilder.Entity<Result>()
                .HasIndex(r => new { r.CompetitionId, r.ParticipantId, r.RoundNumber })
                .IsUnique()
                .HasDatabaseName("ix_results_unique");

            modelBuilder.Entity<FinalResult>()
                .HasIndex(fr => new { fr.CompetitionId, fr.ParticipantId })
                .IsUnique()
                .HasDatabaseName("ix_final_results_unique");

            // Configurar índices para performance
            modelBuilder.Entity<Competition>()
                .HasIndex(c => c.CreatedAt)
                .HasDatabaseName("ix_competitions_created_at");

            modelBuilder.Entity<Result>()
                .HasIndex(r => r.CompetitionId)
                .HasDatabaseName("ix_results_competition_id");

            modelBuilder.Entity<FinalResult>()
                .HasIndex(fr => fr.CompetitionId)
                .HasDatabaseName("ix_final_results_competition_id");

            // Configurar relacionamentos
            modelBuilder.Entity<CompetitionParticipant>()
                .HasOne(cp => cp.Competition)
                .WithMany(c => c.Participants)
                .HasForeignKey(cp => cp.CompetitionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CompetitionParticipant>()
                .HasOne(cp => cp.Participant)
                .WithMany(p => p.Competitions)
                .HasForeignKey(cp => cp.ParticipantId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Result>()
                .HasOne(r => r.Competition)
                .WithMany(c => c.Results)
                .HasForeignKey(r => r.CompetitionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Result>()
                .HasOne(r => r.Participant)
                .WithMany(p => p.Results)
                .HasForeignKey(r => r.ParticipantId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FinalResult>()
                .HasOne(fr => fr.Competition)
                .WithMany(c => c.FinalResults)
                .HasForeignKey(fr => fr.CompetitionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FinalResult>()
                .HasOne(fr => fr.Participant)
                .WithMany(p => p.FinalResults)
                .HasForeignKey(fr => fr.ParticipantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configurar valores padrão
            modelBuilder.Entity<Competition>()
                .Property(c => c.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<Competition>()
                .Property(c => c.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<Participant>()
                .Property(p => p.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<Participant>()
                .Property(p => p.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<CompetitionParticipant>()
                .Property(cp => cp.RegisteredAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<Result>()
                .Property(r => r.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<Result>()
                .Property(r => r.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<FinalResult>()
                .Property(fr => fr.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<FinalResult>()
                .Property(fr => fr.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity is Competition competition)
                {
                    competition.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.Entity is Participant participant)
                {
                    participant.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.Entity is Result result)
                {
                    result.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.Entity is FinalResult finalResult)
                {
                    finalResult.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
    }
}

