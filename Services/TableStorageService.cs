using Azure;
using Azure.Data.Tables;
using Azure.Security.KeyVault.Secrets;
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
        private readonly SecretClient _secretClient;
        private readonly IConfiguration _configuration;
        private string _connectionString;

        public TableStorageService(SecretClient secretClient, IConfiguration configuration)
        {
            _secretClient = secretClient;
            _configuration = configuration;
        }

        private async Task<string> GetConnectionStringAsync()
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                var secretName = _configuration["AzureTableStorage:ConnectionStringSecretName"];
                var secret = await _secretClient.GetSecretAsync(secretName);
                _connectionString = secret.Value.Value;
            }
            return _connectionString;
        }

        public async Task<TableClient> GetTableClientAsync(string tableName)
        {
            var connectionString = await GetConnectionStringAsync();
            var tableClient = new TableClient(connectionString, tableName);
            await EnsureTableExistsAsync(tableName);
            return tableClient;
        }

        public async Task<T> GetEntityAsync<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity, new()
        {
            var tableClient = await GetTableClientAsync(tableName);
            try
            {
                return await tableClient.GetEntityAsync<T>(partitionKey, rowKey);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task<IEnumerable<T>> QueryEntitiesAsync<T>(string tableName, string filter = null) where T : class, ITableEntity, new()
        {
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
            
            return results;
        }

        public async Task AddEntityAsync<T>(string tableName, T entity) where T : class, ITableEntity
        {
            var tableClient = await GetTableClientAsync(tableName);
            await tableClient.AddEntityAsync(entity);
        }

        public async Task UpdateEntityAsync<T>(string tableName, T entity) where T : class, ITableEntity
        {
            var tableClient = await GetTableClientAsync(tableName);
            await tableClient.UpdateEntityAsync(entity, entity.ETag);
        }

        public async Task DeleteEntityAsync<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity, new()
        {
            var tableClient = await GetTableClientAsync(tableName);
            await tableClient.DeleteEntityAsync(partitionKey, rowKey);
        }

        public async Task EnsureTableExistsAsync(string tableName)
        {
            var connectionString = await GetConnectionStringAsync();
            var serviceClient = new TableServiceClient(connectionString);
            await serviceClient.CreateTableIfNotExistsAsync(tableName);
        }
    }
}
