using CompetitionApp.Models;
using CompetitionApp.Models.Entities;
using CompetitionApp.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CompetitionApp.Managers
{
    public interface ICompetitionManager
    {
        // Métodos de gerenciamento de competição atual
        Task<string> GetCurrentCompetitionIdAsync();
        Task SetCurrentCompetitionIdAsync(string competitionId);
        Task<CompetitionEntity> GetCurrentCompetitionAsync();
        Task<CompetitionEntity> CreateCompetitionAsync(string name, string description);
        
        // Métodos de gerenciamento de participantes
        Task<List<Participant>> GetParticipantsAsync();
        Task SaveParticipantsAsync(List<Participant> participants);
        Task<ParticipantEntity> RegisterParticipantAsync(string name, string email = "");
        
        // Métodos de gerenciamento de resultados
        Task<List<Participant>> GetRound1ResultsAsync();
        Task<List<Participant>> GetRound2ResultsAsync();
        Task SaveRound1ResultAsync(Participant participant);
        Task SaveRound2ResultAsync(Participant participant);
        Task RemoveRound1ResultAsync(string participantName);
        Task RemoveRound2ResultAsync(string participantName);
        
        // Métodos de cálculo de resultados finais
        Task<List<FinalResult>> CalculateFinalResultsAsync();
        Task SaveFinalResultsAsync(List<FinalResult> finalResults);
        
        // Métodos de histórico
        Task<IEnumerable<CompetitionEntity>> GetCompetitionHistoryAsync();
        Task<IEnumerable<ResultEntity>> GetParticipantHistoryAsync(string participantName);
    }

    public class CompetitionManager : ICompetitionManager
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICompetitionService _competitionService;
        private readonly IParticipantService _participantService;
        private readonly IResultService _resultService;
        private readonly IFinalResultService _finalResultService;
        
        private const string CurrentCompetitionIdKey = "CurrentCompetitionId";
        private const string ParticipantsKey = "Participants";
        private const string Round1ResultsKey = "Round1Results";
        private const string Round2ResultsKey = "Round2Results";
        
        public CompetitionManager(
            IHttpContextAccessor httpContextAccessor,
            ICompetitionService competitionService,
            IParticipantService participantService,
            IResultService resultService,
            IFinalResultService finalResultService)
        {
            _httpContextAccessor = httpContextAccessor;
            _competitionService = competitionService;
            _participantService = participantService;
            _resultService = resultService;
            _finalResultService = finalResultService;
        }
        
        private HttpContext HttpContext => _httpContextAccessor.HttpContext;
        
        // Métodos auxiliares para sessão
        private T GetFromSession<T>(string key) where T : class
        {
            var json = HttpContext.Session.GetString(key);
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }
            return JsonSerializer.Deserialize<T>(json);
        }
        
        private void SetInSession<T>(string key, T value) where T : class
        {
            var json = JsonSerializer.Serialize(value);
            HttpContext.Session.SetString(key, json);
        }
        
        // Implementação dos métodos de gerenciamento de competição atual
        public async Task<string> GetCurrentCompetitionIdAsync()
        {
            var competitionId = HttpContext.Session.GetString(CurrentCompetitionIdKey);
            if (string.IsNullOrEmpty(competitionId))
            {
                // Criar uma nova competição se não existir
                var competition = await CreateCompetitionAsync("Competição " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"), "Competição criada automaticamente");
                competitionId = competition.RowKey;
                HttpContext.Session.SetString(CurrentCompetitionIdKey, competitionId);
            }
            return competitionId;
        }
        
        public async Task SetCurrentCompetitionIdAsync(string competitionId)
        {
            HttpContext.Session.SetString(CurrentCompetitionIdKey, competitionId);
        }
        
        public async Task<CompetitionEntity> GetCurrentCompetitionAsync()
        {
            var competitionId = await GetCurrentCompetitionIdAsync();
            return await _competitionService.GetCompetitionByIdAsync(competitionId);
        }
        
        public async Task<CompetitionEntity> CreateCompetitionAsync(string name, string description)
        {
            var competition = await _competitionService.CreateCompetitionAsync(name, description, DateTime.Now);
            await SetCurrentCompetitionIdAsync(competition.RowKey);
            return competition;
        }
        
        // Implementação dos métodos de gerenciamento de participantes
        public async Task<List<Participant>> GetParticipantsAsync()
        {
            var participants = GetFromSession<List<Participant>>(ParticipantsKey) ?? new List<Participant>();
            return participants;
        }
        
        public async Task SaveParticipantsAsync(List<Participant> participants)
        {
            SetInSession(ParticipantsKey, participants);
            
            // Salvar participantes no Azure Table Storage
            var competitionId = await GetCurrentCompetitionIdAsync();
            var competition = await GetCurrentCompetitionAsync();
            
            foreach (var participant in participants)
            {
                // Registrar participante se não existir
                var participantEntity = await _participantService.CreateParticipantAsync(participant.Name, "");
            }
        }
        
        public async Task<ParticipantEntity> RegisterParticipantAsync(string name, string email = "")
        {
            // Adicionar à lista de participantes da sessão
            var participants = await GetParticipantsAsync();
            if (!participants.Any(p => p.Name == name))
            {
                participants.Add(new Participant { Name = name });
                await SaveParticipantsAsync(participants);
            }
            
            // Registrar no Azure Table Storage
            return await _participantService.CreateParticipantAsync(name, email);
        }
        
        // Implementação dos métodos de gerenciamento de resultados
        public async Task<List<Participant>> GetRound1ResultsAsync()
        {
            var results = GetFromSession<List<Participant>>(Round1ResultsKey) ?? new List<Participant>();
            return results;
        }
        
        public async Task<List<Participant>> GetRound2ResultsAsync()
        {
            var results = GetFromSession<List<Participant>>(Round2ResultsKey) ?? new List<Participant>();
            return results;
        }
        
        public async Task SaveRound1ResultAsync(Participant participant)
        {
            // Salvar na sessão
            var results = await GetRound1ResultsAsync();
            var existingResult = results.FirstOrDefault(r => r.Name == participant.Name);
            if (existingResult != null)
            {
                results.Remove(existingResult);
            }
            results.Add(participant);
            SetInSession(Round1ResultsKey, results);
            
            try
            {
                // Salvar no Azure Table Storage
                var competitionId = await GetCurrentCompetitionIdAsync();
                var competition = await GetCurrentCompetitionAsync();
                
                // Registrar participante se não existir
                var participantEntity = await _participantService.CreateParticipantAsync(participant.Name, "");
                
                await _resultService.SaveResultAsync(
                    competitionId,
                    competition.Name,
                    participantEntity.RowKey,
                    participant.Name,
                    1, // Round 1
                    participant
                );
                
                Console.WriteLine($"Resultado da Rodada 1 salvo para {participant.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao salvar resultado da Rodada 1 para {participant.Name}: {ex.Message}");
                // Continua a execução mesmo com erro no Azure Storage
            }
        }
        
        public async Task SaveRound2ResultAsync(Participant participant)
        {
            // Salvar na sessão
            var results = await GetRound2ResultsAsync();
            var existingResult = results.FirstOrDefault(r => r.Name == participant.Name);
            if (existingResult != null)
            {
                results.Remove(existingResult);
            }
            results.Add(participant);
            SetInSession(Round2ResultsKey, results);
            
            try
            {
                // Salvar no Azure Table Storage
                var competitionId = await GetCurrentCompetitionIdAsync();
                var competition = await GetCurrentCompetitionAsync();
                
                // Registrar participante se não existir
                var participantEntity = await _participantService.CreateParticipantAsync(participant.Name, "");
                
                await _resultService.SaveResultAsync(
                    competitionId,
                    competition.Name,
                    participantEntity.RowKey,
                    participant.Name,
                    2, // Round 2
                    participant
                );
                
                Console.WriteLine($"Resultado da Rodada 2 salvo para {participant.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao salvar resultado da Rodada 2 para {participant.Name}: {ex.Message}");
                // Continua a execução mesmo com erro no Azure Storage
            }
        }
        
        public async Task RemoveRound1ResultAsync(string participantName)
        {
            // Remover da sessão
            var results = await GetRound1ResultsAsync();
            var existingResult = results.FirstOrDefault(r => r.Name == participantName);
            if (existingResult != null)
            {
                results.Remove(existingResult);
                SetInSession(Round1ResultsKey, results);
            }
            
            // Remover do Azure Table Storage
            var competitionId = await GetCurrentCompetitionIdAsync();
            
            // Encontrar o ID do participante
            var allParticipants = await _participantService.GetAllParticipantsAsync();
            var participantEntity = allParticipants.FirstOrDefault(p => p.Name == participantName);
            
            if (participantEntity != null)
            {
                await _resultService.DeleteResultAsync(
                    competitionId,
                    participantEntity.RowKey,
                    1 // Round 1
                );
            }
        }
        
        public async Task RemoveRound2ResultAsync(string participantName)
        {
            // Remover da sessão
            var results = await GetRound2ResultsAsync();
            var existingResult = results.FirstOrDefault(r => r.Name == participantName);
            if (existingResult != null)
            {
                results.Remove(existingResult);
                SetInSession(Round2ResultsKey, results);
            }
            
            // Remover do Azure Table Storage
            var competitionId = await GetCurrentCompetitionIdAsync();
            
            // Encontrar o ID do participante
            var allParticipants = await _participantService.GetAllParticipantsAsync();
            var participantEntity = allParticipants.FirstOrDefault(p => p.Name == participantName);
            
            if (participantEntity != null)
            {
                await _resultService.DeleteResultAsync(
                    competitionId,
                    participantEntity.RowKey,
                    2 // Round 2
                );
            }
        }
        
        // Implementação dos métodos de cálculo de resultados finais
        public async Task<List<FinalResult>> CalculateFinalResultsAsync()
        {
            var round1Results = await GetRound1ResultsAsync();
            var round2Results = await GetRound2ResultsAsync();
            var finalResults = new List<FinalResult>();
            
            // Obter a configuração de penalidades
            var config = Pages.Configuration.PenaltyConfigModel.GetCurrentConfiguration();
            
            // Processar cada participante que tem pelo menos um resultado
            var allParticipants = await GetParticipantsAsync();
            foreach (var participant in allParticipants)
            {
                var round1Result = round1Results.FirstOrDefault(r => r.Name == participant.Name);
                var round2Result = round2Results.FirstOrDefault(r => r.Name == participant.Name);
                
                if (round1Result == null && round2Result == null)
                {
                    continue;
                }
                
                decimal round1Time = round1Result?.CalculateTotalTime() ?? config.DisqualifiedValue;
                decimal round2Time = round2Result?.CalculateTotalTime() ?? config.DisqualifiedValue;
                
                bool round1Eliminated = round1Result?.IsEliminated ?? true;
                bool round2Eliminated = round2Result?.IsEliminated ?? true;
                
                decimal bestTime;
                string bestRound;
                
                if (round1Eliminated && round2Eliminated)
                {
                    bestTime = config.DisqualifiedValue;
                    bestRound = "Nenhuma";
                }
                else if (round1Eliminated)
                {
                    bestTime = round2Time;
                    bestRound = "Rodada 2";
                }
                else if (round2Eliminated)
                {
                    bestTime = round1Time;
                    bestRound = "Rodada 1";
                }
                else
                {
                    if (round1Time <= round2Time)
                    {
                        bestTime = round1Time;
                        bestRound = "Rodada 1";
                    }
                    else
                    {
                        bestTime = round2Time;
                        bestRound = "Rodada 2";
                    }
                }
                
                finalResults.Add(new FinalResult
                {
                    Name = participant.Name,
                    Round1Time = round1Time,
                    Round2Time = round2Time,
                    BestTime = bestTime,
                    BestRound = bestRound
                });
            }
            
            // Ordenar por melhor tempo
            finalResults = finalResults
                .OrderBy(r => r.BestTime == config.DisqualifiedValue) // Não eliminados primeiro
                .ThenBy(r => r.BestTime)
                .ToList();
            
            // Salvar no Azure Table Storage
            await SaveFinalResultsAsync(finalResults);
            
            return finalResults;
        }
        
        public async Task SaveFinalResultsAsync(List<FinalResult> finalResults)
        {
            try
            {
                var competitionId = await GetCurrentCompetitionIdAsync();
                var competition = await GetCurrentCompetitionAsync();
                
                Console.WriteLine($"Salvando resultados finais para competição: {competition.Name} (ID: {competitionId})");
                
                // Calcular e salvar resultados finais
                await _finalResultService.CalculateAndSaveFinalResultsAsync(competitionId, competition.Name);
                
                Console.WriteLine("Resultados finais salvos com sucesso no Azure Storage");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao salvar resultados finais: {ex.Message}");
                throw; // Re-throw para que o erro seja capturado na UI
            }
        }
        
        // Implementação dos métodos de histórico
        public async Task<IEnumerable<CompetitionEntity>> GetCompetitionHistoryAsync()
        {
            return await _competitionService.GetAllCompetitionsAsync();
        }
        
        public async Task<IEnumerable<ResultEntity>> GetParticipantHistoryAsync(string participantName)
        {
            // Encontrar o ID do participante
            var allParticipants = await _participantService.GetAllParticipantsAsync();
            var participantEntity = allParticipants.FirstOrDefault(p => p.Name == participantName);
            
            if (participantEntity != null)
            {
                return await _resultService.GetResultsByParticipantIdAsync(participantEntity.RowKey);
            }
            
            return new List<ResultEntity>();
        }
    }
}
