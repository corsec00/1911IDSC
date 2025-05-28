using CompetitionApp.Models;
using CompetitionApp.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompetitionApp.Services
{
    public interface IResultService
    {
        Task<IEnumerable<ResultEntity>> GetResultsByCompetitionIdAsync(string competitionId);
        Task<IEnumerable<ResultEntity>> GetResultsByParticipantIdAsync(string participantId);
        Task<ResultEntity> GetResultAsync(string competitionId, string participantId, int roundNumber);
        Task<ResultEntity> SaveResultAsync(string competitionId, string competitionName, string participantId, string participantName, int roundNumber, Participant result);
        Task DeleteResultAsync(string competitionId, string participantId, int roundNumber);
    }

    public class ResultService : IResultService
    {
        private const string TableName = "Results";
        private readonly ITableStorageService _tableStorageService;

        public ResultService(ITableStorageService tableStorageService)
        {
            _tableStorageService = tableStorageService;
        }

        public async Task<IEnumerable<ResultEntity>> GetResultsByCompetitionIdAsync(string competitionId)
        {
            return await _tableStorageService.QueryEntitiesAsync<ResultEntity>(
                TableName, 
                $"PartitionKey eq '{competitionId}'"
            );
        }

        public async Task<IEnumerable<ResultEntity>> GetResultsByParticipantIdAsync(string participantId)
        {
            // Isso requer uma varredura de tabela com filtro secundário
            var allResults = await _tableStorageService.QueryEntitiesAsync<ResultEntity>(TableName);
            return allResults.Where(r => r.ParticipantId == participantId);
        }

        public async Task<ResultEntity> GetResultAsync(string competitionId, string participantId, int roundNumber)
        {
            string rowKey = $"{participantId}_{roundNumber}";
            return await _tableStorageService.GetEntityAsync<ResultEntity>(
                TableName, 
                competitionId, 
                rowKey
            );
        }

        public async Task<ResultEntity> SaveResultAsync(string competitionId, string competitionName, string participantId, string participantName, int roundNumber, Participant result)
        {
            string rowKey = $"{participantId}_{roundNumber}";
            
            // Verificar se já existe
            var existingResult = await _tableStorageService.GetEntityAsync<ResultEntity>(
                TableName, 
                competitionId, 
                rowKey
            );

            var resultEntity = existingResult ?? new ResultEntity
            {
                PartitionKey = competitionId,
                RowKey = rowKey,
                CompetitionId = competitionId,
                CompetitionName = competitionName,
                ParticipantId = participantId,
                ParticipantName = participantName,
                RoundNumber = roundNumber,
                CreatedAt = DateTime.Now
            };

            // Atualizar propriedades
            resultEntity.TimeInSeconds = result.TimeInSeconds;
            resultEntity.BravoCount = result.BravoCount;
            resultEntity.CharlieCount = result.CharlieCount;
            resultEntity.MissCount = result.MissCount;
            resultEntity.FaltaCount = result.FaltaCount;
            resultEntity.VitimaCount = result.VitimaCount;
            resultEntity.PlateCount = result.PlateCount;
            resultEntity.TotalTime = result.CalculateTotalTime();
            resultEntity.IsEliminated = result.IsEliminated;
            resultEntity.UpdatedAt = DateTime.Now;

            if (existingResult == null)
            {
                await _tableStorageService.AddEntityAsync(TableName, resultEntity);
            }
            else
            {
                await _tableStorageService.UpdateEntityAsync(TableName, resultEntity);
            }

            return resultEntity;
        }

        public async Task DeleteResultAsync(string competitionId, string participantId, int roundNumber)
        {
            string rowKey = $"{participantId}_{roundNumber}";
            await _tableStorageService.DeleteEntityAsync<ResultEntity>(
                TableName, 
                competitionId, 
                rowKey
            );
        }
    }
}
