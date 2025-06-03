# Atualização da Lógica da Aplicação para Persistência de Dados

## Visão Geral

Para atualizar a aplicação existente para usar o Azure Table Storage em vez de armazenamento em sessão, precisamos:

1. Criar uma nova classe `CompetitionManager` para gerenciar competições
2. Atualizar os modelos de página para usar os novos serviços
3. Implementar visualizações de histórico de competições e participantes
4. Manter compatibilidade com o fluxo atual da aplicação

## CompetitionManager

Esta classe será responsável por gerenciar o estado atual da competição e fazer a ponte entre a sessão e o armazenamento persistente.

```csharp
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
            
            // Salvar no Azure Table Storage
            var competitionId = await GetCurrentCompetitionIdAsync();
            var competition = await GetCurrentCompetitionAsync();
            
            // Encontrar o ID do participante
            var allParticipants = await _participantService.GetAllParticipantsAsync();
            var participantEntity = allParticipants.FirstOrDefault(p => p.Name == participant.Name);
            
            if (participantEntity != null)
            {
                await _resultService.SaveResultAsync(
                    competitionId,
                    competition.Name,
                    participantEntity.RowKey,
                    participant.Name,
                    1, // Round 1
                    participant
                );
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
            
            // Salvar no Azure Table Storage
            var competitionId = await GetCurrentCompetitionIdAsync();
            var competition = await GetCurrentCompetitionAsync();
            
            // Encontrar o ID do participante
            var allParticipants = await _participantService.GetAllParticipantsAsync();
            var participantEntity = allParticipants.FirstOrDefault(p => p.Name == participant.Name);
            
            if (participantEntity != null)
            {
                await _resultService.SaveResultAsync(
                    competitionId,
                    competition.Name,
                    participantEntity.RowKey,
                    participant.Name,
                    2, // Round 2
                    participant
                );
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
            var competitionId = await GetCurrentCompetitionIdAsync();
            var competition = await GetCurrentCompetitionAsync();
            
            // Encontrar IDs dos participantes
            var allParticipants = await _participantService.GetAllParticipantsAsync();
            
            // Calcular e salvar resultados finais
            await _finalResultService.CalculateAndSaveFinalResultsAsync(competitionId, competition.Name);
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
```

## Registro do CompetitionManager no Program.cs

```csharp
// Registrar o CompetitionManager
builder.Services.AddScoped<ICompetitionManager, CompetitionManager>();
```

## Atualização da Página de Registro de Participantes

```csharp
using CompetitionApp.Managers;
using CompetitionApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompetitionApp.Pages.Participants
{
    public class RegisterModel : PageModel
    {
        private readonly ICompetitionManager _competitionManager;
        
        public RegisterModel(ICompetitionManager competitionManager)
        {
            _competitionManager = competitionManager;
        }
        
        [BindProperty]
        public string ParticipantName { get; set; }
        
        public List<Participant> Participants { get; set; } = new List<Participant>();
        
        public async Task OnGetAsync()
        {
            Participants = await _competitionManager.GetParticipantsAsync();
        }
        
        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(ParticipantName))
            {
                ModelState.AddModelError("ParticipantName", "O nome do participante é obrigatório.");
                Participants = await _competitionManager.GetParticipantsAsync();
                return Page();
            }
            
            Participants = await _competitionManager.GetParticipantsAsync();
            
            if (Participants.Exists(p => p.Name == ParticipantName))
            {
                ModelState.AddModelError("ParticipantName", "Este participante já está registrado.");
                return Page();
            }
            
            await _competitionManager.RegisterParticipantAsync(ParticipantName);
            
            return RedirectToPage();
        }
        
        public async Task<IActionResult> OnPostRemoveAsync(string name)
        {
            Participants = await _competitionManager.GetParticipantsAsync();
            
            var participant = Participants.Find(p => p.Name == name);
            if (participant != null)
            {
                Participants.Remove(participant);
                await _competitionManager.SaveParticipantsAsync(Participants);
            }
            
            return RedirectToPage();
        }
    }
}
```

## Atualização da Página da Rodada 1

```csharp
using CompetitionApp.Managers;
using CompetitionApp.Models;
using CompetitionApp.Pages.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompetitionApp.Pages.Rounds
{
    public class Round1Model : PageModel
    {
        private readonly ICompetitionManager _competitionManager;
        
        public Round1Model(ICompetitionManager competitionManager)
        {
            _competitionManager = competitionManager;
        }
        
        [BindProperty]
        public string ParticipantName { get; set; } = string.Empty;
        
        [BindProperty]
        public decimal TimeInSeconds { get; set; }
        
        [BindProperty]
        public int BravoCount { get; set; }
        
        [BindProperty]
        public int CharlieCount { get; set; }
        
        [BindProperty]
        public int MissCount { get; set; }
        
        [BindProperty]
        public int FaltaCount { get; set; }
        
        [BindProperty]
        public int VitimaCount { get; set; }
        
        [BindProperty]
        public int PlateCount { get; set; }
        
        [BindProperty]
        public bool IsEditing { get; set; }
        
        public List<Participant> Participants { get; set; } = new List<Participant>();
        public List<Participant> Round1Results { get; set; } = new List<Participant>();
        
        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }
        
        public async Task<IActionResult> OnPostAsync()
        {
            await LoadDataAsync();
            
            if (string.IsNullOrWhiteSpace(ParticipantName))
            {
                ModelState.AddModelError("ParticipantName", "Selecione um participante.");
                return Page();
            }
            
            // Verificar se estamos editando ou adicionando um novo resultado
            if (IsEditing)
            {
                // Encontrar o resultado existente para atualizar
                var existingResult = Round1Results.FirstOrDefault(p => p.Name == ParticipantName);
                if (existingResult != null)
                {
                    // Atualizar os valores
                    existingResult.TimeInSeconds = TimeInSeconds;
                    existingResult.BravoCount = BravoCount;
                    existingResult.CharlieCount = CharlieCount;
                    existingResult.MissCount = MissCount;
                    existingResult.FaltaCount = FaltaCount;
                    existingResult.VitimaCount = VitimaCount;
                    existingResult.PlateCount = PlateCount;
                    
                    // Salvar as alterações
                    await _competitionManager.SaveRound1ResultAsync(existingResult);
                    return RedirectToPage();
                }
                else
                {
                    ModelState.AddModelError("ParticipantName", "Participante não encontrado para edição.");
                    return Page();
                }
            }
            else
            {
                // Verificar se o participante já tem resultado registrado
                if (Round1Results.Any(p => p.Name == ParticipantName))
                {
                    ModelState.AddModelError("ParticipantName", "Este participante já tem resultado registrado.");
                    return Page();
                }
                
                // Criar novo resultado
                var result = new Participant
                {
                    Name = ParticipantName,
                    TimeInSeconds = TimeInSeconds,
                    BravoCount = BravoCount,
                    CharlieCount = CharlieCount,
                    MissCount = MissCount,
                    FaltaCount = FaltaCount,
                    VitimaCount = VitimaCount,
                    PlateCount = PlateCount
                };
                
                await _competitionManager.SaveRound1ResultAsync(result);
                
                return RedirectToPage();
            }
        }
        
        public async Task<IActionResult> OnPostRemoveAsync(string name)
        {
            await _competitionManager.RemoveRound1ResultAsync(name);
            
            return RedirectToPage();
        }
        
        private async Task LoadDataAsync()
        {
            Participants = await _competitionManager.GetParticipantsAsync();
            Round1Results = await _competitionManager.GetRound1ResultsAsync();
        }
    }
}
```

## Atualização da Página da Rodada 2

```csharp
using CompetitionApp.Managers;
using CompetitionApp.Models;
using CompetitionApp.Pages.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompetitionApp.Pages.Rounds
{
    public class Round2Model : PageModel
    {
        private readonly ICompetitionManager _competitionManager;
        
        public Round2Model(ICompetitionManager competitionManager)
        {
            _competitionManager = competitionManager;
        }
        
        [BindProperty]
        public string ParticipantName { get; set; } = string.Empty;
        
        [BindProperty]
        public decimal TimeInSeconds { get; set; }
        
        [BindProperty]
        public int BravoCount { get; set; }
        
        [BindProperty]
        public int CharlieCount { get; set; }
        
        [BindProperty]
        public int MissCount { get; set; }
        
        [BindProperty]
        public int FaltaCount { get; set; }
        
        [BindProperty]
        public int VitimaCount { get; set; }
        
        [BindProperty]
        public int PlateCount { get; set; }
        
        [BindProperty]
        public bool IsEditing { get; set; }
        
        public List<Participant> Participants { get; set; } = new List<Participant>();
        public List<Participant> Round2Results { get; set; } = new List<Participant>();
        
        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }
        
        public async Task<IActionResult> OnPostAsync()
        {
            await LoadDataAsync();
            
            if (string.IsNullOrWhiteSpace(ParticipantName))
            {
                ModelState.AddModelError("ParticipantName", "Selecione um participante.");
                return Page();
            }
            
            // Verificar se estamos editando ou adicionando um novo resultado
            if (IsEditing)
            {
                // Encontrar o resultado existente para atualizar
                var existingResult = Round2Results.FirstOrDefault(p => p.Name == ParticipantName);
                if (existingResult != null)
                {
                    // Atualizar os valores
                    existingResult.TimeInSeconds = TimeInSeconds;
                    existingResult.BravoCount = BravoCount;
                    existingResult.CharlieCount = CharlieCount;
                    existingResult.MissCount = MissCount;
                    existingResult.FaltaCount = FaltaCount;
                    existingResult.VitimaCount = VitimaCount;
                    existingResult.PlateCount = PlateCount;
                    
                    // Salvar as alterações
                    await _competitionManager.SaveRound2ResultAsync(existingResult);
                    return RedirectToPage();
                }
                else
                {
                    ModelState.AddModelError("ParticipantName", "Participante não encontrado para edição.");
                    return Page();
                }
            }
            else
            {
                // Verificar se o participante já tem resultado registrado
                if (Round2Results.Any(p => p.Name == ParticipantName))
                {
                    ModelState.AddModelError("ParticipantName", "Este participante já tem resultado registrado.");
                    return Page();
                }
                
                // Criar novo resultado
                var result = new Participant
                {
                    Name = ParticipantName,
                    TimeInSeconds = TimeInSeconds,
                    BravoCount = BravoCount,
                    CharlieCount = CharlieCount,
                    MissCount = MissCount,
                    FaltaCount = FaltaCount,
                    VitimaCount = VitimaCount,
                    PlateCount = PlateCount
                };
                
                await _competitionManager.SaveRound2ResultAsync(result);
                
                return RedirectToPage();
            }
        }
        
        public async Task<IActionResult> OnPostRemoveAsync(string name)
        {
            await _competitionManager.RemoveRound2ResultAsync(name);
            
            return RedirectToPage();
        }
        
        private async Task LoadDataAsync()
        {
            Participants = await _competitionManager.GetParticipantsAsync();
            Round2Results = await _competitionManager.GetRound2ResultsAsync();
        }
    }
}
```

## Atualização da Página de Resultados Finais

```csharp
using CompetitionApp.Managers;
using CompetitionApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompetitionApp.Pages.Results
{
    public class FinalModel : PageModel
    {
        private readonly ICompetitionManager _competitionManager;
        
        public FinalModel(ICompetitionManager competitionManager)
        {
            _competitionManager = competitionManager;
        }
        
        public List<FinalResult> FinalResults { get; set; } = new List<FinalResult>();
        public string CompetitionName { get; set; }
        
        public async Task OnGetAsync()
        {
            var competition = await _competitionManager.GetCurrentCompetitionAsync();
            CompetitionName = competition.Name;
            
            FinalResults = await _competitionManager.CalculateFinalResultsAsync();
        }
    }
}
```

## Implementação de Páginas de Histórico

### Página de Histórico de Competições

```csharp
using CompetitionApp.Managers;
using CompetitionApp.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompetitionApp.Pages.History
{
    public class CompetitionsModel : PageModel
    {
        private readonly ICompetitionManager _competitionManager;
        
        public CompetitionsModel(ICompetitionManager competitionManager)
        {
            _competitionManager = competitionManager;
        }
        
        public IEnumerable<CompetitionEntity> Competitions { get; set; }
        
        public async Task OnGetAsync()
        {
            Competitions = await _competitionManager.GetCompetitionHistoryAsync();
        }
        
        public async Task<IActionResult> OnPostSelectAsync(string competitionId)
        {
            await _competitionManager.SetCurrentCompetitionIdAsync(competitionId);
            return RedirectToPage("/Index");
        }
    }
}
```

### Página de Histórico de Participantes

```csharp
using CompetitionApp.Managers;
using CompetitionApp.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompetitionApp.Pages.History
{
    public class ParticipantHistoryModel : PageModel
    {
        private readonly ICompetitionManager _competitionManager;
        
        public ParticipantHistoryModel(ICompetitionManager competitionManager)
        {
            _competitionManager = competitionManager;
        }
        
        [BindProperty(SupportsGet = true)]
        public string ParticipantName { get; set; }
        
        public IEnumerable<ResultEntity> Results { get; set; }
        public Dictionary<string, string> CompetitionNames { get; set; } = new Dictionary<string, string>();
        
        public async Task OnGetAsync()
        {
            if (!string.IsNullOrEmpty(ParticipantName))
            {
                Results = await _competitionManager.GetParticipantHistoryAsync(ParticipantName);
                
                // Obter nomes das competições
                var competitions = await _competitionManager.GetCompetitionHistoryAsync();
                foreach (var competition in competitions)
                {
                    CompetitionNames[competition.RowKey] = competition.Name;
                }
            }
        }
    }
}
```

## Implementação das Páginas Razor para Histórico

### Competitions.cshtml

```html
@page
@model CompetitionApp.Pages.History.CompetitionsModel
@{
    ViewData["Title"] = "Histórico de Competições";
}

<h1>Histórico de Competições</h1>

<div class="card mb-4">
    <div class="card-header">
        Competições Anteriores
    </div>
    <div class="card-body">
        @if (!Model.Competitions.Any())
        {
            <p>Nenhuma competição encontrada.</p>
        }
        else
        {
            <div class="table-responsive">
                <table class="table table-striped">
                    <thead>
                        <tr>
                            <th>Nome</th>
                            <th>Data</th>
                            <th>Descrição</th>
                            <th>Ações</th>
                        </tr>
                    </thead>
                    <tbody>
                        @foreach (var competition in Model.Competitions)
                        {
                            <tr>
                                <td>@competition.Name</td>
                                <td>@competition.Date.ToString("dd/MM/yyyy HH:mm")</td>
                                <td>@competition.Description</td>
                                <td>
                                    <form method="post" asp-page-handler="Select">
                                        <input type="hidden" name="competitionId" value="@competition.RowKey" />
                                        <button type="submit" class="btn btn-sm btn-primary">Selecionar</button>
                                    </form>
                                </td>
                            </tr>
                        }
                    </tbody>
                </table>
            </div>
        }
    </div>
    <div class="card-footer">
        <a asp-page="/Index" class="btn btn-secondary">Voltar</a>
    </div>
</div>
```

### ParticipantHistory.cshtml

```html
@page
@model CompetitionApp.Pages.History.ParticipantHistoryModel
@{
    ViewData["Title"] = "Histórico de Participante";
}

<h1>Histórico de Participante</h1>

<div class="card mb-4">
    <div class="card-header">
        Buscar Participante
    </div>
    <div class="card-body">
        <form method="get">
            <div class="form-group mb-3">
                <label for="participantName">Nome do Participante:</label>
                <input type="text" id="participantName" name="ParticipantName" class="form-control" value="@Model.ParticipantName" required />
            </div>
            <button type="submit" class="btn btn-primary">Buscar</button>
        </form>
    </div>
</div>

@if (!string.IsNullOrEmpty(Model.ParticipantName))
{
    <div class="card">
        <div class="card-header">
            Resultados de @Model.ParticipantName
        </div>
        <div class="card-body">
            @if (Model.Results == null || !Model.Results.Any())
            {
                <p>Nenhum resultado encontrado para este participante.</p>
            }
            else
            {
                <div class="table-responsive">
                    <table class="table table-striped">
                        <thead>
                            <tr>
                                <th>Competição</th>
                                <th>Rodada</th>
                                <th>Tempo Base</th>
                                <th>Bravo</th>
                                <th>Charlie</th>
                                <th>Miss</th>
                                <th>Fault</th>
                                <th>Vítima</th>
                                <th>Plate</th>
                                <th>Tempo Total</th>
                                <th>Data</th>
                            </tr>
                        </thead>
                        <tbody>
                            @foreach (var result in Model.Results.OrderByDescending(r => r.CreatedAt))
                            {
                                <tr>
                                    <td>@result.CompetitionName</td>
                                    <td>@result.RoundNumber</td>
                                    <td>@(result.IsEliminated ? "Desclassificado" : $"{result.TimeInSeconds:F3}s")</td>
                                    <td>@result.BravoCount</td>
                                    <td>@result.CharlieCount</td>
                                    <td>@result.MissCount</td>
                                    <td>@result.FaltaCount</td>
                                    <td>@result.VitimaCount</td>
                                    <td>@result.PlateCount</td>
                                    <td>@(result.IsEliminated ? "Desclassificado" : $"{result.TotalTime:F3}s")</td>
                                    <td>@result.CreatedAt.ToString("dd/MM/yyyy HH:mm")</td>
                                </tr>
                            }
                        </tbody>
                    </table>
                </div>
            }
        </div>
        <div class="card-footer">
            <a asp-page="/Index" class="btn btn-secondary">Voltar</a>
        </div>
    </div>
}
```

## Atualização do Menu de Navegação

Adicione links para as novas páginas de histórico no arquivo `_Layout.cshtml`:

```html
<li class="nav-item">
    <a class="nav-link text-dark" asp-area="" asp-page="/History/Competitions">Histórico de Competições</a>
</li>
<li class="nav-item">
    <a class="nav-link text-dark" asp-area="" asp-page="/History/ParticipantHistory">Histórico de Participantes</a>
</li>
```

## Atualização da Página Inicial

Atualize a página inicial para mostrar a competição atual:

```csharp
using CompetitionApp.Managers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace CompetitionApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ICompetitionManager _competitionManager;
        
        public IndexModel(ICompetitionManager competitionManager)
        {
            _competitionManager = competitionManager;
        }
        
        public string CompetitionName { get; set; }
        public string CompetitionDate { get; set; }
        
        public async Task OnGetAsync()
        {
            var competition = await _competitionManager.GetCurrentCompetitionAsync();
            CompetitionName = competition.Name;
            CompetitionDate = competition.Date.ToString("dd/MM/yyyy HH:mm");
        }
        
        public async Task<IActionResult> OnPostNewCompetitionAsync()
        {
            await _competitionManager.CreateCompetitionAsync("Nova Competição " + System.DateTime.Now.ToString("dd/MM/yyyy HH:mm"), "Competição criada manualmente");
            return RedirectToPage();
        }
    }
}
```

Atualize a página Index.cshtml para mostrar a competição atual e permitir criar uma nova:

```html
@page
@model IndexModel
@{
    ViewData["Title"] = "Início";
}

<div class="text-center">
    <h1 class="display-4">Sistema de Competição</h1>
    
    <div class="alert alert-info mt-4">
        <h4>Competição Atual: @Model.CompetitionName</h4>
        <p>Data: @Model.CompetitionDate</p>
        
        <form method="post" asp-page-handler="NewCompetition" class="mt-3">
            <button type="submit" class="btn btn-primary">Criar Nova Competição</button>
        </form>
    </div>
    
    <div class="row mt-4">
        <div class="col-md-4">
            <div class="card mb-4">
                <div class="card-header">
                    Participantes
                </div>
                <div class="card-body">
                    <p>Cadastre os participantes da competição.</p>
                    <a asp-page="/Participants/Register" class="btn btn-primary">Cadastrar Participantes</a>
                </div>
            </div>
        </div>
        
        <div class="col-md-4">
            <div class="card mb-4">
                <div class="card-header">
                    Rodadas
                </div>
                <div class="card-body">
                    <p>Registre os resultados das rodadas.</p>
                    <a asp-page="/Rounds/Round1" class="btn btn-primary">Primeira Rodada</a>
                    <a asp-page="/Rounds/Round2" class="btn btn-primary mt-2">Segunda Rodada</a>
                </div>
            </div>
        </div>
        
        <div class="col-md-4">
            <div class="card mb-4">
                <div class="card-header">
                    Resultados
                </div>
                <div class="card-body">
                    <p>Visualize os resultados finais da competição.</p>
                    <a asp-page="/Results/Final" class="btn btn-success">Resultados Finais</a>
                </div>
            </div>
        </div>
    </div>
    
    <div class="row mt-2">
        <div class="col-md-6">
            <div class="card mb-4">
                <div class="card-header">
                    Histórico
                </div>
                <div class="card-body">
                    <p>Acesse o histórico de competições e participantes.</p>
                    <a asp-page="/History/Competitions" class="btn btn-info">Histórico de Competições</a>
                    <a asp-page="/History/ParticipantHistory" class="btn btn-info mt-2">Histórico de Participantes</a>
                </div>
            </div>
        </div>
        
        <div class="col-md-6">
            <div class="card mb-4">
                <div class="card-header">
                    Configurações
                </div>
                <div class="card-body">
                    <p>Configure os valores de penalidades.</p>
                    <a asp-page="/Configuration/PenaltyConfig" class="btn btn-secondary">Configurar Penalidades</a>
                </div>
            </div>
        </div>
    </div>
</div>
```

Esta implementação fornece uma atualização completa da lógica da aplicação para usar o Azure Table Storage para persistência de dados, mantendo a compatibilidade com o fluxo atual baseado em sessão. Também adiciona novas páginas para visualizar o histórico de competições e participantes.
