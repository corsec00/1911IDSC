using CompetitionApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using CompetitionApp.Pages.Configuration;

namespace CompetitionApp.Pages.Rounds
{
    public class Round2Model : PageModel
    {
        [BindProperty]
        public string ParticipantName { get; set; } = string.Empty;
        
        [BindProperty]
        public decimal TimeInSeconds { get; set; }
        
        [BindProperty]
        public int BravoCount { get; set; }
        
        [BindProperty]
        public int CharlieCount { get; set; }
        
        [BindProperty]
        public int MissCount { get; set; }
        
        [BindProperty]
        public int FaltaCount { get; set; }
        
        [BindProperty]
        public int VitimaCount { get; set; }
        
        [BindProperty]
        public int PlateCount { get; set; }
        
        [BindProperty]
        public bool IsEditing { get; set; }
        
        public List<Participant> Participants { get; set; } = new List<Participant>();
        public List<Participant> Round2Results { get; set; } = new List<Participant>();
        
        public void OnGet()
        {
            LoadData();
        }
        
        public IActionResult OnPost()
        {
            LoadData();
            
            if (string.IsNullOrWhiteSpace(ParticipantName))
            {
                ModelState.AddModelError("ParticipantName", "Selecione um participante.");
                return Page();
            }
            
            // Verificar se estamos editando ou adicionando um novo resultado
            if (IsEditing)
            {
                // Encontrar o resultado existente para atualizar
                var existingResult = Round2Results.FirstOrDefault(p => p.Name == ParticipantName);
                if (existingResult != null)
                {
                    // Atualizar os valores
                    existingResult.TimeInSeconds = TimeInSeconds;
                    existingResult.BravoCount = BravoCount;
                    existingResult.CharlieCount = CharlieCount;
                    existingResult.MissCount = MissCount;
                    existingResult.FaltaCount = FaltaCount;
                    existingResult.VitimaCount = VitimaCount;
                    existingResult.PlateCount = PlateCount;
                    
                    // Salvar as alterações
                    HttpContext.Session.SetString("Round2Results", JsonSerializer.Serialize(Round2Results));
                    return RedirectToPage();
                }
                else
                {
                    ModelState.AddModelError("ParticipantName", "Participante não encontrado para edição.");
                    return Page();
                }
            }
            else
            {
                // Verificar se o participante já tem resultado registrado
                if (Round2Results.Any(p => p.Name == ParticipantName))
                {
                    ModelState.AddModelError("ParticipantName", "Este participante já tem resultado registrado.");
                    return Page();
                }
                
                // Criar novo resultado
                var result = new Participant
                {
                    Name = ParticipantName,
                    TimeInSeconds = TimeInSeconds,
                    BravoCount = BravoCount,
                    CharlieCount = CharlieCount,
                    MissCount = MissCount,
                    FaltaCount = FaltaCount,
                    VitimaCount = VitimaCount,
                    PlateCount = PlateCount
                };
                
                Round2Results.Add(result);
                HttpContext.Session.SetString("Round2Results", JsonSerializer.Serialize(Round2Results));
                
                return RedirectToPage();
            }
        }
        
        public IActionResult OnPostRemove(string name)
        {
            LoadData();
            
            var result = Round2Results.FirstOrDefault(p => p.Name == name);
            if (result != null)
            {
                Round2Results.Remove(result);
                HttpContext.Session.SetString("Round2Results", JsonSerializer.Serialize(Round2Results));
            }
            
            return RedirectToPage();
        }
        
        private void LoadData()
        {
            var participantsJson = HttpContext.Session.GetString("Participants");
            if (!string.IsNullOrEmpty(participantsJson))
            {
                Participants = JsonSerializer.Deserialize<List<Participant>>(participantsJson) ?? new List<Participant>();
            }
            
            var round2ResultsJson = HttpContext.Session.GetString("Round2Results");
            if (!string.IsNullOrEmpty(round2ResultsJson))
            {
                Round2Results = JsonSerializer.Deserialize<List<Participant>>(round2ResultsJson) ?? new List<Participant>();
                
                // Garantir que participantes antigos tenham os novos campos inicializados
                foreach (var result in Round2Results)
                {
                    if (result.VitimaCount == null) result.VitimaCount = 0;
                    if (result.PlateCount == null) result.PlateCount = 0;
                }
            }
        }
    }
}
