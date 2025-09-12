using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CompetitionApp.Pages.Configuration
{
    public class PenaltyConfigModel : PageModel
    {
        [BindProperty]
        [Display(Name = "Alfa")]
        [Range(0, 999, ErrorMessage = "O valor deve estar entre 0 e 999")]
        public decimal AlfaValue { get; set; } = 0;

        [BindProperty]
        [Display(Name = "Bravo")]
        [Range(0, 999, ErrorMessage = "O valor deve estar entre 0 e 999")]
        public decimal BravoValue { get; set; } = 2;

        [BindProperty]
        [Display(Name = "Charlie")]
        [Range(0, 999, ErrorMessage = "O valor deve estar entre 0 e 999")]
        public decimal CharlieValue { get; set; } = 5;

        [BindProperty]
        [Display(Name = "Miss")]
        [Range(0, 999, ErrorMessage = "O valor deve estar entre 0 e 999")]
        public decimal MissValue { get; set; } = 10;

        [BindProperty]
        [Display(Name = "Fault")]
        [Range(0, 999, ErrorMessage = "O valor deve estar entre 0 e 999")]
        public decimal FaultValue { get; set; } = 4;

        [BindProperty]
        [Display(Name = "Vítima")]
        [Range(0, 999, ErrorMessage = "O valor deve estar entre 0 e 999")]
        public decimal VitimaValue { get; set; } = 10;

        [BindProperty]
        [Display(Name = "Plate")]
        [Range(0, 999, ErrorMessage = "O valor deve estar entre 0 e 999")]
        public decimal PlateValue { get; set; } = 10;

        [BindProperty]
        [Display(Name = "Desclassificado")]
        [Range(0, 9999, ErrorMessage = "O valor deve estar entre 0 e 9999")]
        public decimal DisqualifiedValue { get; set; } = 999;

        // Configuração estática para ser usada em toda a aplicação
        private static PenaltyConfigModel _currentConfig = new PenaltyConfigModel();

        public static PenaltyConfigModel GetCurrentConfiguration()
        {
            return _currentConfig;
        }

        public void OnGet()
        {
            // Carregar valores atuais da configuração
            AlfaValue = _currentConfig.AlfaValue;
            BravoValue = _currentConfig.BravoValue;
            CharlieValue = _currentConfig.CharlieValue;
            MissValue = _currentConfig.MissValue;
            FaultValue = _currentConfig.FaultValue;
            VitimaValue = _currentConfig.VitimaValue;
            PlateValue = _currentConfig.PlateValue;
            DisqualifiedValue = _currentConfig.DisqualifiedValue;
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Atualizar configuração global
                _currentConfig.AlfaValue = AlfaValue;
                _currentConfig.BravoValue = BravoValue;
                _currentConfig.CharlieValue = CharlieValue;
                _currentConfig.MissValue = MissValue;
                _currentConfig.FaultValue = FaultValue;
                _currentConfig.VitimaValue = VitimaValue;
                _currentConfig.PlateValue = PlateValue;
                _currentConfig.DisqualifiedValue = DisqualifiedValue;

                TempData["SuccessMessage"] = "Configurações de penalidades salvas com sucesso!";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Erro ao salvar configurações: {ex.Message}");
                return Page();
            }
        }
    }
}

