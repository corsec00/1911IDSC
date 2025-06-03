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
    public class CompetitionsModel : PageModel
    {
        private readonly ICompetitionManager _competitionManager;
        
        public CompetitionsModel(ICompetitionManager competitionManager)
        {
            _competitionManager = competitionManager;
        }
        
        public IEnumerable<CompetitionEntity> Competitions { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string NameFilter { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }
        
        public async Task OnGetAsync()
        {
            var allCompetitions = await _competitionManager.GetCompetitionHistoryAsync();
            
            // Apply filters
            Competitions = FilterCompetitions(allCompetitions);
        }
        
        public async Task<IActionResult> OnPostSelectAsync(string competitionId)
        {
            await _competitionManager.SetCurrentCompetitionIdAsync(competitionId);
            return RedirectToPage("/Index");
        }
        
        private IEnumerable<CompetitionEntity> FilterCompetitions(IEnumerable<CompetitionEntity> competitions)
        {
            var filtered = competitions;
            
            // Filter by name
            if (!string.IsNullOrWhiteSpace(NameFilter))
            {
                filtered = filtered.Where(c => c.Name.Contains(NameFilter, StringComparison.OrdinalIgnoreCase));
            }
            
            // Filter by start date
            if (StartDate.HasValue)
            {
                filtered = filtered.Where(c => c.CreatedAt.Date >= StartDate.Value.Date);
            }
            
            // Filter by end date
            if (EndDate.HasValue)
            {
                filtered = filtered.Where(c => c.CreatedAt.Date <= EndDate.Value.Date);
            }
            
            // Order by most recent registration date
            return filtered.OrderByDescending(c => c.CreatedAt);
        }
    }
}
