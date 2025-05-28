using CompetitionApp.Managers;
using CompetitionApp.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompetitionApp.Pages.History
{
    public class ParticipantHistoryModel : PageModel
    {
        private readonly ICompetitionManager _competitionManager;
        
        public ParticipantHistoryModel(ICompetitionManager competitionManager)
        {
            _competitionManager = competitionManager;
        }
        
        [BindProperty(SupportsGet = true)]
        public string ParticipantName { get; set; }
        
        public IEnumerable<ResultEntity> Results { get; set; }
        public Dictionary<string, string> CompetitionNames { get; set; } = new Dictionary<string, string>();
        
        public async Task OnGetAsync()
        {
            if (!string.IsNullOrEmpty(ParticipantName))
            {
                Results = await _competitionManager.GetParticipantHistoryAsync(ParticipantName);
                
                // Obter nomes das competições
                var competitions = await _competitionManager.GetCompetitionHistoryAsync();
                foreach (var competition in competitions)
                {
                    CompetitionNames[competition.RowKey] = competition.Name;
                }
            }
        }
    }
}
