using CompetitionApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace CompetitionApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }
        
        public bool HasAnyResults { get; private set; }

        public void OnGet()
        {
            // Inicializa a lista de participantes na sessão se não existir
            if (HttpContext.Session.GetString("Participants") == null)
            {
                HttpContext.Session.SetString("Participants", JsonSerializer.Serialize(new List<Participant>()));
            }

            if (HttpContext.Session.GetString("Round1Results") == null)
            {
                HttpContext.Session.SetString("Round1Results", JsonSerializer.Serialize(new List<Participant>()));
            }

            if (HttpContext.Session.GetString("Round2Results") == null)
            {
                HttpContext.Session.SetString("Round2Results", JsonSerializer.Serialize(new List<Participant>()));
            }
            
            // Verifica se já existem resultados cadastrados
            CheckForExistingResults();
        }

        public IActionResult OnPostClearData()
        {
            HttpContext.Session.Clear();
            return RedirectToPage();
        }
        
        private void CheckForExistingResults()
        {
            var round1ResultsJson = HttpContext.Session.GetString("Round1Results");
            var round2ResultsJson = HttpContext.Session.GetString("Round2Results");
            
            var round1Results = !string.IsNullOrEmpty(round1ResultsJson) 
                ? JsonSerializer.Deserialize<List<Participant>>(round1ResultsJson) 
                : new List<Participant>();
                
            var round2Results = !string.IsNullOrEmpty(round2ResultsJson) 
                ? JsonSerializer.Deserialize<List<Participant>>(round2ResultsJson) 
                : new List<Participant>();
                
            // A competição já foi iniciada se houver qualquer resultado em qualquer rodada
            HasAnyResults = (round1Results != null && round1Results.Count > 0) || 
                           (round2Results != null && round2Results.Count > 0);
        }
    }
}
