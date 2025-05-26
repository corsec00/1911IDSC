using CompetitionApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace CompetitionApp.Pages.Participants
{
    public class RegisterModel : PageModel
    {
        [BindProperty]
        public Participant NewParticipant { get; set; } = new Participant();
        
        public List<Participant> Participants { get; set; } = new List<Participant>();
        
        public void OnGet()
        {
            var participantsJson = HttpContext.Session.GetString("Participants");
            if (!string.IsNullOrEmpty(participantsJson))
            {
                Participants = JsonSerializer.Deserialize<List<Participant>>(participantsJson) ?? new List<Participant>();
            }
        }
        
        public IActionResult OnPost()
        {
            if (string.IsNullOrWhiteSpace(NewParticipant.Name))
            {
                ModelState.AddModelError("NewParticipant.Name", "O nome é obrigatório.");
                return Page();
            }
            
            var participantsJson = HttpContext.Session.GetString("Participants");
            if (!string.IsNullOrEmpty(participantsJson))
            {
                Participants = JsonSerializer.Deserialize<List<Participant>>(participantsJson) ?? new List<Participant>();
            }
            
            // Verificar se já existe um participante com o mesmo nome
            if (Participants.Any(p => p.Name.Equals(NewParticipant.Name, StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError("NewParticipant.Name", "Já existe um participante com este nome.");
                return Page();
            }
            
            // Verificar se já atingiu o limite de 30 participantes
            if (Participants.Count >= 30)
            {
                ModelState.AddModelError("", "Limite de 30 participantes atingido.");
                return Page();
            }
            
            Participants.Add(new Participant { Name = NewParticipant.Name });
            HttpContext.Session.SetString("Participants", JsonSerializer.Serialize(Participants));
            
            return RedirectToPage();
        }
        
        public IActionResult OnPostRemove(string name)
        {
            var participantsJson = HttpContext.Session.GetString("Participants");
            if (!string.IsNullOrEmpty(participantsJson))
            {
                Participants = JsonSerializer.Deserialize<List<Participant>>(participantsJson) ?? new List<Participant>();
                var participant = Participants.FirstOrDefault(p => p.Name == name);
                if (participant != null)
                {
                    Participants.Remove(participant);
                    HttpContext.Session.SetString("Participants", JsonSerializer.Serialize(Participants));
                }
            }
            
            return RedirectToPage();
        }
    }
}
