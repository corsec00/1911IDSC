using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CompetitionApp.Models
{
    // Modelos para PostgreSQL - sem conflitos com modelos existentes
    
    [Table("competitions")]
    public class Competition
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        [Column("description")]
        public string? Description { get; set; }

        [Column("competition_date")]
        public DateTime CompetitionDate { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Relacionamentos
        public virtual ICollection<CompetitionParticipant> CompetitionParticipants { get; set; } = new List<CompetitionParticipant>();
        public virtual ICollection<Result> Results { get; set; } = new List<Result>();
        public virtual ICollection<FinalResult> FinalResults { get; set; } = new List<FinalResult>();
    }

    [Table("participants")]
    public class ParticipantModel
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        [Column("email")]
        public string? Email { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Relacionamentos
        public virtual ICollection<CompetitionParticipant> CompetitionParticipants { get; set; } = new List<CompetitionParticipant>();
        public virtual ICollection<Result> Results { get; set; } = new List<Result>();
        public virtual ICollection<FinalResult> FinalResults { get; set; } = new List<FinalResult>();
    }

    [Table("competition_participants")]
    public class CompetitionParticipant
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("competition_id")]
        public int CompetitionId { get; set; }

        [Column("participant_id")]
        public int ParticipantId { get; set; }

        [Column("registered_at")]
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        // Relacionamentos
        [ForeignKey("CompetitionId")]
        public virtual Competition Competition { get; set; } = null!;

        [ForeignKey("ParticipantId")]
        public virtual ParticipantModel Participant { get; set; } = null!;
    }

    [Table("results")]
    public class Result
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("competition_id")]
        public int CompetitionId { get; set; }

        [Column("participant_id")]
        public int ParticipantId { get; set; }

        [Column("round_number")]
        public int RoundNumber { get; set; }

        [Column("time_in_seconds", TypeName = "decimal(10,3)")]
        public decimal TimeInSeconds { get; set; }

        [Column("bravo_count")]
        public int BravoCount { get; set; }

        [Column("charlie_count")]
        public int CharlieCount { get; set; }

        [Column("miss_count")]
        public int MissCount { get; set; }

        [Column("fault_count")]
        public int FaultCount { get; set; }

        [Column("vitima_count")]
        public int VitimaCount { get; set; }

        [Column("plate_count")]
        public int PlateCount { get; set; }

        [Column("total_time", TypeName = "decimal(10,3)")]
        public decimal TotalTime { get; set; }

        [Column("is_eliminated")]
        public bool IsEliminated { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Relacionamentos
        [ForeignKey("CompetitionId")]
        public virtual Competition Competition { get; set; } = null!;

        [ForeignKey("ParticipantId")]
        public virtual ParticipantModel Participant { get; set; } = null!;
    }

    [Table("final_results")]
    public class FinalResultModel
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("competition_id")]
        public int CompetitionId { get; set; }

        [Column("participant_id")]
        public int ParticipantId { get; set; }

        [Column("position")]
        public int Position { get; set; }

        [Column("round1_time", TypeName = "decimal(10,3)")]
        public decimal Round1Time { get; set; }

        [Column("round2_time", TypeName = "decimal(10,3)")]
        public decimal Round2Time { get; set; }

        [Column("best_time", TypeName = "decimal(10,3)")]
        public decimal BestTime { get; set; }

        [Column("best_round")]
        public int BestRound { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Relacionamentos
        [ForeignKey("CompetitionId")]
        public virtual Competition Competition { get; set; } = null!;

        [ForeignKey("ParticipantId")]
        public virtual ParticipantModel Participant { get; set; } = null!;
    }
}

