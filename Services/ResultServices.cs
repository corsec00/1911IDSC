using CompetitionApp.Data;
using CompetitionApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CompetitionApp.Services
{
    public class ResultService : IResultService
    {
        private readonly CompetitionDbContext _context;
        private readonly ILogger<ResultService> _logger;

        public ResultService(CompetitionDbContext context, ILogger<ResultService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result> SaveResultAsync(int competitionId, int participantId, int roundNumber, 
            decimal timeInSeconds, int bravoCount, int charlieCount, int missCount, int faultCount, 
            int vitimaCount, int plateCount, decimal totalTime, bool isEliminated)
        {
            try
            {
                _logger.LogInformation("Salvando resultado - Competição: {CompetitionId}, Participante: {ParticipantId}, Rodada: {RoundNumber}", 
                    competitionId, participantId, roundNumber);

                var existingResult = await _context.Results
                    .FirstOrDefaultAsync(r => r.CompetitionId == competitionId && 
                                            r.ParticipantId == participantId && 
                                            r.RoundNumber == roundNumber);

                if (existingResult != null)
                {
                    existingResult.TimeInSeconds = timeInSeconds;
                    existingResult.BravoCount = bravoCount;
                    existingResult.CharlieCount = charlieCount;
                    existingResult.MissCount = missCount;
                    existingResult.FaultCount = faultCount;
                    existingResult.VitimaCount = vitimaCount;
                    existingResult.PlateCount = plateCount;
                    existingResult.TotalTime = totalTime;
                    existingResult.IsEliminated = isEliminated;
                    existingResult.UpdatedAt = DateTime.UtcNow;

                    _context.Results.Update(existingResult);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Resultado atualizado com sucesso. ID: {Id}", existingResult.Id);
                    return existingResult;
                }
                else
                {
                    var result = new Result
                    {
                        CompetitionId = competitionId,
                        ParticipantId = participantId,
                        RoundNumber = roundNumber,
                        TimeInSeconds = timeInSeconds,
                        BravoCount = bravoCount,
                        CharlieCount = charlieCount,
                        MissCount = missCount,
                        FaultCount = faultCount,
                        VitimaCount = vitimaCount,
                        PlateCount = plateCount,
                        TotalTime = totalTime,
                        IsEliminated = isEliminated,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Results.Add(result);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Resultado criado com sucesso. ID: {Id}", result.Id);
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar resultado - Competição: {CompetitionId}, Participante: {ParticipantId}, Rodada: {RoundNumber}", 
                    competitionId, participantId, roundNumber);
                throw;
            }
        }

        public async Task<Result?> GetResultAsync(int competitionId, int participantId, int roundNumber)
        {
            try
            {
                return await _context.Results
                    .Include(r => r.Competition)
                    .Include(r => r.Participant)
                    .FirstOrDefaultAsync(r => r.CompetitionId == competitionId && 
                                            r.ParticipantId == participantId && 
                                            r.RoundNumber == roundNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar resultado - Competição: {CompetitionId}, Participante: {ParticipantId}, Rodada: {RoundNumber}", 
                    competitionId, participantId, roundNumber);
                throw;
            }
        }

        public async Task<IEnumerable<Result>> GetResultsByCompetitionIdAsync(int competitionId)
        {
            try
            {
                _logger.LogInformation("Buscando resultados para competição: {CompetitionId}", competitionId);

                var results = await _context.Results
                    .Where(r => r.CompetitionId == competitionId)
                    .Include(r => r.Participant)
                    .Include(r => r.Competition)
                    .OrderBy(r => r.RoundNumber)
                    .ThenBy(r => r.Participant.Name)
                    .ToListAsync();

                _logger.LogInformation("Encontrados {Count} resultados para competição {CompetitionId}", results.Count, competitionId);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar resultados por competição: {CompetitionId}", competitionId);
                throw;
            }
        }

        public async Task<IEnumerable<Result>> GetResultsByParticipantIdAsync(int participantId)
        {
            try
            {
                return await _context.Results
                    .Where(r => r.ParticipantId == participantId)
                    .Include(r => r.Competition)
                    .Include(r => r.Participant)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar resultados por participante: {ParticipantId}", participantId);
                throw;
            }
        }

        public async Task<IEnumerable<Result>> GetResultsByCompetitionAndRoundAsync(int competitionId, int roundNumber)
        {
            try
            {
                return await _context.Results
                    .Where(r => r.CompetitionId == competitionId && r.RoundNumber == roundNumber)
                    .Include(r => r.Participant)
                    .OrderBy(r => r.IsEliminated)
                    .ThenBy(r => r.TotalTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar resultados por competição e rodada: {CompetitionId}, {RoundNumber}", competitionId, roundNumber);
                throw;
            }
        }

        public async Task DeleteResultAsync(int id)
        {
            try
            {
                var result = await _context.Results.FindAsync(id);
                if (result == null)
                {
                    throw new ArgumentException($"Resultado com ID {id} não encontrado");
                }

                _context.Results.Remove(result);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Resultado excluído com sucesso. ID: {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir resultado ID: {Id}", id);
                throw;
            }
        }

        public async Task DeleteResultsByCompetitionAsync(int competitionId)
        {
            try
            {
                var results = await _context.Results
                    .Where(r => r.CompetitionId == competitionId)
                    .ToListAsync();

                _context.Results.RemoveRange(results);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Excluídos {Count} resultados da competição {CompetitionId}", results.Count, competitionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir resultados da competição: {CompetitionId}", competitionId);
                throw;
            }
        }
    }

    public class FinalResultService : IFinalResultService
    {
        private readonly CompetitionDbContext _context;
        private readonly IResultService _resultService;
        private readonly ILogger<FinalResultService> _logger;

        public FinalResultService(CompetitionDbContext context, IResultService resultService, ILogger<FinalResultService> logger)
        {
            _context = context;
            _resultService = resultService;
            _logger = logger;
        }

        public async Task<IEnumerable<FinalResult>> CalculateAndSaveFinalResultsAsync(int competitionId)
        {
            try
            {
                _logger.LogInformation("Calculando resultados finais para competição: {CompetitionId}", competitionId);

                var results = await _resultService.GetResultsByCompetitionIdAsync(competitionId);
                var resultsList = results.ToList();

                if (!resultsList.Any())
                {
                    _logger.LogWarning("Nenhum resultado encontrado para competição {CompetitionId}", competitionId);
                    return new List<FinalResult>();
                }

                var participantResults = resultsList.GroupBy(r => r.ParticipantId);
                var finalResults = new List<FinalResult>();

                foreach (var group in participantResults)
                {
                    var participantId = group.Key;
                    var participant = group.First().Participant;

                    var round1Result = group.FirstOrDefault(r => r.RoundNumber == 1);
                    var round2Result = group.FirstOrDefault(r => r.RoundNumber == 2);

                    if (round1Result == null && round2Result == null)
                    {
                        continue;
                    }

                    decimal round1Time = round1Result?.IsEliminated == true ? decimal.MaxValue : (round1Result?.TotalTime ?? decimal.MaxValue);
                    decimal round2Time = round2Result?.IsEliminated == true ? decimal.MaxValue : (round2Result?.TotalTime ?? decimal.MaxValue);

                    decimal bestTime;
                    int bestRound;

                    if (round1Time == decimal.MaxValue && round2Time == decimal.MaxValue)
                    {
                        bestTime = 0;
                        bestRound = 0;
                    }
                    else if (round1Time <= round2Time)
                    {
                        bestTime = round1Time == decimal.MaxValue ? 0 : round1Time;
                        bestRound = 1;
                    }
                    else
                    {
                        bestTime = round2Time == decimal.MaxValue ? 0 : round2Time;
                        bestRound = 2;
                    }

                    var existingFinalResult = await _context.FinalResults
                        .FirstOrDefaultAsync(fr => fr.CompetitionId == competitionId && fr.ParticipantId == participantId);

                    if (existingFinalResult != null)
                    {
                        existingFinalResult.Round1Time = round1Time == decimal.MaxValue ? 0 : round1Time;
                        existingFinalResult.Round2Time = round2Time == decimal.MaxValue ? 0 : round2Time;
                        existingFinalResult.BestTime = bestTime;
                        existingFinalResult.BestRound = bestRound;
                        existingFinalResult.UpdatedAt = DateTime.UtcNow;

                        finalResults.Add(existingFinalResult);
                    }
                    else
                    {
                        var finalResult = new FinalResult
                        {
                            CompetitionId = competitionId,
                            ParticipantId = participantId,
                            Round1Time = round1Time == decimal.MaxValue ? 0 : round1Time,
                            Round2Time = round2Time == decimal.MaxValue ? 0 : round2Time,
                            BestTime = bestTime,
                            BestRound = bestRound,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        _context.FinalResults.Add(finalResult);
                        finalResults.Add(finalResult);
                    }
                }

                var sortedResults = finalResults
                    .OrderBy(r => r.BestTime == 0)
                    .ThenBy(r => r.BestTime)
                    .ToList();

                int position = 1;
                foreach (var result in sortedResults)
                {
                    result.Position = position++;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Calculados e salvos {Count} resultados finais para competição {CompetitionId}", finalResults.Count, competitionId);
                return finalResults;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular resultados finais para competição: {CompetitionId}", competitionId);
                throw;
            }
        }

        public async Task<FinalResult?> GetFinalResultAsync(int competitionId, int participantId)
        {
            try
            {
                return await _context.FinalResults
                    .Include(fr => fr.Competition)
                    .Include(fr => fr.Participant)
                    .FirstOrDefaultAsync(fr => fr.CompetitionId == competitionId && fr.ParticipantId == participantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar resultado final - Competição: {CompetitionId}, Participante: {ParticipantId}", competitionId, participantId);
                throw;
            }
        }

        public async Task<IEnumerable<FinalResult>> GetFinalResultsByCompetitionIdAsync(int competitionId)
        {
            try
            {
                _logger.LogInformation("Buscando resultados finais para competição: {CompetitionId}", competitionId);

                var finalResults = await _context.FinalResults
                    .Where(fr => fr.CompetitionId == competitionId)
                    .Include(fr => fr.Participant)
                    .OrderBy(fr => fr.Position)
                    .ToListAsync();

                _logger.LogInformation("Encontrados {Count} resultados finais para competição {CompetitionId}", finalResults.Count, competitionId);
                return finalResults;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar resultados finais por competição: {CompetitionId}", competitionId);
                throw;
            }
        }

        public async Task DeleteFinalResultAsync(int id)
        {
            try
            {
                var finalResult = await _context.FinalResults.FindAsync(id);
                if (finalResult == null)
                {
                    throw new ArgumentException($"Resultado final com ID {id} não encontrado");
                }

                _context.FinalResults.Remove(finalResult);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Resultado final excluído com sucesso. ID: {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir resultado final ID: {Id}", id);
                throw;
            }
        }

        public async Task DeleteFinalResultsByCompetitionAsync(int competitionId)
        {
            try
            {
                var finalResults = await _context.FinalResults
                    .Where(fr => fr.CompetitionId == competitionId)
                    .ToListAsync();

                _context.FinalResults.RemoveRange(finalResults);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Excluídos {Count} resultados finais da competição {CompetitionId}", finalResults.Count, competitionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir resultados finais da competição: {CompetitionId}", competitionId);
                throw;
            }
        }
    }
}

