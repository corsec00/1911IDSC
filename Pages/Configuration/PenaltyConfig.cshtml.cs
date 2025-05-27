using CompetitionApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace CompetitionApp.Pages.Configuration
{
    public class PenaltyConfigModel : PageModel
    {
        // Armazenamento estático para manter os valores entre solicitações
        private static PenaltyConfiguration _penaltyConfig = new PenaltyConfiguration();

        [BindProperty]
        public PenaltyConfiguration PenaltyConfig { get; set; }

        public void OnGet()
        {
            // Carrega os valores atuais da configuração
            PenaltyConfig = _penaltyConfig;
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Atualiza a configuração estática com os novos valores
            _penaltyConfig = PenaltyConfig;

            // Adiciona mensagem de sucesso
            TempData["SuccessMessage"] = "Configurações de penalidades atualizadas com sucesso!";

            return RedirectToPage();
        }

        // Método estático para obter a configuração atual de qualquer lugar do aplicativo
        public static PenaltyConfiguration GetCurrentConfiguration()
        {
            return _penaltyConfig;
        }
    }
}
