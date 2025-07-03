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
            try
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
            catch (Exception ex)
            {
                // Em caso de erro, registra o erro e carrega os valores padrão
                Console.WriteLine($"Erro ao carregar a página de configuração: {ex.Message}");
                PenaltyConfig = new PenaltyConfiguration();
            }
        }

        public IActionResult OnPost()
        {
            try
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
            catch (Exception ex)
            {
                // Em caso de erro, registra o erro e retorna para a página com mensagem
                Console.WriteLine($"Erro ao salvar a configuração: {ex.Message}");
                ModelState.AddModelError(string.Empty, "Ocorreu um erro ao salvar as configurações. Por favor, tente novamente.");
                PenaltyConfig = _penaltyConfig;
                return Page();
            }
        }

        // Método estático para obter a configuração atual de qualquer lugar do aplicativo
        public static PenaltyConfiguration GetCurrentConfiguration()
        {
            return _penaltyConfig;
        }
        
        private void CheckForExistingResults()
        {
            try
            {
                // Verifica se a sessão está disponível
                if (HttpContext.Session == null)
                {
                    HasAnyResults = false;
                    return;
                }
                
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
            catch
            {
                // Em caso de erro, assume que não há resultados
                HasAnyResults = false;
            }
        }
    }
}
