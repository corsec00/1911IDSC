using Azure;
using Azure.Data.Tables;
using System;

namespace CompetitionApp.Models.Entities
{
    public class ResultEntity : ITableEntity
    {
        public string PartitionKey { get; set; } // CompetitionId
        public string RowKey { get; set; } // ParticipantId_RoundNumber
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        
        public string ParticipantId { get; set; } = string.Empty;
        public string ParticipantName { get; set; } = string.Empty;
        public string CompetitionId { get; set; } = string.Empty;
        public string CompetitionName { get; set; } = string.Empty;
        public int RoundNumber { get; set; }
        public decimal TimeInSeconds { get; set; }
        public int BravoCount { get; set; }
        public int CharlieCount { get; set; }
        public int MissCount { get; set; }
        public int FaltaCount { get; set; }
        public int VitimaCount { get; set; }
        public int PlateCount { get; set; }
        public decimal TotalTime { get; set; }
        public bool IsEliminated { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
