# Competition App - PostgreSQL Version

Sistema de gerenciamento de competições esportivas usando ASP.NET Core e PostgreSQL.

## 🎯 Características

- **Backend**: ASP.NET Core 8.0 com Razor Pages
- **Banco de dados**: PostgreSQL com Entity Framework Core
- **Deploy**: Azure App Service com Azure Database for PostgreSQL
- **CI/CD**: GitHub Actions automatizado

## 🚀 Início Rápido

### Pré-requisitos

- .NET 8.0 SDK
- PostgreSQL (local ou Azure)
- Azure CLI (para deploy)

### Configuração Local

1. **Clone o repositório**
```bash
git clone <seu-repositorio>
cd CompetitionApp_Clean
```

2. **Configure a connection string**
```bash
# Opção 1: appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=competitiondb;Username=postgres;Password=password;"
  }
}

# Opção 2: Variável de ambiente
export DATABASE_CONNECTION_STRING="Host=localhost;Database=competitiondb;Username=postgres;Password=password;"
```

3. **Execute migrações**
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

4. **Execute a aplicação**
```bash
dotnet run
```

## ☁️ Deploy no Azure

### 1. Criar banco PostgreSQL
```bash
./Scripts/setup-azure-postgresql.sh \
  -g "CompetitionApp-RG" \
  -s "competitionapp-db" \
  -u "dbadmin" \
  -p "SuaSenhaSegura123!"
```

### 2. Configurar GitHub Secrets
- `DATABASE_CONNECTION_STRING`: Connection string do PostgreSQL
- `AZURE_WEBAPP_PUBLISH_PROFILE`: Perfil de publicação do App Service

### 3. Deploy automático
- Push para branch `main` executa deploy automaticamente
- Migrações são aplicadas automaticamente após deploy

## 📊 Estrutura do Banco

```sql
competitions
├── id (PK)
├── name
├── description
├── competition_date
└── timestamps

participants
├── id (PK)
├── name (UNIQUE)
├── email
└── timestamps

competition_participants
├── id (PK)
├── competition_id (FK)
├── participant_id (FK)
└── registered_at

results
├── id (PK)
├── competition_id (FK)
├── participant_id (FK)
├── round_number
├── time_in_seconds
├── penalty_counts
├── total_time
├── is_eliminated
└── timestamps

final_results
├── id (PK)
├── competition_id (FK)
├── participant_id (FK)
├── position
├── round1_time
├── round2_time
├── best_time
├── best_round
└── timestamps
```

## 🔧 Comandos Úteis

```bash
# Restaurar dependências
dotnet restore

# Compilar
dotnet build

# Executar testes
dotnet test

# Criar migração
dotnet ef migrations add NomeDaMigracao

# Aplicar migrações
dotnet ef database update

# Remover última migração
dotnet ef migrations remove

# Ver status das migrações
dotnet ef migrations list
```

## 📁 Estrutura do Projeto

```
CompetitionApp_Clean/
├── Data/
│   └── CompetitionDbContext.cs
├── Models/
│   └── Models.cs
├── Services/
│   ├── Services.cs
│   └── ResultServices.cs
├── Pages/
│   ├── Participants/
│   ├── Rounds/
│   ├── Results/
│   ├── History/
│   └── Shared/
├── Scripts/
│   └── setup-azure-postgresql.sh
├── .github/workflows/
│   └── azure-deploy-postgresql.yml
└── wwwroot/
```

## 🛠️ Desenvolvimento

### Adicionando novas funcionalidades

1. **Criar modelo** em `Models/Models.cs`
2. **Adicionar DbSet** em `CompetitionDbContext.cs`
3. **Criar serviço** em `Services/`
4. **Registrar serviço** em `Program.cs`
5. **Criar páginas** em `Pages/`
6. **Criar migração** e aplicar

### Boas práticas

- Use logging em todos os serviços
- Implemente tratamento de erros
- Valide dados de entrada
- Use transações para operações complexas
- Mantenha connection strings seguras

## 🔒 Segurança

- Connection strings em variáveis de ambiente
- Validação de entrada em todos os formulários
- Logs não expõem dados sensíveis
- HTTPS obrigatório em produção

## 📈 Performance

- Índices em colunas frequentemente consultadas
- Eager loading com `Include()` quando necessário
- Paginação para listas grandes
- Connection pooling habilitado

## 🐛 Troubleshooting

### Erro de conexão com banco
```bash
# Verificar connection string
echo $DATABASE_CONNECTION_STRING

# Testar conexão
psql "Host=servidor;Database=db;Username=user;Password=pass"
```

### Erro de migração
```bash
# Resetar migrações
dotnet ef database drop
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Erro de deploy
- Verificar secrets do GitHub
- Confirmar connection string
- Verificar logs do App Service

## 📞 Suporte

Para problemas ou dúvidas:
1. Verificar logs da aplicação
2. Consultar documentação do Entity Framework
3. Verificar status do Azure Database

## 🎉 Funcionalidades

- ✅ Cadastro de participantes
- ✅ Registro de resultados (2 rodadas)
- ✅ Cálculo automático de classificação
- ✅ Histórico de competições
- ✅ Export de resultados
- ✅ Interface responsiva
- ✅ Deploy automatizado
- ✅ Backup automático (Azure)
- ✅ Logs detalhados
- ✅ Tratamento de erros

