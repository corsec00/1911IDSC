using CompetitionApp.Models.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompetitionApp.Services
{
    public interface IParticipantService
    {
        Task<IEnumerable<ParticipantEntity>> GetAllParticipantsAsync();
        Task<ParticipantEntity> GetParticipantByIdAsync(string id);
        Task<ParticipantEntity> CreateParticipantAsync(string name, string email);
        Task UpdateParticipantAsync(ParticipantEntity participant);
        Task DeleteParticipantAsync(string id);
    }

    public class ParticipantService : IParticipantService
    {
        private const string TableName = "Participants";
        private readonly ITableStorageService _tableStorageService;

        public ParticipantService(ITableStorageService tableStorageService)
        {
            _tableStorageService = tableStorageService;
        }

        public async Task<IEnumerable<ParticipantEntity>> GetAllParticipantsAsync()
        {
            return await _tableStorageService.QueryEntitiesAsync<ParticipantEntity>(
                TableName, 
                $"PartitionKey eq 'Participant'"
            );
        }

        public async Task<ParticipantEntity> GetParticipantByIdAsync(string id)
        {
            return await _tableStorageService.GetEntityAsync<ParticipantEntity>(
                TableName, 
                "Participant", 
                id
            );
        }

        public async Task<ParticipantEntity> CreateParticipantAsync(string name, string email)
        {
            var participant = new ParticipantEntity
            {
                Name = name,
                Email = email,
                CreatedAt = DateTime.Now
            };

            await _tableStorageService.AddEntityAsync(TableName, participant);
            return participant;
        }

        public async Task UpdateParticipantAsync(ParticipantEntity participant)
        {
            await _tableStorageService.UpdateEntityAsync(TableName, participant);
        }

        public async Task DeleteParticipantAsync(string id)
        {
            await _tableStorageService.DeleteEntityAsync<ParticipantEntity>(
                TableName, 
                "Participant", 
                id
            );
        }
    }
}
