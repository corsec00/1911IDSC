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
