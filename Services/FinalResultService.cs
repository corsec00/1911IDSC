using CompetitionApp.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompetitionApp.Services
{
    public interface IFinalResultService
    {
        Task<IEnumerable<FinalResultEntity>> GetFinalResultsByCompetitionIdAsync(string competitionId);
        Task<IEnumerable<FinalResultEntity>> GetFinalResultsByParticipantIdAsync(string participantId);
        Task<FinalResultEntity> GetFinalResultAsync(string competitionId, string participantId);
        Task<FinalResultEntity> SaveFinalResultAsync(FinalResultEntity finalResult);
        Task DeleteFinalResultAsync(string competitionId, string participantId);
        Task<IEnumerable<FinalResultEntity>> CalculateAndSaveFinalResultsAsync(string competitionId, string competitionName);
    }

    public class FinalResultService : IFinalResultService
    {
        private const string TableName = "FinalResults";
        private readonly ITableStorageService _tableStorageService;
        private readonly IResultService _resultService;

        public FinalResultService(ITableStorageService tableStorageService, IResultService resultService)
        {
            _tableStorageService = tableStorageService;
            _resultService = resultService;
        }

        public async Task<IEnumerable<FinalResultEntity>> GetFinalResultsByCompetitionIdAsync(string competitionId)
        {
            return await _tableStorageService.QueryEntitiesAsync<FinalResultEntity>(
                TableName, 
                $"PartitionKey eq '{competitionId}'"
            );
        }

        public async Task<IEnumerable<FinalResultEntity>> GetFinalResultsByParticipantIdAsync(string participantId)
        {
            // Isso requer uma varredura de tabela com filtro secundário
            var allResults = await _tableStorageService.QueryEntitiesAsync<FinalResultEntity>(TableName);
            return allResults.Where(r => r.ParticipantId == participantId);
        }

        public async Task<FinalResultEntity> GetFinalResultAsync(string competitionId, string participantId)
        {
            return await _tableStorageService.GetEntityAsync<FinalResultEntity>(
                TableName, 
                competitionId, 
                participantId
            );
        }

        public async Task<FinalResultEntity> SaveFinalResultAsync(FinalResultEntity finalResult)
        {
            var existingResult = await _tableStorageService.GetEntityAsync<FinalResultEntity>(
                TableName, 
                finalResult.PartitionKey, 
                finalResult.RowKey
            );

            if (existingResult == null)
            {
                await _tableStorageService.AddEntityAsync(TableName, finalResult);
            }
            else
            {
                finalResult.UpdatedAt = DateTime.Now;
                await _tableStorageService.UpdateEntityAsync(TableName, finalResult);
            }

            return finalResult;
        }

        public async Task DeleteFinalResultAsync(string competitionId, string participantId)
        {
            await _tableStorageService.DeleteEntityAsync<FinalResultEntity>(
                TableName, 
                competitionId, 
                participantId
            );
        }

        public async Task<IEnumerable<FinalResultEntity>> CalculateAndSaveFinalResultsAsync(string competitionId, string competitionName)
        {
            // Obter todos os resultados da competição
            var results = await _resultService.GetResultsByCompetitionIdAsync(competitionId);
            
            // Agrupar por participante
            var participantResults = results.GroupBy(r => r.ParticipantId);
            
            var finalResults = new List<FinalResultEntity>();
            
            foreach (var group in participantResults)
            {
                var participantId = group.Key;
                var participantName = group.First().ParticipantName;
                
                var round1Result = group.FirstOrDefault(r => r.RoundNumber == 1);
                var round2Result = group.FirstOrDefault(r => r.RoundNumber == 2);
                
                if (round1Result == null && round2Result == null)
                {
                    continue;
                }
                
                decimal round1Time = round1Result?.TotalTime ?? decimal.MaxValue;
                decimal round2Time = round2Result?.TotalTime ?? decimal.MaxValue;
                
                decimal bestTime;
                int bestRound;
                
                if (round1Time <= round2Time)
                {
                    bestTime = round1Time;
                    bestRound = 1;
                }
                else
                {
                    bestTime = round2Time;
                    bestRound = 2;
                }
                
                var finalResult = new FinalResultEntity
                {
                    PartitionKey = competitionId,
                    RowKey = participantId,
                    ParticipantId = participantId,
                    ParticipantName = participantName,
                    CompetitionId = competitionId,
                    CompetitionName = competitionName,
                    Round1Time = round1Time == decimal.MaxValue ? 0 : round1Time,
                    Round2Time = round2Time == decimal.MaxValue ? 0 : round2Time,
                    BestTime = bestTime == decimal.MaxValue ? 0 : bestTime,
                    BestRound = bestRound,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                
                finalResults.Add(finalResult);
            }
            
            // Calcular posições
            int position = 1;
            foreach (var result in finalResults.OrderBy(r => r.BestTime))
            {
                result.Position = position++;
                await SaveFinalResultAsync(result);
            }
            
            return finalResults;
        }
    }
}
