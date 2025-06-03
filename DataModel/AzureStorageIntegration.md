# Integração com Azure Table Storage e Azure Key Vault

## Visão Geral

Para integrar nossa aplicação com o Azure Table Storage e usar o Azure Key Vault para armazenamento seguro de credenciais, precisamos:

1. Criar os modelos de entidade para o Azure Table Storage
2. Configurar o acesso ao Azure Key Vault
3. Implementar serviços para interagir com o Azure Table Storage
4. Atualizar a lógica da aplicação para usar esses serviços

## Pacotes NuGet Necessários

```xml
<ItemGroup>
  <PackageReference Include="Azure.Data.Tables" Version="12.8.2" />
  <PackageReference Include="Azure.Identity" Version="1.10.4" />
  <PackageReference Include="Azure.Security.KeyVault.Secrets" Version="4.5.0" />
  <PackageReference Include="Microsoft.Extensions.Azure" Version="1.7.1" />
</ItemGroup>
```

## Modelos de Entidade

Vamos criar modelos que herdam de `ITableEntity` para cada tabela definida no modelo de dados.

### CompetitionEntity

```csharp
using Azure;
using Azure.Data.Tables;
using System;

namespace CompetitionApp.Models.Entities
{
    public class CompetitionEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = "Competition";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.Now;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
```

### ParticipantEntity

```csharp
using Azure;
using Azure.Data.Tables;
using System;

namespace CompetitionApp.Models.Entities
{
    public class ParticipantEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = "Participant";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
```

### ResultEntity

```csharp
using Azure;
using Azure.Data.Tables;
using System;

namespace CompetitionApp.Models.Entities
{
    public class ResultEntity : ITableEntity
    {
        public string PartitionKey { get; set; } // CompetitionId
        public string RowKey { get; set; } // ParticipantId_RoundNumber
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        
        public string ParticipantId { get; set; } = string.Empty;
        public string ParticipantName { get; set; } = string.Empty;
        public string CompetitionId { get; set; } = string.Empty;
        public string CompetitionName { get; set; } = string.Empty;
        public int RoundNumber { get; set; }
        public decimal TimeInSeconds { get; set; }
        public int BravoCount { get; set; }
        public int CharlieCount { get; set; }
        public int MissCount { get; set; }
        public int FaltaCount { get; set; }
        public int VitimaCount { get; set; }
        public int PlateCount { get; set; }
        public decimal TotalTime { get; set; }
        public bool IsEliminated { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
```

### FinalResultEntity

```csharp
using Azure;
using Azure.Data.Tables;
using System;

namespace CompetitionApp.Models.Entities
{
    public class FinalResultEntity : ITableEntity
    {
        public string PartitionKey { get; set; } // CompetitionId
        public string RowKey { get; set; } // ParticipantId
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        
        public string ParticipantId { get; set; } = string.Empty;
        public string ParticipantName { get; set; } = string.Empty;
        public string CompetitionId { get; set; } = string.Empty;
        public string CompetitionName { get; set; } = string.Empty;
        public decimal Round1Time { get; set; }
        public decimal Round2Time { get; set; }
        public decimal BestTime { get; set; }
        public int BestRound { get; set; }
        public int Position { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
```

## Configuração do Azure Key Vault

Para armazenar e recuperar credenciais de forma segura, usaremos o Azure Key Vault.

### Configuração no appsettings.json

```json
{
  "KeyVault": {
    "VaultUri": "https://your-key-vault-name.vault.azure.net/"
  },
  "AzureTableStorage": {
    "ConnectionStringSecretName": "AzureTableStorageConnectionString"
  }
}
```

### Configuração no Program.cs

```csharp
using Azure.Identity;
using Microsoft.Extensions.Azure;

// Adicionar serviços do Azure
builder.Services.AddAzureClients(clientBuilder =>
{
    // Configurar cliente do Key Vault
    clientBuilder.AddSecretClient(new Uri(builder.Configuration["KeyVault:VaultUri"]));
    
    // Configurar autenticação
    clientBuilder.UseCredential(new DefaultAzureCredential());
});

// Adicionar serviços do Azure Table Storage
builder.Services.AddSingleton<ITableStorageService, TableStorageService>();
```

## Serviço para Azure Table Storage

Vamos criar um serviço que encapsula as operações com o Azure Table Storage.

```csharp
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
```

## Serviços Específicos para Cada Entidade

Para facilitar o uso, vamos criar serviços específicos para cada tipo de entidade.

### CompetitionService

```csharp
using CompetitionApp.Models.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompetitionApp.Services
{
    public interface ICompetitionService
    {
        Task<IEnumerable<CompetitionEntity>> GetAllCompetitionsAsync();
        Task<CompetitionEntity> GetCompetitionByIdAsync(string id);
        Task<CompetitionEntity> CreateCompetitionAsync(string name, string description, DateTime date);
        Task UpdateCompetitionAsync(CompetitionEntity competition);
        Task DeleteCompetitionAsync(string id);
    }

    public class CompetitionService : ICompetitionService
    {
        private const string TableName = "Competitions";
        private readonly ITableStorageService _tableStorageService;

        public CompetitionService(ITableStorageService tableStorageService)
        {
            _tableStorageService = tableStorageService;
        }

        public async Task<IEnumerable<CompetitionEntity>> GetAllCompetitionsAsync()
        {
            return await _tableStorageService.QueryEntitiesAsync<CompetitionEntity>(
                TableName, 
                $"PartitionKey eq 'Competition'"
            );
        }

        public async Task<CompetitionEntity> GetCompetitionByIdAsync(string id)
        {
            return await _tableStorageService.GetEntityAsync<CompetitionEntity>(
                TableName, 
                "Competition", 
                id
            );
        }

        public async Task<CompetitionEntity> CreateCompetitionAsync(string name, string description, DateTime date)
        {
            var competition = new CompetitionEntity
            {
                Name = name,
                Description = description,
                Date = date,
                CreatedAt = DateTime.Now
            };

            await _tableStorageService.AddEntityAsync(TableName, competition);
            return competition;
        }

        public async Task UpdateCompetitionAsync(CompetitionEntity competition)
        {
            await _tableStorageService.UpdateEntityAsync(TableName, competition);
        }

        public async Task DeleteCompetitionAsync(string id)
        {
            await _tableStorageService.DeleteEntityAsync<CompetitionEntity>(
                TableName, 
                "Competition", 
                id
            );
        }
    }
}
```

### ParticipantService

```csharp
using CompetitionApp.Models.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompetitionApp.Services
{
    public interface IParticipantService
    {
        Task<IEnumerable<ParticipantEntity>> GetAllParticipantsAsync();
        Task<ParticipantEntity> GetParticipantByIdAsync(string id);
        Task<ParticipantEntity> CreateParticipantAsync(string name, string email);
        Task UpdateParticipantAsync(ParticipantEntity participant);
        Task DeleteParticipantAsync(string id);
    }

    public class ParticipantService : IParticipantService
    {
        private const string TableName = "Participants";
        private readonly ITableStorageService _tableStorageService;

        public ParticipantService(ITableStorageService tableStorageService)
        {
            _tableStorageService = tableStorageService;
        }

        public async Task<IEnumerable<ParticipantEntity>> GetAllParticipantsAsync()
        {
            return await _tableStorageService.QueryEntitiesAsync<ParticipantEntity>(
                TableName, 
                $"PartitionKey eq 'Participant'"
            );
        }

        public async Task<ParticipantEntity> GetParticipantByIdAsync(string id)
        {
            return await _tableStorageService.GetEntityAsync<ParticipantEntity>(
                TableName, 
                "Participant", 
                id
            );
        }

        public async Task<ParticipantEntity> CreateParticipantAsync(string name, string email)
        {
            var participant = new ParticipantEntity
            {
                Name = name,
                Email = email,
                CreatedAt = DateTime.Now
            };

            await _tableStorageService.AddEntityAsync(TableName, participant);
            return participant;
        }

        public async Task UpdateParticipantAsync(ParticipantEntity participant)
        {
            await _tableStorageService.UpdateEntityAsync(TableName, participant);
        }

        public async Task DeleteParticipantAsync(string id)
        {
            await _tableStorageService.DeleteEntityAsync<ParticipantEntity>(
                TableName, 
                "Participant", 
                id
            );
        }
    }
}
```

### ResultService

```csharp
using CompetitionApp.Models;
using CompetitionApp.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompetitionApp.Services
{
    public interface IResultService
    {
        Task<IEnumerable<ResultEntity>> GetResultsByCompetitionIdAsync(string competitionId);
        Task<IEnumerable<ResultEntity>> GetResultsByParticipantIdAsync(string participantId);
        Task<ResultEntity> GetResultAsync(string competitionId, string participantId, int roundNumber);
        Task<ResultEntity> SaveResultAsync(string competitionId, string competitionName, string participantId, string participantName, int roundNumber, Participant result);
        Task DeleteResultAsync(string competitionId, string participantId, int roundNumber);
    }

    public class ResultService : IResultService
    {
        private const string TableName = "Results";
        private readonly ITableStorageService _tableStorageService;

        public ResultService(ITableStorageService tableStorageService)
        {
            _tableStorageService = tableStorageService;
        }

        public async Task<IEnumerable<ResultEntity>> GetResultsByCompetitionIdAsync(string competitionId)
        {
            return await _tableStorageService.QueryEntitiesAsync<ResultEntity>(
                TableName, 
                $"PartitionKey eq '{competitionId}'"
            );
        }

        public async Task<IEnumerable<ResultEntity>> GetResultsByParticipantIdAsync(string participantId)
        {
            // Isso requer uma varredura de tabela com filtro secundário
            var allResults = await _tableStorageService.QueryEntitiesAsync<ResultEntity>(TableName);
            return allResults.Where(r => r.ParticipantId == participantId);
        }

        public async Task<ResultEntity> GetResultAsync(string competitionId, string participantId, int roundNumber)
        {
            string rowKey = $"{participantId}_{roundNumber}";
            return await _tableStorageService.GetEntityAsync<ResultEntity>(
                TableName, 
                competitionId, 
                rowKey
            );
        }

        public async Task<ResultEntity> SaveResultAsync(string competitionId, string competitionName, string participantId, string participantName, int roundNumber, Participant result)
        {
            string rowKey = $"{participantId}_{roundNumber}";
            
            // Verificar se já existe
            var existingResult = await _tableStorageService.GetEntityAsync<ResultEntity>(
                TableName, 
                competitionId, 
                rowKey
            );

            var resultEntity = existingResult ?? new ResultEntity
            {
                PartitionKey = competitionId,
                RowKey = rowKey,
                CompetitionId = competitionId,
                CompetitionName = competitionName,
                ParticipantId = participantId,
                ParticipantName = participantName,
                RoundNumber = roundNumber,
                CreatedAt = DateTime.Now
            };

            // Atualizar propriedades
            resultEntity.TimeInSeconds = result.TimeInSeconds;
            resultEntity.BravoCount = result.BravoCount;
            resultEntity.CharlieCount = result.CharlieCount;
            resultEntity.MissCount = result.MissCount;
            resultEntity.FaltaCount = result.FaltaCount;
            resultEntity.VitimaCount = result.VitimaCount ?? 0;
            resultEntity.PlateCount = result.PlateCount ?? 0;
            resultEntity.TotalTime = result.CalculateTotalTime();
            resultEntity.IsEliminated = result.IsEliminated;
            resultEntity.UpdatedAt = DateTime.Now;

            if (existingResult == null)
            {
                await _tableStorageService.AddEntityAsync(TableName, resultEntity);
            }
            else
            {
                await _tableStorageService.UpdateEntityAsync(TableName, resultEntity);
            }

            return resultEntity;
        }

        public async Task DeleteResultAsync(string competitionId, string participantId, int roundNumber)
        {
            string rowKey = $"{participantId}_{roundNumber}";
            await _tableStorageService.DeleteEntityAsync<ResultEntity>(
                TableName, 
                competitionId, 
                rowKey
            );
        }
    }
}
```

### FinalResultService

```csharp
using CompetitionApp.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompetitionApp.Services
{
    public interface IFinalResultService
    {
        Task<IEnumerable<FinalResultEntity>> GetFinalResultsByCompetitionIdAsync(string competitionId);
        Task<IEnumerable<FinalResultEntity>> GetFinalResultsByParticipantIdAsync(string participantId);
        Task<FinalResultEntity> GetFinalResultAsync(string competitionId, string participantId);
        Task<FinalResultEntity> SaveFinalResultAsync(FinalResultEntity finalResult);
        Task DeleteFinalResultAsync(string competitionId, string participantId);
        Task<IEnumerable<FinalResultEntity>> CalculateAndSaveFinalResultsAsync(string competitionId, string competitionName);
    }

    public class FinalResultService : IFinalResultService
    {
        private const string TableName = "FinalResults";
        private readonly ITableStorageService _tableStorageService;
        private readonly IResultService _resultService;

        public FinalResultService(ITableStorageService tableStorageService, IResultService resultService)
        {
            _tableStorageService = tableStorageService;
            _resultService = resultService;
        }

        public async Task<IEnumerable<FinalResultEntity>> GetFinalResultsByCompetitionIdAsync(string competitionId)
        {
            return await _tableStorageService.QueryEntitiesAsync<FinalResultEntity>(
                TableName, 
                $"PartitionKey eq '{competitionId}'"
            );
        }

        public async Task<IEnumerable<FinalResultEntity>> GetFinalResultsByParticipantIdAsync(string participantId)
        {
            // Isso requer uma varredura de tabela com filtro secundário
            var allResults = await _tableStorageService.QueryEntitiesAsync<FinalResultEntity>(TableName);
            return allResults.Where(r => r.ParticipantId == participantId);
        }

        public async Task<FinalResultEntity> GetFinalResultAsync(string competitionId, string participantId)
        {
            return await _tableStorageService.GetEntityAsync<FinalResultEntity>(
                TableName, 
                competitionId, 
                participantId
            );
        }

        public async Task<FinalResultEntity> SaveFinalResultAsync(FinalResultEntity finalResult)
        {
            var existingResult = await _tableStorageService.GetEntityAsync<FinalResultEntity>(
                TableName, 
                finalResult.PartitionKey, 
                finalResult.RowKey
            );

            if (existingResult == null)
            {
                await _tableStorageService.AddEntityAsync(TableName, finalResult);
            }
            else
            {
                finalResult.UpdatedAt = DateTime.Now;
                await _tableStorageService.UpdateEntityAsync(TableName, finalResult);
            }

            return finalResult;
        }

        public async Task DeleteFinalResultAsync(string competitionId, string participantId)
        {
            await _tableStorageService.DeleteEntityAsync<FinalResultEntity>(
                TableName, 
                competitionId, 
                participantId
            );
        }

        public async Task<IEnumerable<FinalResultEntity>> CalculateAndSaveFinalResultsAsync(string competitionId, string competitionName)
        {
            // Obter todos os resultados da competição
            var results = await _resultService.GetResultsByCompetitionIdAsync(competitionId);
            
            // Agrupar por participante
            var participantResults = results.GroupBy(r => r.ParticipantId);
            
            var finalResults = new List<FinalResultEntity>();
            
            foreach (var group in participantResults)
            {
                var participantId = group.Key;
                var participantName = group.First().ParticipantName;
                
                var round1Result = group.FirstOrDefault(r => r.RoundNumber == 1);
                var round2Result = group.FirstOrDefault(r => r.RoundNumber == 2);
                
                if (round1Result == null && round2Result == null)
                {
                    continue;
                }
                
                decimal round1Time = round1Result?.TotalTime ?? decimal.MaxValue;
                decimal round2Time = round2Result?.TotalTime ?? decimal.MaxValue;
                
                decimal bestTime;
                int bestRound;
                
                if (round1Time <= round2Time)
                {
                    bestTime = round1Time;
                    bestRound = 1;
                }
                else
                {
                    bestTime = round2Time;
                    bestRound = 2;
                }
                
                var finalResult = new FinalResultEntity
                {
                    PartitionKey = competitionId,
                    RowKey = participantId,
                    ParticipantId = participantId,
                    ParticipantName = participantName,
                    CompetitionId = competitionId,
                    CompetitionName = competitionName,
                    Round1Time = round1Time == decimal.MaxValue ? 0 : round1Time,
                    Round2Time = round2Time == decimal.MaxValue ? 0 : round2Time,
                    BestTime = bestTime == decimal.MaxValue ? 0 : bestTime,
                    BestRound = bestRound,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                
                finalResults.Add(finalResult);
            }
            
            // Calcular posições
            int position = 1;
            foreach (var result in finalResults.OrderBy(r => r.BestTime))
            {
                result.Position = position++;
                await SaveFinalResultAsync(result);
            }
            
            return finalResults;
        }
    }
}
```

## Registro dos Serviços no Program.cs

```csharp
// Registrar serviços
builder.Services.AddSingleton<ITableStorageService, TableStorageService>();
builder.Services.AddScoped<ICompetitionService, CompetitionService>();
builder.Services.AddScoped<IParticipantService, ParticipantService>();
builder.Services.AddScoped<IResultService, ResultService>();
builder.Services.AddScoped<IFinalResultService, FinalResultService>();
```

Esta implementação fornece uma base sólida para integrar o Azure Table Storage e o Azure Key Vault na aplicação de competição, permitindo o armazenamento seguro e eficiente de dados históricos de competições e participantes.
