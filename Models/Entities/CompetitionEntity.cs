using Azure;
using Azure.Data.Tables;
using System;

namespace CompetitionApp.Models.Entities
{
    public class CompetitionEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = "Competition";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
    }
}
