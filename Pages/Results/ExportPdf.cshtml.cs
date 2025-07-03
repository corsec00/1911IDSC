using CompetitionApp.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace CompetitionApp.Pages.Results
{
    public class ExportPdfModel : PageModel
    {
        public List<Participant> Round1Results { get; set; } = new List<Participant>();
        public List<Participant> Round2Results { get; set; } = new List<Participant>();
        public List<FinalResult> FinalResults { get; set; } = new List<FinalResult>();
        
        public void OnGet()
        {
            LoadData();
            CalculateFinalResults();
        }
        
        public IActionResult OnGetDownload()
        {
            LoadData();
            CalculateFinalResults();
            
            // Criar o PDF em memória
            using (var stream = new MemoryStream())
            {
                using (var writer = new PdfWriter(stream))
                {
                    using (var pdf = new PdfDocument(writer))
                    {
                        using (var document = new Document(pdf))
                        {
                            // Título
                            document.Add(new Paragraph("Resultados da Competição")
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(20));
                            
                            document.Add(new Paragraph($"Data: {DateTime.Now:dd/MM/yyyy HH:mm}")
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(12));
                            
                            document.Add(new Paragraph("\n"));
                            
                            // Classificação Final
                            document.Add(new Paragraph("Classificação Final (Baseada no Melhor Tempo)")
                                .SetFontSize(16)
                                .SetBold());
                            
                            Table finalTable = new Table(6).UseAllAvailableWidth();
                            finalTable.AddHeaderCell("Posição");
                            finalTable.AddHeaderCell("Nome");
                            finalTable.AddHeaderCell("Melhor Tempo");
                            finalTable.AddHeaderCell("Rodada do Melhor Tempo");
                            finalTable.AddHeaderCell("Tempo Rodada 1");
                            finalTable.AddHeaderCell("Tempo Rodada 2");
                            
                            for (int i = 0; i < FinalResults.Count; i++)
                            {
                                var result = FinalResults[i];
                                finalTable.AddCell((i + 1).ToString());
                                finalTable.AddCell(result.Name);
                                finalTable.AddCell(result.BestTime == 999 ? "Eliminado" : $"{result.BestTime}s");
                                finalTable.AddCell(result.BestRound);
                                finalTable.AddCell(result.Round1Time == 999 ? "Eliminado" : (result.Round1Time == 0 ? "Não registrado" : $"{result.Round1Time}s"));
                                finalTable.AddCell(result.Round2Time == 999 ? "Eliminado" : (result.Round2Time == 0 ? "Não registrado" : $"{result.Round2Time}s"));
                            }
                            
                            document.Add(finalTable);
                            document.Add(new Paragraph("\n"));
                            
                            // Resultados da Primeira Rodada
                            document.Add(new Paragraph("Resultados da Primeira Rodada")
                                .SetFontSize(16)
                                .SetBold());
                            
                            Table round1Table = new Table(8).UseAllAvailableWidth();
                            round1Table.AddHeaderCell("Posição");
                            round1Table.AddHeaderCell("Nome");
                            round1Table.AddHeaderCell("Bravo (3s)");
                            round1Table.AddHeaderCell("Charlie (5s)");
                            round1Table.AddHeaderCell("Miss (10s)");
                            round1Table.AddHeaderCell("Falta (10s)");
                            round1Table.AddHeaderCell("Tempo Base");
                            round1Table.AddHeaderCell("Tempo Total");
                            
                            var sortedRound1 = Round1Results
                                .OrderBy(p => p.IsEliminated)
                                .ThenBy(p => p.CalculateTotalTime())
                                .ToList();
                            
                            for (int i = 0; i < sortedRound1.Count; i++)
                            {
                                var result = sortedRound1[i];
                                round1Table.AddCell((i + 1).ToString());
                                round1Table.AddCell(result.Name);
                                round1Table.AddCell(result.BravoCount.ToString());
                                round1Table.AddCell(result.CharlieCount.ToString());
                                round1Table.AddCell(result.MissCount.ToString());
                                round1Table.AddCell(result.FaltaCount.ToString());
                                round1Table.AddCell(result.IsEliminated ? "Eliminado" : $"{result.TimeInSeconds}s");
                                round1Table.AddCell(result.IsEliminated ? "Eliminado" : $"{result.CalculateTotalTime()}s");
                            }
                            
                            document.Add(round1Table);
                            document.Add(new Paragraph("\n"));
                            
                            // Resultados da Segunda Rodada
                            document.Add(new Paragraph("Resultados da Segunda Rodada")
                                .SetFontSize(16)
                                .SetBold());
                            
                            Table round2Table = new Table(8).UseAllAvailableWidth();
                            round2Table.AddHeaderCell("Posição");
                            round2Table.AddHeaderCell("Nome");
                            round2Table.AddHeaderCell("Bravo (3s)");
                            round2Table.AddHeaderCell("Charlie (5s)");
                            round2Table.AddHeaderCell("Miss (10s)");
                            round2Table.AddHeaderCell("Falta (10s)");
                            round2Table.AddHeaderCell("Tempo Base");
                            round2Table.AddHeaderCell("Tempo Total");
                            
                            var sortedRound2 = Round2Results
                                .OrderBy(p => p.IsEliminated)
                                .ThenBy(p => p.CalculateTotalTime())
                                .ToList();
                            
                            for (int i = 0; i < sortedRound2.Count; i++)
                            {
                                var result = sortedRound2[i];
                                round2Table.AddCell((i + 1).ToString());
                                round2Table.AddCell(result.Name);
                                round2Table.AddCell(result.BravoCount.ToString());
                                round2Table.AddCell(result.CharlieCount.ToString());
                                round2Table.AddCell(result.MissCount.ToString());
                                round2Table.AddCell(result.FaltaCount.ToString());
                                round2Table.AddCell(result.IsEliminated ? "Eliminado" : $"{result.TimeInSeconds}s");
                                round2Table.AddCell(result.IsEliminated ? "Eliminado" : $"{result.CalculateTotalTime()}s");
                            }
                            
                            document.Add(round2Table);
                            
                            // Rodapé
                            document.Add(new Paragraph($"Sistema de Competição - Gerado em {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(10));
                        }
                    }
                }
                
                // Retornar o PDF como um arquivo para download
                return File(stream.ToArray(), "application/pdf", "Resultados_Competicao.pdf");
            }
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
                
                decimal round1Time = round1Participant != null ? round1Participant.CalculateTotalTime() : 0;
                decimal round2Time = round2Participant != null ? round2Participant.CalculateTotalTime() : 0;
                
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
                        decimal bestTime = Math.Min(round1Time, round2Time);
                        
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
}
