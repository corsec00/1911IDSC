using CompetitionApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CompetitionApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ICompetitionService _competitionService;
        private readonly IResultService _resultService;

        public IndexModel(ILogger<IndexModel> logger, ICompetitionService competitionService, IResultService resultService)
        {
            _logger = logger;
            _competitionService = competitionService;
            _resultService = resultService;
        }

        public void OnGet()
        {
            _logger.LogInformation("Página inicial acessada");
        }
    }
}

