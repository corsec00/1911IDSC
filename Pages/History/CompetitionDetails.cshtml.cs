using CompetitionApp.Managers;
using CompetitionApp.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompetitionApp.Pages.History
{
    public class CompetitionDetailsModel : PageModel
    {
        private readonly ICompetitionManager _competitionManager;
        private readonly ICompetitionService _competitionService;
        private readonly IResultService _resultService;
        private readonly IFinalResultService _finalResultService;
        
        public CompetitionDetailsModel(
            ICompetitionManager competitionManager,
            ICompetitionService competitionService,
            IResultService resultService,
            IFinalResultService finalResultService)
        {
            _competitionManager = competitionManager;
            _competitionService = competitionService;
            _resultService = resultService;
            _finalResultService = finalResultService;
        }
        
        public CompetitionEntity Competition { get; set; }
        public IEnumerable<ResultEntity> Round1Results { get; set; } = new List<ResultEntity>();
        public IEnumerable<ResultEntity> Round2Results { get; set; } = new List<ResultEntity>();
        public IEnumerable<FinalResultEntity> FinalResults { get; set; } = new List<FinalResultEntity>();
        
        public async Task<IActionResult> OnGetAsync(string competitionId)
        {
            if (string.IsNullOrEmpty(competitionId))
            {
                return NotFound();
            }
            
            // Obter detalhes da competição
            Competition = await _competitionService.GetCompetitionByIdAsync(competitionId);
            if (Competition == null)
            {
                return NotFound();
            }
            
            // Obter resultados da Rodada 1 e 2
            var allResults = await _resultService.GetResultsByCompetitionIdAsync(competitionId);
            Round1Results = allResults.Where(r => r.RoundNumber == 1).ToList();
            Round2Results = allResults.Where(r => r.RoundNumber == 2).ToList();
            
            // Obter resultados finais
            FinalResults = await _finalResultService.GetFinalResultsByCompetitionIdAsync(competitionId);
            
            return Page();
        }
        
        public async Task<IActionResult> OnPostDeleteAsync(string competitionId)
        {
            if (string.IsNullOrEmpty(competitionId))
            {
                return NotFound();
            }
            
            // Obter detalhes da competição para confirmar que existe
            var competition = await _competitionService.GetCompetitionByIdAsync(competitionId);
            if (competition == null)
            {
                return NotFound();
            }
            
            // 1. Excluir todos os resultados finais da competição
            var finalResults = await _finalResultService.GetFinalResultsByCompetitionIdAsync(competitionId);
            foreach (var result in finalResults)
            {
                await _finalResultService.DeleteFinalResultAsync(competitionId, result.ParticipantId);
            }
            
            // 2. Excluir todos os resultados de rodadas da competição
            var roundResults = await _resultService.GetResultsByCompetitionIdAsync(competitionId);
            foreach (var result in roundResults)
            {
                await _resultService.DeleteResultAsync(competitionId, result.ParticipantId, result.RoundNumber);
            }
            
            // 3. Excluir a competição
            await _competitionService.DeleteCompetitionAsync(competitionId);
            
            TempData["SuccessMessage"] = $"Competição '{competition.Name}' excluída com sucesso.";
            
            return RedirectToPage("./Competitions");
        }
    }
}
