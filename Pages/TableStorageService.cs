using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompetitionApp.Services
{
    public interface ITableStorageService
    {
        Task<TableClient> GetTableClientAsync(string tableName);
        Task<T> GetEntityAsync<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity, new();
        Task<IEnumerable<T>> QueryEntitiesAsync<T>(string tableName, string filter = null) where T : class, ITableEntity, new();
        Task AddEntityAsync<T>(string tableName, T entity) where T : class, ITableEntity;
        Task UpdateEntityAsync<T>(string tableName, T entity) where T : class, ITableEntity;
        Task DeleteEntityAsync<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity, new();
        Task EnsureTableExistsAsync(string tableName);
    }

    public class TableStorageService : ITableStorageService
    {
        private readonly IConfiguration _configuration;
        private string _connectionString;

        public TableStorageService(IConfiguration configuration)
        {
            _configuration = configuration;
            Console.WriteLine("TableStorageService: Serviço inicializado");
        }

        private async Task<string> GetConnectionStringAsync()
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                Console.WriteLine("TableStorageService: Obtendo connection string...");
                
                // Primeiro, tentar obter da variável de ambiente
                _connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
                Console.WriteLine($"TableStorageService: Variável de ambiente AZURE_STORAGE_CONNECTION_STRING: {(!string.IsNullOrEmpty(_connectionString) ? "ENCONTRADA" : "NÃO ENCONTRADA")}");
                
                // Se não encontrou na variável de ambiente, tentar obter da configuração
                if (string.IsNullOrEmpty(_connectionString))
                {
                    _connectionString = _configuration["AzureTableStorage:ConnectionString"];
                    Console.WriteLine($"TableStorageService: Connection string do appsettings.json: {(!string.IsNullOrEmpty(_connectionString) ? "ENCONTRADA" : "NÃO ENCONTRADA")}");
                }
                
                // Se ainda não encontrou, usar o emulador local para desenvolvimento
                if (string.IsNullOrEmpty(_connectionString))
                {
                    _connectionString = "UseDevelopmentStorage=true";
                    Console.WriteLine("TableStorageService: AVISO - Usando emulador local do Azure Storage. Configure AZURE_STORAGE_CONNECTION_STRING para usar Azure Storage real.");
                }
                else
                {
                    // Mascarar a connection string para logs (mostrar apenas os primeiros e últimos caracteres)
                    var maskedConnectionString = _connectionString.Length > 20 
                        ? _connectionString.Substring(0, 10) + "..." + _connectionString.Substring(_connectionString.Length - 10)
                        : "***";
                    Console.WriteLine($"TableStorageService: Azure Storage configurado com sucesso. Connection string: {maskedConnectionString}");
                }
            }
            return _connectionString;
        }

        public async Task<TableClient> GetTableClientAsync(string tableName)
        {
            try
            {
                Console.WriteLine($"TableStorageService: Criando TableClient para tabela '{tableName}'");
                var connectionString = await GetConnectionStringAsync();
                var tableClient = new TableClient(connectionString, tableName);
                
                Console.WriteLine($"TableStorageService: TableClient criado para '{tableName}'");
                await EnsureTableExistsAsync(tableName);
                Console.WriteLine($"TableStorageService: Tabela '{tableName}' verificada/criada com sucesso");
                
                return tableClient;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TableStorageService: ERRO ao criar TableClient para '{tableName}': {ex.Message}");
                Console.WriteLine($"TableStorageService: Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<T> GetEntityAsync<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity, new()
        {
            try
            {
                Console.WriteLine($"TableStorageService: Buscando entidade - Tabela: {tableName}, PartitionKey: {partitionKey}, RowKey: {rowKey}");
                var tableClient = await GetTableClientAsync(tableName);
                
                var result = await tableClient.GetEntityAsync<T>(partitionKey, rowKey);
                Console.WriteLine($"TableStorageService: Entidade encontrada - Tabela: {tableName}, PartitionKey: {partitionKey}, RowKey: {rowKey}");
                return result.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                Console.WriteLine($"TableStorageService: Entidade NÃO encontrada - Tabela: {tableName}, PartitionKey: {partitionKey}, RowKey: {rowKey}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TableStorageService: ERRO ao buscar entidade - Tabela: {tableName}, PartitionKey: {partitionKey}, RowKey: {rowKey}");
                Console.WriteLine($"TableStorageService: Erro: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<T>> QueryEntitiesAsync<T>(string tableName, string filter = null) where T : class, ITableEntity, new()
        {
            try
            {
                Console.WriteLine($"TableStorageService: Consultando entidades - Tabela: {tableName}, Filtro: {filter ?? "SEM FILTRO"}");
                var tableClient = await GetTableClientAsync(tableName);
                var results = new List<T>();
                
                AsyncPageable<T> queryResults;
                if (string.IsNullOrEmpty(filter))
                {
                    queryResults = tableClient.QueryAsync<T>();
                }
                else
                {
                    queryResults = tableClient.QueryAsync<T>(filter);
                }

                await foreach (var entity in queryResults)
                {
                    results.Add(entity);
                }
                
                Console.WriteLine($"TableStorageService: Consulta concluída - Tabela: {tableName}, Resultados encontrados: {results.Count}");
                
                // Log detalhado dos primeiros resultados para debug
                if (results.Count > 0)
                {
                    Console.WriteLine($"TableStorageService: Primeiros resultados:");
                    for (int i = 0; i < Math.Min(3, results.Count); i++)
                    {
                        var entity = results[i];
                        Console.WriteLine($"  [{i+1}] PartitionKey: {entity.PartitionKey}, RowKey: {entity.RowKey}");
                    }
                }
                
                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TableStorageService: ERRO ao consultar entidades - Tabela: {tableName}");
                Console.WriteLine($"TableStorageService: Erro: {ex.Message}");
                Console.WriteLine($"TableStorageService: Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task AddEntityAsync<T>(string tableName, T entity) where T : class, ITableEntity
        {
            try
            {
                Console.WriteLine($"TableStorageService: Adicionando entidade - Tabela: {tableName}, PartitionKey: {entity.PartitionKey}, RowKey: {entity.RowKey}");
                var tableClient = await GetTableClientAsync(tableName);
                await tableClient.AddEntityAsync(entity);
                Console.WriteLine($"TableStorageService: Entidade adicionada com sucesso - Tabela: {tableName}, PartitionKey: {entity.PartitionKey}, RowKey: {entity.RowKey}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TableStorageService: ERRO ao adicionar entidade - Tabela: {tableName}, PartitionKey: {entity.PartitionKey}, RowKey: {entity.RowKey}");
                Console.WriteLine($"TableStorageService: Erro: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateEntityAsync<T>(string tableName, T entity) where T : class, ITableEntity
        {
            try
            {
                Console.WriteLine($"TableStorageService: Atualizando entidade - Tabela: {tableName}, PartitionKey: {entity.PartitionKey}, RowKey: {entity.RowKey}");
                var tableClient = await GetTableClientAsync(tableName);
                await tableClient.UpdateEntityAsync(entity, entity.ETag);
                Console.WriteLine($"TableStorageService: Entidade atualizada com sucesso - Tabela: {tableName}, PartitionKey: {entity.PartitionKey}, RowKey: {entity.RowKey}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TableStorageService: ERRO ao atualizar entidade - Tabela: {tableName}, PartitionKey: {entity.PartitionKey}, RowKey: {entity.RowKey}");
                Console.WriteLine($"TableStorageService: Erro: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteEntityAsync<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity, new()
        {
            try
            {
                Console.WriteLine($"TableStorageService: Excluindo entidade - Tabela: {tableName}, PartitionKey: {partitionKey}, RowKey: {rowKey}");
                var tableClient = await GetTableClientAsync(tableName);
                await tableClient.DeleteEntityAsync(partitionKey, rowKey);
                Console.WriteLine($"TableStorageService: Entidade excluída com sucesso - Tabela: {tableName}, PartitionKey: {partitionKey}, RowKey: {rowKey}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TableStorageService: ERRO ao excluir entidade - Tabela: {tableName}, PartitionKey: {partitionKey}, RowKey: {rowKey}");
                Console.WriteLine($"TableStorageService: Erro: {ex.Message}");
                throw;
            }
        }

        public async Task EnsureTableExistsAsync(string tableName)
        {
            try
            {
                Console.WriteLine($"TableStorageService: Verificando se tabela '{tableName}' existe");
                var connectionString = await GetConnectionStringAsync();
                var serviceClient = new TableServiceClient(connectionString);
                await serviceClient.CreateTableIfNotExistsAsync(tableName);
                Console.WriteLine($"TableStorageService: Tabela '{tableName}' verificada/criada");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TableStorageService: ERRO ao verificar/criar tabela '{tableName}': {ex.Message}");
                throw;
            }
        }
    }
}

