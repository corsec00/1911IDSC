using CompetitionApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompetitionApp.Pages
{
    public class DiagnosticModel : PageModel
    {
        private readonly ITableStorageService _tableStorageService;
        private readonly ICompetitionService _competitionService;
        private readonly IResultService _resultService;
        private readonly IFinalResultService _finalResultService;

        public DiagnosticModel(
            ITableStorageService tableStorageService,
            ICompetitionService competitionService,
            IResultService resultService,
            IFinalResultService finalResultService)
        {
            _tableStorageService = tableStorageService;
            _competitionService = competitionService;
            _resultService = resultService;
            _finalResultService = finalResultService;
        }

        public List<string> DiagnosticResults { get; set; } = new List<string>();

        public async Task<IActionResult> OnGetAsync()
        {
            DiagnosticResults.Add("=== DIAGNÓSTICO DE CONEXÃO COM AZURE STORAGE ===");
            DiagnosticResults.Add($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            DiagnosticResults.Add("");

            try
            {
                // Teste 1: Verificar variáveis de ambiente
                DiagnosticResults.Add("1. VERIFICANDO VARIÁVEIS DE AMBIENTE:");
                var envConnectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
                DiagnosticResults.Add($"   AZURE_STORAGE_CONNECTION_STRING: {(string.IsNullOrEmpty(envConnectionString) ? "NÃO CONFIGURADA" : "CONFIGURADA")}");
                
                if (!string.IsNullOrEmpty(envConnectionString))
                {
                    var maskedConnectionString = envConnectionString.Length > 20 
                        ? envConnectionString.Substring(0, 15) + "..." + envConnectionString.Substring(envConnectionString.Length - 15)
                        : "***";
                    DiagnosticResults.Add($"   Connection String (mascarada): {maskedConnectionString}");
                }
                DiagnosticResults.Add("");

                // Teste 2: Testar criação de TableClient
                DiagnosticResults.Add("2. TESTANDO CRIAÇÃO DE TABLE CLIENTS:");
                var tables = new[] { "Competitions", "Results", "FinalResults", "Participants" };
                
                foreach (var tableName in tables)
                {
                    try
                    {
                        var tableClient = await _tableStorageService.GetTableClientAsync(tableName);
                        DiagnosticResults.Add($"   ✓ Tabela '{tableName}': SUCESSO");
                    }
                    catch (Exception ex)
                    {
                        DiagnosticResults.Add($"   ✗ Tabela '{tableName}': ERRO - {ex.Message}");
                    }
                }
                DiagnosticResults.Add("");

                // Teste 3: Listar competições
                DiagnosticResults.Add("3. TESTANDO RECUPERAÇÃO DE DADOS:");
                try
                {
                    var competitions = await _competitionService.GetAllCompetitionsAsync();
                    var competitionsList = competitions.ToList();
                    DiagnosticResults.Add($"   ✓ Competições encontradas: {competitionsList.Count}");
                    
                    foreach (var comp in competitionsList.Take(3))
                    {
                        DiagnosticResults.Add($"     - {comp.Name} (ID: {comp.RowKey}, Data: {comp.CreatedAt:yyyy-MM-dd HH:mm})");
                    }
                }
                catch (Exception ex)
                {
                    DiagnosticResults.Add($"   ✗ Erro ao buscar competições: {ex.Message}");
                }
                DiagnosticResults.Add("");

                // Teste 4: Testar uma competição específica se existir
                try
                {
                    var competitions = await _competitionService.GetAllCompetitionsAsync();
                    var firstCompetition = competitions.FirstOrDefault();
                    
                    if (firstCompetition != null)
                    {
                        DiagnosticResults.Add($"4. TESTANDO RECUPERAÇÃO DE RESULTADOS DA COMPETIÇÃO: {firstCompetition.Name}");
                        
                        var results = await _resultService.GetResultsByCompetitionIdAsync(firstCompetition.RowKey);
                        var resultsList = results.ToList();
                        DiagnosticResults.Add($"   ✓ Resultados de rodadas encontrados: {resultsList.Count}");
                        
                        var finalResults = await _finalResultService.GetFinalResultsByCompetitionIdAsync(firstCompetition.RowKey);
                        var finalResultsList = finalResults.ToList();
                        DiagnosticResults.Add($"   ✓ Resultados finais encontrados: {finalResultsList.Count}");
                        
                        foreach (var result in resultsList.Take(3))
                        {
                            DiagnosticResults.Add($"     - {result.ParticipantName}, Rodada {result.RoundNumber}, Tempo: {result.TotalTime:F2}s");
                        }
                    }
                    else
                    {
                        DiagnosticResults.Add("4. NENHUMA COMPETIÇÃO ENCONTRADA PARA TESTAR RESULTADOS");
                    }
                }
                catch (Exception ex)
                {
                    DiagnosticResults.Add($"   ✗ Erro ao testar resultados: {ex.Message}");
                }
                DiagnosticResults.Add("");

                // Teste 5: Informações do sistema
                DiagnosticResults.Add("5. INFORMAÇÕES DO SISTEMA:");
                DiagnosticResults.Add($"   Ambiente: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}");
                DiagnosticResults.Add($"   Máquina: {Environment.MachineName}");
                DiagnosticResults.Add($"   Usuário: {Environment.UserName}");
                DiagnosticResults.Add($"   Diretório de trabalho: {Environment.CurrentDirectory}");

            }
            catch (Exception ex)
            {
                DiagnosticResults.Add($"ERRO GERAL NO DIAGNÓSTICO: {ex.Message}");
                DiagnosticResults.Add($"Stack trace: {ex.StackTrace}");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostTestConnectionAsync()
        {
            DiagnosticResults.Add("=== TESTE DE CONEXÃO MANUAL ===");
            
            try
            {
                // Teste básico de conexão
                var tableClient = await _tableStorageService.GetTableClientAsync("TestTable");
                DiagnosticResults.Add("✓ Conexão com Azure Storage estabelecida com sucesso!");
                
                // Tentar criar uma entidade de teste
                var testEntity = new TestEntity
                {
                    PartitionKey = "test",
                    RowKey = Guid.NewGuid().ToString(),
                    TestData = "Teste de conexão"
                };
                
                await _tableStorageService.AddEntityAsync("TestTable", testEntity);
                DiagnosticResults.Add("✓ Entidade de teste criada com sucesso!");
                
                // Tentar recuperar a entidade
                var retrievedEntity = await _tableStorageService.GetEntityAsync<TestEntity>("TestTable", "test", testEntity.RowKey);
                if (retrievedEntity != null)
                {
                    DiagnosticResults.Add("✓ Entidade de teste recuperada com sucesso!");
                }
                else
                {
                    DiagnosticResults.Add("✗ Falha ao recuperar entidade de teste");
                }
                
                // Limpar entidade de teste
                await _tableStorageService.DeleteEntityAsync<TestEntity>("TestTable", "test", testEntity.RowKey);
                DiagnosticResults.Add("✓ Entidade de teste removida com sucesso!");
                
            }
            catch (Exception ex)
            {
                DiagnosticResults.Add($"✗ Erro no teste de conexão: {ex.Message}");
                DiagnosticResults.Add($"Stack trace: {ex.StackTrace}");
            }
            
            return Page();
        }
    }

    public class TestEntity : Azure.Data.Tables.ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public Azure.ETag ETag { get; set; }
        public string TestData { get; set; }
    }
}

