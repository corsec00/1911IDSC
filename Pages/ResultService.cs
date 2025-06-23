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
            Console.WriteLine("ResultService: Serviço inicializado");
        }

        public async Task<IEnumerable<ResultEntity>> GetResultsByCompetitionIdAsync(string competitionId)
        {
            try
            {
                Console.WriteLine($"ResultService: Buscando resultados para competição ID: {competitionId}");
                
                var filter = $"PartitionKey eq '{competitionId}'";
                Console.WriteLine($"ResultService: Filtro aplicado: {filter}");
                
                var results = await _tableStorageService.QueryEntitiesAsync<ResultEntity>(TableName, filter);
                var resultsList = results.ToList();
                
                Console.WriteLine($"ResultService: Encontrados {resultsList.Count} resultados para competição {competitionId}");
                
                // Log detalhado dos resultados encontrados
                foreach (var result in resultsList)
                {
                    Console.WriteLine($"ResultService: Resultado encontrado - Participante: {result.ParticipantName}, Rodada: {result.RoundNumber}, Tempo: {result.TotalTime:F2}s, Eliminado: {result.IsEliminated}");
                }
                
                return resultsList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ResultService: ERRO ao buscar resultados para competição {competitionId}: {ex.Message}");
                Console.WriteLine($"ResultService: Stack trace: {ex.StackTrace}");
                return new List<ResultEntity>();
            }
        }

        public async Task<IEnumerable<ResultEntity>> GetResultsByParticipantIdAsync(string participantId)
        {
            try
            {
                Console.WriteLine($"ResultService: Buscando resultados para participante ID: {participantId}");
                
                // Isso requer uma varredura de tabela com filtro secundário
                var allResults = await _tableStorageService.QueryEntitiesAsync<ResultEntity>(TableName);
                var participantResults = allResults.Where(r => r.ParticipantId == participantId).ToList();
                
                Console.WriteLine($"ResultService: Encontrados {participantResults.Count} resultados para participante {participantId}");
                return participantResults;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ResultService: ERRO ao buscar resultados para participante {participantId}: {ex.Message}");
                return new List<ResultEntity>();
            }
        }

        public async Task<ResultEntity> GetResultAsync(string competitionId, string participantId, int roundNumber)
        {
            try
            {
                string rowKey = $"{participantId}_{roundNumber}";
                Console.WriteLine($"ResultService: Buscando resultado específico - Competição: {competitionId}, Participante: {participantId}, Rodada: {roundNumber}, RowKey: {rowKey}");
                
                var result = await _tableStorageService.GetEntityAsync<ResultEntity>(TableName, competitionId, rowKey);
                
                if (result != null)
                {
                    Console.WriteLine($"ResultService: Resultado específico encontrado - {result.ParticipantName}, Rodada {result.RoundNumber}");
                }
                else
                {
                    Console.WriteLine($"ResultService: Resultado específico NÃO encontrado - Competição: {competitionId}, RowKey: {rowKey}");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ResultService: ERRO ao buscar resultado específico: {ex.Message}");
                return null;
            }
        }

        public async Task<ResultEntity> SaveResultAsync(string competitionId, string competitionName, string participantId, string participantName, int roundNumber, Participant result)
        {
            try
            {
                string rowKey = $"{participantId}_{roundNumber}";
                Console.WriteLine($"ResultService: Salvando resultado - Competição: {competitionId}, Participante: {participantName}, Rodada: {roundNumber}");
                Console.WriteLine($"ResultService: Dados do resultado - Tempo: {result.TimeInSeconds:F2}s, Total: {result.CalculateTotalTime():F2}s, Eliminado: {result.IsEliminated}");
                
                // Verificar se já existe
                var existingResult = await _tableStorageService.GetEntityAsync<ResultEntity>(TableName, competitionId, rowKey);

                var resultEntity = existingResult ?? new ResultEntity
                {
                    PartitionKey = competitionId,
                    RowKey = rowKey,
                    CompetitionId = competitionId,
                    CompetitionName = competitionName,
                    ParticipantId = participantId,
                    ParticipantName = participantName,
                    RoundNumber = roundNumber,
                    CreatedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc)
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
                resultEntity.UpdatedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);

                if (existingResult == null)
                {
                    Console.WriteLine($"ResultService: Adicionando novo resultado para {participantName}, Rodada {roundNumber}");
                    await _tableStorageService.AddEntityAsync(TableName, resultEntity);
                    Console.WriteLine($"ResultService: Resultado adicionado com sucesso");
                }
                else
                {
                    Console.WriteLine($"ResultService: Atualizando resultado existente para {participantName}, Rodada {roundNumber}");
                    await _tableStorageService.UpdateEntityAsync(TableName, resultEntity);
                    Console.WriteLine($"ResultService: Resultado atualizado com sucesso");
                }

                return resultEntity;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ResultService: ERRO ao salvar resultado para {participantName}, Rodada {roundNumber}: {ex.Message}");
                Console.WriteLine($"ResultService: Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task DeleteResultAsync(string competitionId, string participantId, int roundNumber)
        {
            try
            {
                string rowKey = $"{participantId}_{roundNumber}";
                Console.WriteLine($"ResultService: Excluindo resultado - Competição: {competitionId}, Participante: {participantId}, Rodada: {roundNumber}");
                
                await _tableStorageService.DeleteEntityAsync<ResultEntity>(TableName, competitionId, rowKey);
                Console.WriteLine($"ResultService: Resultado excluído com sucesso");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ResultService: ERRO ao excluir resultado: {ex.Message}");
                throw;
            }
        }
    }
}

