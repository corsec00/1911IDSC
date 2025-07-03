using Azure;
using Azure.Data.Tables;
using System;

namespace CompetitionApp.Models.Entities
{
    public class FinalResultEntity : ITableEntity
    {
        public string PartitionKey { get; set; } // CompetitionId
        public string RowKey { get; set; } // ParticipantId
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        
        public string ParticipantId { get; set; } = string.Empty;
        public string ParticipantName { get; set; } = string.Empty;
        public string CompetitionId { get; set; } = string.Empty;
        public string CompetitionName { get; set; } = string.Empty;
        public decimal Round1Time { get; set; }
        public decimal Round2Time { get; set; }
        public decimal BestTime { get; set; }
        public int BestRound { get; set; }
        public int Position { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
