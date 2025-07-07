using CompetitionApp.Managers;
using CompetitionApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using CompetitionApp.Pages.Configuration;
using System.Collections.Generic;
using System.Text;

namespace CompetitionApp.Pages.Results
{
    public class FinalModel : PageModel
    {
        private readonly ICompetitionManager _competitionManager;

        public FinalModel(ICompetitionManager competitionManager)
        {
            _competitionManager = competitionManager;
        }

        public List<Participant> Round1Results { get; set; } = new List<Participant>();
        public List<Participant> Round2Results { get; set; } = new List<Participant>();
        public List<Models.FinalResult> FinalResults { get; set; } = new List<Models.FinalResult>();

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            LoadData();
            FinalResults = await CalculateFinalResultsAsync();
            return Page();
        }

        public async Task<IActionResult> OnGetExportPdfAsync()
        {
            LoadData();
            FinalResults = await CalculateFinalResultsAsync();
            // Redirecionar para a página de PDF
            return RedirectToPage("/Results/ExportPdf");
        }

        public async Task<IActionResult> OnGetExportCsvAsync()
        {
            LoadData();
            FinalResults = await CalculateFinalResultsAsync();

            // Gerar CSV
            var csvContent = GenerateCsvContent();
            var fileName = $"Resultados_Competicao_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            // Retornar arquivo CSV para download
            return File(Encoding.UTF8.GetBytes(csvContent), "text/csv", fileName);
        }

        public async Task<IActionResult> OnPostSaveToStorageAsync()
        {
            LoadData();
            FinalResults = await CalculateFinalResultsAsync();

            // Salvar os resultados finais no Azure Storage
            await _competitionManager.SaveFinalResultsAsync(FinalResults.ToList());

            StatusMessage = "Resultados salvos com sucesso no Azure Storage!";
            return RedirectToPage();
        }

        private string GenerateCsvContent()
        {
            var config = PenaltyConfigModel.GetCurrentConfiguration();
            var csv = new StringBuilder();

            // Cabeçalho do arquivo
            csv.AppendLine("# Resultados da Competição");
            csv.AppendLine($"# Data de Exportação: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            csv.AppendLine($"# Total de Participantes: {FinalResults.Count}");
            csv.AppendLine($"# Participantes Classificados: {FinalResults.Count(r => r.BestTime < config.DisqualifiedValue)}");
            csv.AppendLine($"# Participantes Desclassificados: {FinalResults.Count(r => r.BestTime >= config.DisqualifiedValue)}");
            csv.AppendLine();

            // Configuração de penalidades utilizada
            csv.AppendLine("# Configuração de Penalidades Utilizada:");
            csv.AppendLine($"# Alfa: {config.AlfaValue}s, Bravo: {config.BravoValue}s, Charlie: {config.CharlieValue}s");
            csv.AppendLine($"# Miss: {config.MissValue}s, Fault: {config.FaultValue}s, Vítima: {config.VitimaValue}s, Plate: {config.PlateValue}s");
            csv.AppendLine();

            // === CLASSIFICAÇÃO FINAL ===
            csv.AppendLine("=== CLASSIFICAÇÃO FINAL ===");
            csv.AppendLine("Posição,Nome,Melhor Tempo (s),Rodada do Melhor Tempo,Tempo Rodada 1 (s),Tempo Rodada 2 (s),Status");

            for (int i = 0; i < FinalResults.Count; i++)
            {
                var result = FinalResults[i];
                var position = i + 1;
                var bestTime = result.BestTime >= config.DisqualifiedValue ? "DESCLASSIFICADO" : result.BestTime.ToString("F2");
                var round1Time = result.Round1Time >= config.DisqualifiedValue ? "DESCLASSIFICADO" : result.Round1Time.ToString("F2");
                var round2Time = result.Round2Time >= config.DisqualifiedValue ? "DESCLASSIFICADO" : result.Round2Time.ToString("F2");
                var status = result.BestTime >= config.DisqualifiedValue ? "DESCLASSIFICADO" : "CLASSIFICADO";

                csv.AppendLine($"{position},\"{result.Name}\",{bestTime},{result.BestRound},{round1Time},{round2Time},{status}");
            }

            csv.AppendLine();

            // === DETALHES DA RODADA 1 ===
            csv.AppendLine("=== DETALHES DA RODADA 1 ===");
            csv.AppendLine("Nome,Tempo Base (s),Alfa,Bravo,Charlie,Miss,Fault,Vítima,Plate,Total Marcadores,Penalidades (s),Tempo Final (s),Status");

            foreach (var result in Round1Results.OrderBy(r => r.CalculateTotalTime()))
            {
                var totalMarkers = result.AlfaCount + result.BravoCount + result.CharlieCount + result.MissCount + result.FaltaCount + result.VitimaCount + result.PlateCount;
                var penalties = (result.AlfaCount * config.AlfaValue) + (result.BravoCount * config.BravoValue) + 
                               (result.CharlieCount * config.CharlieValue) + (result.MissCount * config.MissValue) + 
                               (result.FaltaCount * config.FaultValue) + (result.VitimaCount * config.VitimaValue) + 
                               (result.PlateCount * config.PlateValue);
                var finalTime = result.CalculateTotalTime();
                var status = finalTime >= config.DisqualifiedValue ? "DESCLASSIFICADO" : "CLASSIFICADO";

                csv.AppendLine($"\"{result.Name}\",{result.TimeInSeconds:F2},{result.AlfaCount},{result.BravoCount},{result.CharlieCount},{result.MissCount},{result.FaltaCount},{result.VitimaCount},{result.PlateCount},{totalMarkers},{penalties:F2},{finalTime:F2},{status}");
            }

            csv.AppendLine();

            // === DETALHES DA RODADA 2 ===
            csv.AppendLine("=== DETALHES DA RODADA 2 ===");
            csv.AppendLine("Nome,Tempo Base (s),Alfa,Bravo,Charlie,Miss,Fault,Vítima,Plate,Total Marcadores,Penalidades (s),Tempo Final (s),Status");

            foreach (var result in Round2Results.OrderBy(r => r.CalculateTotalTime()))
            {
                var totalMarkers = result.AlfaCount + result.BravoCount + result.CharlieCount + result.MissCount + result.FaltaCount + result.VitimaCount + result.PlateCount;
                var penalties = (result.AlfaCount * config.AlfaValue) + (result.BravoCount * config.BravoValue) + 
                               (result.CharlieCount * config.CharlieValue) + (result.MissCount * config.MissValue) + 
                               (result.FaltaCount * config.FaultValue) + (result.VitimaCount * config.VitimaValue) + 
                               (result.PlateCount * config.PlateValue);
                var finalTime = result.CalculateTotalTime();
                var status = finalTime >= config.DisqualifiedValue ? "DESCLASSIFICADO" : "CLASSIFICADO";

                csv.AppendLine($"\"{result.Name}\",{result.TimeInSeconds:F2},{result.AlfaCount},{result.BravoCount},{result.CharlieCount},{result.MissCount},{result.FaltaCount},{result.VitimaCount},{result.PlateCount},{totalMarkers},{penalties:F2},{finalTime:F2},{status}");
            }

            csv.AppendLine();

            // === ESTATÍSTICAS GERAIS ===
            csv.AppendLine("=== ESTATÍSTICAS GERAIS ===");
            
            if (FinalResults.Any(r => r.BestTime < config.DisqualifiedValue))
            {
                var bestOverall = FinalResults.Where(r => r.BestTime < config.DisqualifiedValue).OrderBy(r => r.BestTime).First();
                csv.AppendLine($"Melhor Tempo Geral,{bestOverall.BestTime:F2}s,{bestOverall.Name}");
                
                var worstClassified = FinalResults.Where(r => r.BestTime < config.DisqualifiedValue).OrderByDescending(r => r.BestTime).First();
                csv.AppendLine($"Pior Tempo Classificado,{worstClassified.BestTime:F2}s,{worstClassified.Name}");
                
                var averageTime = FinalResults.Where(r => r.BestTime < config.DisqualifiedValue).Average(r => r.BestTime);
                csv.AppendLine($"Tempo Médio dos Classificados,{averageTime:F2}s");
            }

            // Estatísticas de marcadores
            var allResults = Round1Results.Concat(Round2Results).ToList();
            if (allResults.Any())
            {
                csv.AppendLine();
                csv.AppendLine("=== ESTATÍSTICAS DE MARCADORES ===");
                csv.AppendLine("Tipo,Total,Média por Participante");
                csv.AppendLine($"Alfa,{allResults.Sum(r => r.AlfaCount)},{allResults.Average(r => r.AlfaCount):F1}");
                csv.AppendLine($"Bravo,{allResults.Sum(r => r.BravoCount)},{allResults.Average(r => r.BravoCount):F1}");
                csv.AppendLine($"Charlie,{allResults.Sum(r => r.CharlieCount)},{allResults.Average(r => r.CharlieCount):F1}");
                csv.AppendLine($"Miss,{allResults.Sum(r => r.MissCount)},{allResults.Average(r => r.MissCount):F1}");
                csv.AppendLine($"Fault,{allResults.Sum(r => r.FaltaCount)},{allResults.Average(r => r.FaltaCount):F1}");
                csv.AppendLine($"Vítima,{allResults.Sum(r => r.VitimaCount)},{allResults.Average(r => r.VitimaCount):F1}");
                csv.AppendLine($"Plate,{allResults.Sum(r => r.PlateCount)},{allResults.Average(r => r.PlateCount):F1}");
            }

            return csv.ToString();
        }

        private void LoadData()
        {
            var round1ResultsJson = HttpContext.Session.GetString("Round1Results");
            if (!string.IsNullOrEmpty(round1ResultsJson))
            {
                Round1Results = JsonSerializer.Deserialize<List<Participant>>(round1ResultsJson) ?? new List<Participant>();
                // Garantir que participantes antigos tenham os novos campos inicializados
                foreach (var result in Round1Results)
                {
                    if (result.VitimaCount == 0 && result.PlateCount == 0 && result.AlfaCount == 0)
                    {
                        result.VitimaCount = 0;
                        result.PlateCount = 0;
                        result.AlfaCount = 0;
                    }
                }
            }

            var round2ResultsJson = HttpContext.Session.GetString("Round2Results");
            if (!string.IsNullOrEmpty(round2ResultsJson))
            {
                Round2Results = JsonSerializer.Deserialize<List<Participant>>(round2ResultsJson) ?? new List<Participant>();
                // Garantir que participantes antigos tenham os novos campos inicializados
                foreach (var result in Round2Results)
                {
                    if (result.VitimaCount == 0 && result.PlateCount == 0 && result.AlfaCount == 0)
                    {
                        result.VitimaCount = 0;
                        result.PlateCount = 0;
                        result.AlfaCount = 0;
                    }
                }
            }
        }

        private async Task<List<FinalResult>> CalculateFinalResultsAsync()
        {
            // Usar o CompetitionManager para calcular os resultados finais
            // Isso garante que a lógica seja consistente e que os resultados sejam salvos no Azure Storage
            return await _competitionManager.CalculateFinalResultsAsync();
        }
    }
}

