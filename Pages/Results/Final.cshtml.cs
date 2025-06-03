using CompetitionApp.Managers;
using CompetitionApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using CompetitionApp.Pages.Configuration;

namespace CompetitionApp.Pages.Results
{
    public class FinalModel : PageModel
    {
        private readonly ICompetitionManager _competitionManager;
        
        public FinalModel(ICompetitionManager competitionManager)
        {
            _competitionManager = competitionManager;
        }
        
        public List<Participant> Round1Results { get; set; } = new List<Participant>();
        public List<Participant> Round2Results { get; set; } = new List<Participant>();
        public List<FinalResult> FinalResults { get; set; } = new List<FinalResult>();
        
        [TempData]
        public string StatusMessage { get; set; }
        
        public async Task<IActionResult> OnGetAsync()
        {
            LoadData();
            FinalResults = await CalculateFinalResultsAsync();
            return Page();
        }
        
        public async Task<IActionResult> OnGetExportPdfAsync()
        {
            LoadData();
            FinalResults = await CalculateFinalResultsAsync();
            
            // Redirecionar para a página de PDF
            return RedirectToPage("/Results/ExportPdf");
        }
        
        public async Task<IActionResult> OnPostSaveToStorageAsync()
        {
            LoadData();
            FinalResults = await CalculateFinalResultsAsync();
            
            // Salvar os resultados finais no Azure Storage
            await _competitionManager.SaveFinalResultsAsync(FinalResults);
            
            StatusMessage = "Resultados salvos com sucesso no Azure Storage!";
            return RedirectToPage();
        }
        
        private void LoadData()
        {
            var round1ResultsJson = HttpContext.Session.GetString("Round1Results");
            if (!string.IsNullOrEmpty(round1ResultsJson))
            {
                Round1Results = JsonSerializer.Deserialize<List<Participant>>(round1ResultsJson) ?? new List<Participant>();
                
                // Garantir que participantes antigos tenham os novos campos inicializados
                foreach (var result in Round1Results)
                {
                    if (result.VitimaCount == null) result.VitimaCount = 0;
                    if (result.PlateCount == null) result.PlateCount = 0;
                }
            }
            
            var round2ResultsJson = HttpContext.Session.GetString("Round2Results");
            if (!string.IsNullOrEmpty(round2ResultsJson))
            {
                Round2Results = JsonSerializer.Deserialize<List<Participant>>(round2ResultsJson) ?? new List<Participant>();
                
                // Garantir que participantes antigos tenham os novos campos inicializados
                foreach (var result in Round2Results)
                {
                    if (result.VitimaCount == null) result.VitimaCount = 0;
                    if (result.PlateCount == null) result.PlateCount = 0;
                }
            }
        }
        
        private async Task<List<FinalResult>> CalculateFinalResultsAsync()
        {
            // Usar o CompetitionManager para calcular os resultados finais
            // Isso garante que a lógica seja consistente e que os resultados sejam salvos no Azure Storage
            return await _competitionManager.CalculateFinalResultsAsync();
        }
    }
    
    public class FinalResult
    {
        public string Name { get; set; } = string.Empty;
        public decimal Round1Time { get; set; }
        public decimal Round2Time { get; set; }
        public decimal BestTime { get; set; }
        public string BestRound { get; set; } = string.Empty;
    }
}
