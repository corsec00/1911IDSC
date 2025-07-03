using CompetitionApp.Models.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompetitionApp.Services
{
    public interface ICompetitionService
    {
        Task<IEnumerable<CompetitionEntity>> GetAllCompetitionsAsync();
        Task<CompetitionEntity> GetCompetitionByIdAsync(string id);
        Task<CompetitionEntity> CreateCompetitionAsync(string name, string description, DateTime date);
        Task UpdateCompetitionAsync(CompetitionEntity competition);
        Task DeleteCompetitionAsync(string id);
    }

    public class CompetitionService : ICompetitionService
    {
        private const string TableName = "Competitions";
        private readonly ITableStorageService _tableStorageService;

        public CompetitionService(ITableStorageService tableStorageService)
        {
            _tableStorageService = tableStorageService;
        }

        public async Task<IEnumerable<CompetitionEntity>> GetAllCompetitionsAsync()
        {
            return await _tableStorageService.QueryEntitiesAsync<CompetitionEntity>(
                TableName, 
                $"PartitionKey eq 'Competition'"
            );
        }

        public async Task<CompetitionEntity> GetCompetitionByIdAsync(string id)
        {
            return await _tableStorageService.GetEntityAsync<CompetitionEntity>(
                TableName, 
                "Competition", 
                id
            );
        }

        public async Task<CompetitionEntity> CreateCompetitionAsync(string name, string description, DateTime date)
        {
            var competition = new CompetitionEntity
            {
                Name = name,
                Description = description,
                Date = DateTime.SpecifyKind(date, DateTimeKind.Utc),
                CreatedAt = DateTime.UtcNow
            };

            await _tableStorageService.AddEntityAsync(TableName, competition);
            return competition;
        }

        public async Task UpdateCompetitionAsync(CompetitionEntity competition)
        {
            await _tableStorageService.UpdateEntityAsync(TableName, competition);
        }

        public async Task DeleteCompetitionAsync(string id)
        {
            await _tableStorageService.DeleteEntityAsync<CompetitionEntity>(
                TableName, 
                "Competition", 
                id
            );
        }
    }
}
