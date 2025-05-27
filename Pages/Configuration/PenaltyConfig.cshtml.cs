using CompetitionApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Text.Json;

namespace CompetitionApp.Pages.Configuration
{
    public class PenaltyConfigModel : PageModel
    {
        // Armazenamento estático para manter os valores entre solicitações
        private static PenaltyConfiguration _penaltyConfig = new PenaltyConfiguration();

        [BindProperty]
        public PenaltyConfiguration PenaltyConfig { get; set; }
        
        public bool HasAnyResults { get; private set; }

        public void OnGet()
        {
            // Verifica se já existem resultados cadastrados
            CheckForExistingResults();
            
            // Se já existem resultados, redireciona para a página inicial
            if (HasAnyResults)
            {
                TempData["ErrorMessage"] = "A competição já foi iniciada. As penalidades não podem ser alteradas.";
                Response.Redirect("/Index");
                return;
            }
            
            // Carrega os valores atuais da configuração
            PenaltyConfig = _penaltyConfig;
        }

        public IActionResult OnPost()
        {
            // Verifica se já existem resultados cadastrados
            CheckForExistingResults();
            
            // Se já existem resultados, não permite alterações
            if (HasAnyResults)
            {
                TempData["ErrorMessage"] = "A competição já foi iniciada. As penalidades não podem ser alteradas.";
                return RedirectToPage("/Index");
            }
            
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Atualiza a configuração estática com os novos valores
            _penaltyConfig = PenaltyConfig;

            // Adiciona mensagem de sucesso
            TempData["SuccessMessage"] = "Configurações de penalidades atualizadas com sucesso!";

            return RedirectToPage("/Index");
        }

        // Método estático para obter a configuração atual de qualquer lugar do aplicativo
        public static PenaltyConfiguration GetCurrentConfiguration()
        {
            return _penaltyConfig;
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
