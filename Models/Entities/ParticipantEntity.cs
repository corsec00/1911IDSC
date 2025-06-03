using Azure;
using Azure.Data.Tables;
using System;

namespace CompetitionApp.Models.Entities
{
    public class ParticipantEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = "Participant";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
