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
            try
            {
                var results = await _tableStorageService.QueryEntitiesAsync<FinalResultEntity>(
                    TableName, 
                    $"PartitionKey eq '{competitionId}'"
                );
                Console.WriteLine($"FinalResultService: Encontrados {results.Count()} resultados finais para competição {competitionId}");
                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar resultados finais: {ex.Message}");
                return new List<FinalResultEntity>();
            }
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
            // Verificar se já existe
            var existingResult = await _tableStorageService.GetEntityAsync<FinalResultEntity>(
                TableName, 
                finalResult.PartitionKey, 
                finalResult.RowKey
            );

            if (existingResult == null)
            {
                // Garantir que as datas estejam em UTC
                finalResult.CreatedAt = DateTime.SpecifyKind(finalResult.CreatedAt, DateTimeKind.Utc);
                finalResult.UpdatedAt = DateTime.SpecifyKind(finalResult.UpdatedAt, DateTimeKind.Utc);
                
                await _tableStorageService.AddEntityAsync(TableName, finalResult);
                Console.WriteLine($"Resultado final adicionado para {finalResult.ParticipantName}");
            }
            else
            {
                // Atualizar propriedades
                existingResult.Round1Time = finalResult.Round1Time;
                existingResult.Round2Time = finalResult.Round2Time;
                existingResult.BestTime = finalResult.BestTime;
                existingResult.BestRound = finalResult.BestRound;
                existingResult.Position = finalResult.Position;
                existingResult.UpdatedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                
                await _tableStorageService.UpdateEntityAsync(TableName, existingResult);
                Console.WriteLine($"Resultado final atualizado para {finalResult.ParticipantName}");
                finalResult = existingResult;
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
            try
            {
                Console.WriteLine($"Calculando resultados finais para competição: {competitionId}");
                
                // Obter todos os resultados da competição
                var results = await _resultService.GetResultsByCompetitionIdAsync(competitionId);
                Console.WriteLine($"Encontrados {results.Count()} resultados de rodadas");
                
                if (!results.Any())
                {
                    Console.WriteLine("Nenhum resultado de rodada encontrado para calcular resultados finais");
                    return new List<FinalResultEntity>();
                }
                
                // Agrupar por participante
                var participantResults = results.GroupBy(r => r.ParticipantId);
                
                var finalResults = new List<FinalResultEntity>();
                
                foreach (var group in participantResults)
                {
                    var participantId = group.Key;
                    var participantName = group.First().ParticipantName;
                    
                    var round1Result = group.FirstOrDefault(r => r.RoundNumber == 1);
                    var round2Result = group.FirstOrDefault(r => r.RoundNumber == 2);
                    
                    Console.WriteLine($"Processando participante: {participantName}");
                    Console.WriteLine($"  Rodada 1: {(round1Result != null ? $"{round1Result.TotalTime:F2}s" : "N/A")}");
                    Console.WriteLine($"  Rodada 2: {(round2Result != null ? $"{round2Result.TotalTime:F2}s" : "N/A")}");
                    
                    if (round1Result == null && round2Result == null)
                    {
                        continue;
                    }
                    
                    // Considerar participantes eliminados
                    decimal round1Time = (round1Result?.IsEliminated == true) ? decimal.MaxValue : (round1Result?.TotalTime ?? decimal.MaxValue);
                    decimal round2Time = (round2Result?.IsEliminated == true) ? decimal.MaxValue : (round2Result?.TotalTime ?? decimal.MaxValue);
                    
                    decimal bestTime;
                    int bestRound;
                    
                    if (round1Time == decimal.MaxValue && round2Time == decimal.MaxValue)
                    {
                        // Ambas as rodadas eliminadas
                        bestTime = decimal.MaxValue;
                        bestRound = 0;
                    }
                    else if (round1Time <= round2Time)
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
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    
                    finalResults.Add(finalResult);
                }
                
                // Calcular posições - participantes não eliminados primeiro, depois por melhor tempo
                var sortedResults = finalResults
                    .OrderBy(r => r.BestTime == 0) // Eliminados por último
                    .ThenBy(r => r.BestTime)
                    .ToList();
                
                int position = 1;
                foreach (var result in sortedResults)
                {
                    result.Position = position++;
                    await SaveFinalResultAsync(result);
                }
                
                Console.WriteLine($"Salvos {finalResults.Count} resultados finais");
                return finalResults;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao calcular e salvar resultados finais: {ex.Message}");
                throw;
            }
        }
    }
}

