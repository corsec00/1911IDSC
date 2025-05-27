using CompetitionApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace CompetitionApp.Pages.Results
{
    public class FinalModel : PageModel
    {
        public List<Participant> Round1Results { get; set; } = new List<Participant>();
        public List<Participant> Round2Results { get; set; } = new List<Participant>();
        public List<FinalResult> FinalResults { get; set; } = new List<FinalResult>();
        
        public void OnGet()
        {
            LoadData();
            CalculateFinalResults();
        }
        
        public IActionResult OnGetExportPdf()
        {
            LoadData();
            CalculateFinalResults();
            
            // Redirecionar para a página de PDF
            return RedirectToPage("/Results/ExportPdf");
        }
        
        private void LoadData()
        {
            var round1ResultsJson = HttpContext.Session.GetString("Round1Results");
            if (!string.IsNullOrEmpty(round1ResultsJson))
            {
                Round1Results = JsonSerializer.Deserialize<List<Participant>>(round1ResultsJson) ?? new List<Participant>();
            }
            
            var round2ResultsJson = HttpContext.Session.GetString("Round2Results");
            if (!string.IsNullOrEmpty(round2ResultsJson))
            {
                Round2Results = JsonSerializer.Deserialize<List<Participant>>(round2ResultsJson) ?? new List<Participant>();
            }
        }
        
        private void CalculateFinalResults()
        {
            // Obter todos os participantes únicos das duas rodadas
            var allParticipants = new HashSet<string>();
            
            foreach (var participant in Round1Results)
            {
                allParticipants.Add(participant.Name);
            }
            
            foreach (var participant in Round2Results)
            {
                allParticipants.Add(participant.Name);
            }
            
            // Calcular o melhor tempo para cada participante
            foreach (var name in allParticipants)
            {
                var round1Participant = Round1Results.FirstOrDefault(p => p.Name == name);
                var round2Participant = Round2Results.FirstOrDefault(p => p.Name == name);
                
                int round1Time = round1Participant != null ? round1Participant.CalculateTotalTime() : 0;
                int round2Time = round2Participant != null ? round2Participant.CalculateTotalTime() : 0;
                
                // Se o participante não tem tempo em uma rodada, considerar o tempo da outra rodada
                if (round1Time == 0 && round2Time > 0)
                {
                    FinalResults.Add(new FinalResult
                    {
                        Name = name,
                        Round1Time = 0,
                        Round2Time = round2Time,
                        BestTime = round2Time,
                        BestRound = "Rodada 2"
                    });
                }
                else if (round2Time == 0 && round1Time > 0)
                {
                    FinalResults.Add(new FinalResult
                    {
                        Name = name,
                        Round1Time = round1Time,
                        Round2Time = 0,
                        BestTime = round1Time,
                        BestRound = "Rodada 1"
                    });
                }
                else if (round1Time > 0 && round2Time > 0)
                {
                    // Se o participante tem tempo nas duas rodadas, pegar o melhor
                    bool isEliminated1 = round1Participant?.IsEliminated ?? false;
                    bool isEliminated2 = round2Participant?.IsEliminated ?? false;
                    
                    // Se foi eliminado nas duas rodadas
                    if (isEliminated1 && isEliminated2)
                    {
                        FinalResults.Add(new FinalResult
                        {
                            Name = name,
                            Round1Time = 999,
                            Round2Time = 999,
                            BestTime = 999,
                            BestRound = "Eliminado"
                        });
                    }
                    // Se foi eliminado apenas na rodada 1
                    else if (isEliminated1 && !isEliminated2)
                    {
                        FinalResults.Add(new FinalResult
                        {
                            Name = name,
                            Round1Time = 999,
                            Round2Time = round2Time,
                            BestTime = round2Time,
                            BestRound = "Rodada 2"
                        });
                    }
                    // Se foi eliminado apenas na rodada 2
                    else if (!isEliminated1 && isEliminated2)
                    {
                        FinalResults.Add(new FinalResult
                        {
                            Name = name,
                            Round1Time = round1Time,
                            Round2Time = 999,
                            BestTime = round1Time,
                            BestRound = "Rodada 1"
                        });
                    }
                    // Se não foi eliminado em nenhuma rodada
                    else
                    {
                        string bestRound = round1Time <= round2Time ? "Rodada 1" : "Rodada 2";
                        int bestTime = Math.Min(round1Time, round2Time);
                        
                        FinalResults.Add(new FinalResult
                        {
                            Name = name,
                            Round1Time = round1Time,
                            Round2Time = round2Time,
                            BestTime = bestTime,
                            BestRound = bestRound
                        });
                    }
                }
            }
            
            // Ordenar os resultados finais
            FinalResults = FinalResults
                .OrderBy(r => r.BestTime == 999) // Não eliminados primeiro
                .ThenBy(r => r.BestTime)         // Depois pelo melhor tempo
                .ToList();
        }
    }
    
    public class FinalResult
    {
        public string Name { get; set; } = string.Empty;
        public int Round1Time { get; set; }
        public int Round2Time { get; set; }
        public int BestTime { get; set; }
        public string BestRound { get; set; } = string.Empty;
    }
}
