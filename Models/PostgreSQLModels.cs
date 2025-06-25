using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompetitionApp.Models
{
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
        public string Description { get; set; } = string.Empty;

        [Column("competition_date")]
        public DateTime CompetitionDate { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<CompetitionParticipant> Participants { get; set; } = new List<CompetitionParticipant>();
        public virtual ICollection<Result> Results { get; set; } = new List<Result>();
        public virtual ICollection<FinalResult> FinalResults { get; set; } = new List<FinalResult>();
    }

    [Table("participants")]
    public class Participant
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
        public string Email { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<CompetitionParticipant> Competitions { get; set; } = new List<CompetitionParticipant>();
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

        // Navigation properties
        [ForeignKey("CompetitionId")]
        public virtual Competition Competition { get; set; } = null!;

        [ForeignKey("ParticipantId")]
        public virtual Participant Participant { get; set; } = null!;
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

        [Column("time_in_seconds")]
        [Precision(10, 3)]
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

        [Column("total_time")]
        [Precision(10, 3)]
        public decimal TotalTime { get; set; }

        [Column("is_eliminated")]
        public bool IsEliminated { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("CompetitionId")]
        public virtual Competition Competition { get; set; } = null!;

        [ForeignKey("ParticipantId")]
        public virtual Participant Participant { get; set; } = null!;
    }

    [Table("final_results")]
    public class FinalResult
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

        [Column("round1_time")]
        [Precision(10, 3)]
        public decimal Round1Time { get; set; }

        [Column("round2_time")]
        [Precision(10, 3)]
        public decimal Round2Time { get; set; }

        [Column("best_time")]
        [Precision(10, 3)]
        public decimal BestTime { get; set; }

        [Column("best_round")]
        public int BestRound { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("CompetitionId")]
        public virtual Competition Competition { get; set; } = null!;

        [ForeignKey("ParticipantId")]
        public virtual Participant Participant { get; set; } = null!;
    }
}

