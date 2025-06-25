# Migração para Azure Database for PostgreSQL

Este documento descreve como migrar a aplicação de competição de Azure Table Storage para Azure Database for PostgreSQL.

## Índice

1. [Pré-requisitos](#pré-requisitos)
2. [Configuração do Azure](#configuração-do-azure)
3. [Configuração Local](#configuração-local)
4. [Migrações do Banco de Dados](#migrações-do-banco-de-dados)
5. [Configuração do GitHub Actions](#configuração-do-github-actions)
6. [Deploy da Aplicação](#deploy-da-aplicação)
7. [Troubleshooting](#troubleshooting)

## Pré-requisitos

### Software Necessário
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli) ou [Azure PowerShell](https://docs.microsoft.com/powershell/azure/install-az-ps)
- [PostgreSQL Client](https://www.postgresql.org/download/) (opcional, para testes locais)
- [Git](https://git-scm.com/downloads)

### Conta Azure
- Assinatura ativa do Azure
- Permissões para criar recursos (Resource Groups, PostgreSQL, App Service)

## Configuração do Azure

### Opção 1: Usando Azure CLI (Recomendado)

1. **Faça login no Azure:**
   ```bash
   az login
   ```

2. **Execute o script de configuração:**
   ```bash
   chmod +x Scripts/setup-azure-postgresql.sh
   ./Scripts/setup-azure-postgresql.sh \
     -g "CompetitionApp-RG" \
     -s "competitionapp-db" \
     -u "dbadmin" \
     -p "SuaSenhaSegura123!" \
     -a "CompetitionApp-Plan" \
     -w "competitionapp-web"
   ```

### Opção 2: Usando PowerShell

1. **Execute o script PowerShell:**
   ```powershell
   .\Scripts\Setup-AzurePostgreSQL.ps1 `
     -ResourceGroupName "CompetitionApp-RG" `
     -ServerName "competitionapp-db" `
     -AdminUsername "dbadmin" `
     -AdminPassword (ConvertTo-SecureString "SuaSenhaSegura123!" -AsPlainText -Force) `
     -AppServicePlanName "CompetitionApp-Plan" `
     -WebAppName "competitionapp-web"
   ```

### Opção 3: Manual via Portal Azure

1. **Criar Resource Group:**
   - Nome: `CompetitionApp-RG`
   - Região: `East US`

2. **Criar Azure Database for PostgreSQL:**
   - Nome do servidor: `competitionapp-db`
   - Versão: `13`
   - Compute + Storage: `General Purpose, 2 vCores`
   - Usuário admin: `dbadmin`
   - Senha: `SuaSenhaSegura123!`

3. **Configurar Firewall:**
   - Permitir serviços do Azure: `Sim`
   - Adicionar seu IP atual

4. **Criar banco de dados:**
   - Nome: `competitiondb`

## Configuração Local

### 1. Atualizar Arquivos do Projeto

Substitua os seguintes arquivos pelos novos:

- `CompetitionApp.csproj` → `CompetitionApp_PostgreSQL.csproj`
- `Program.cs` → `Program_PostgreSQL.cs`
- `appsettings.json` → `appsettings_PostgreSQL.json`

### 2. Adicionar Novos Arquivos

Copie os seguintes arquivos para o projeto:

- `Models/PostgreSQLModels.cs`
- `Data/CompetitionDbContext.cs`
- `Services/PostgreSQLServices.cs`
- `Services/PostgreSQLResultServices.cs`

### 3. Configurar Connection String

**Opção A: Variável de Ambiente (Recomendado)**
```bash
export DATABASE_CONNECTION_STRING="Host=competitionapp-db.postgres.database.azure.com;Database=competitiondb;Username=dbadmin@competitionapp-db;Password=SuaSenhaSegura123!;SSL Mode=Require;"
```

**Opção B: appsettings.json**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=competitionapp-db.postgres.database.azure.com;Database=competitiondb;Username=dbadmin@competitionapp-db;Password=SuaSenhaSegura123!;SSL Mode=Require;"
  }
}
```

### 4. Instalar Dependências

```bash
dotnet restore
```

## Migrações do Banco de Dados

### 1. Instalar Entity Framework Tools

```bash
dotnet tool install --global dotnet-ef
```

### 2. Criar Migração Inicial

```bash
dotnet ef migrations add InitialCreate
```

### 3. Aplicar Migrações

```bash
dotnet ef database update
```

### 4. Verificar Criação das Tabelas

Execute o script SQL de verificação:
```bash
psql -h competitionapp-db.postgres.database.azure.com -U dbadmin@competitionapp-db -d competitiondb -f Scripts/init-database.sql
```

## Configuração do GitHub Actions

### 1. Configurar Secrets

No repositório GitHub, vá em **Settings > Secrets and variables > Actions** e adicione:

- `DATABASE_CONNECTION_STRING`: Connection string do PostgreSQL
- `AZURE_WEBAPP_PUBLISH_PROFILE`: Perfil de publicação do App Service

### 2. Obter Publish Profile

```bash
az webapp deployment list-publishing-profiles \
  --resource-group CompetitionApp-RG \
  --name competitionapp-web \
  --xml
```

### 3. Configurar Workflow

Copie o arquivo `.github/workflows/azure-deploy-postgresql.yml` para seu repositório e atualize:

```yaml
env:
  AZURE_WEBAPP_NAME: 'competitionapp-web'  # Seu nome do Web App
```

## Deploy da Aplicação

### 1. Deploy Manual

```bash
# Build da aplicação
dotnet build --configuration Release

# Publicar
dotnet publish --configuration Release --output ./publish

# Deploy via Azure CLI
az webapp deployment source config-zip \
  --resource-group CompetitionApp-RG \
  --name competitionapp-web \
  --src publish.zip
```

### 2. Deploy Automático

Faça push para a branch `main`:

```bash
git add .
git commit -m "Migração para PostgreSQL"
git push origin main
```

O GitHub Actions executará automaticamente:
1. Build da aplicação
2. Testes
3. Deploy para Azure App Service
4. Execução das migrações do banco

## Estrutura do Banco de Dados

### Tabelas Criadas

1. **competitions**
   - `id` (SERIAL PRIMARY KEY)
   - `name` (VARCHAR(200))
   - `description` (VARCHAR(1000))
   - `competition_date` (TIMESTAMP)
   - `created_at`, `updated_at` (TIMESTAMP)

2. **participants**
   - `id` (SERIAL PRIMARY KEY)
   - `name` (VARCHAR(200))
   - `email` (VARCHAR(200))
   - `created_at`, `updated_at` (TIMESTAMP)

3. **competition_participants**
   - `id` (SERIAL PRIMARY KEY)
   - `competition_id` (INTEGER FK)
   - `participant_id` (INTEGER FK)
   - `registered_at` (TIMESTAMP)

4. **results**
   - `id` (SERIAL PRIMARY KEY)
   - `competition_id` (INTEGER FK)
   - `participant_id` (INTEGER FK)
   - `round_number` (INTEGER)
   - Campos de penalidades e tempos
   - `created_at`, `updated_at` (TIMESTAMP)

5. **final_results**
   - `id` (SERIAL PRIMARY KEY)
   - `competition_id` (INTEGER FK)
   - `participant_id` (INTEGER FK)
   - `position` (INTEGER)
   - Tempos das rodadas e melhor tempo
   - `created_at`, `updated_at` (TIMESTAMP)

### Índices e Constraints

- Índices únicos para evitar duplicatas
- Foreign keys com CASCADE DELETE
- Índices de performance para consultas
- Triggers para atualização automática de timestamps

## Troubleshooting

### Problemas Comuns

#### 1. Erro de Conexão com PostgreSQL

**Sintoma:** `Npgsql.NpgsqlException: Connection refused`

**Soluções:**
- Verificar se o servidor PostgreSQL está rodando
- Confirmar regras de firewall
- Validar connection string

#### 2. Erro de Autenticação

**Sintoma:** `password authentication failed`

**Soluções:**
- Verificar usuário e senha
- Confirmar formato da connection string
- Verificar se o usuário tem permissões

#### 3. Erro de SSL

**Sintoma:** `SSL connection required`

**Soluções:**
- Adicionar `SSL Mode=Require` na connection string
- Para desenvolvimento local: `SSL Mode=Prefer`

#### 4. Migrações Falham

**Sintoma:** `Unable to create migration`

**Soluções:**
```bash
# Limpar migrações
rm -rf Migrations/

# Recriar migração
dotnet ef migrations add InitialCreate

# Aplicar com força
dotnet ef database update --force
```

### Logs e Monitoramento

#### 1. Logs da Aplicação

```bash
# Ver logs do App Service
az webapp log tail --resource-group CompetitionApp-RG --name competitionapp-web
```

#### 2. Logs do PostgreSQL

No portal Azure:
1. Vá para o servidor PostgreSQL
2. **Monitoring > Logs**
3. Configure log queries

#### 3. Métricas de Performance

Monitor no portal Azure:
- CPU usage
- Memory usage
- Database connections
- Query performance

### Comandos Úteis

```bash
# Verificar status do servidor
az postgres server show --resource-group CompetitionApp-RG --name competitionapp-db

# Listar bancos de dados
az postgres db list --resource-group CompetitionApp-RG --server-name competitionapp-db

# Conectar via psql
psql -h competitionapp-db.postgres.database.azure.com -U dbadmin@competitionapp-db -d competitiondb

# Backup do banco
pg_dump -h competitionapp-db.postgres.database.azure.com -U dbadmin@competitionapp-db -d competitiondb > backup.sql

# Restaurar backup
psql -h competitionapp-db.postgres.database.azure.com -U dbadmin@competitionapp-db -d competitiondb < backup.sql
```

## Migração de Dados (Se Necessário)

Se você tem dados existentes no Azure Table Storage, crie um script de migração:

```csharp
// Exemplo de script de migração
public async Task MigrateFromTableStorage()
{
    // 1. Conectar ao Table Storage
    // 2. Ler dados existentes
    // 3. Converter para modelos PostgreSQL
    // 4. Inserir no PostgreSQL
}
```

## Considerações de Segurança

1. **Connection Strings:**
   - Nunca commitar senhas no código
   - Usar Azure Key Vault para produção
   - Rotacionar senhas regularmente

2. **Firewall:**
   - Configurar apenas IPs necessários
   - Usar VNet para isolamento
   - Monitorar tentativas de acesso

3. **Backup:**
   - Configurar backup automático
   - Testar restauração regularmente
   - Manter backups em região diferente

## Custos Estimados

- **Azure Database for PostgreSQL (Basic, 2 vCores):** ~$50/mês
- **App Service (Basic B1):** ~$13/mês
- **Storage e transferência:** ~$5/mês

**Total estimado:** ~$68/mês

## Próximos Passos

1. Testar a aplicação localmente
2. Executar deploy para ambiente de teste
3. Migrar dados existentes (se aplicável)
4. Configurar monitoramento e alertas
5. Documentar procedimentos operacionais
6. Treinar equipe nos novos procedimentos

