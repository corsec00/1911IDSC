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
        }

        public IActionResult OnPostClearData()
        {
            HttpContext.Session.Clear();
            return RedirectToPage();
        }
    }
}
