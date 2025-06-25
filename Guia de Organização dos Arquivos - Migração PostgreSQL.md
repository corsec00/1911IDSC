# Guia de Organização dos Arquivos - Migração PostgreSQL

## 📁 Estrutura Recomendada do Projeto

```
CompetitionApp/
├── 📁 Data/                          # NOVA PASTA
│   └── CompetitionDbContext.cs       # NOVO ARQUIVO
├── 📁 Models/
│   ├── Participant.cs                # EXISTENTE (manter)
│   ├── PenaltyConfiguration.cs       # EXISTENTE (manter)
│   ├── FinalResult.cs               # EXISTENTE (manter)
│   ├── PostgreSQLModels.cs          # NOVO ARQUIVO
│   └── 📁 Entities/                 # EXISTENTE (manter para compatibilidade)
│       ├── CompetitionEntity.cs      # EXISTENTE (manter)
│       ├── ParticipantEntity.cs      # EXISTENTE (manter)
│       ├── ResultEntity.cs          # EXISTENTE (manter)
│       └── FinalResultEntity.cs     # EXISTENTE (manter)
├── 📁 Services/
│   ├── CompetitionService.cs         # EXISTENTE (manter para compatibilidade)
│   ├── ParticipantService.cs         # EXISTENTE (manter)
│   ├── ResultService.cs             # EXISTENTE (manter para compatibilidade)
│   ├── FinalResultService.cs        # EXISTENTE (manter para compatibilidade)
│   ├── PostgreSQLServices.cs        # NOVO ARQUIVO
│   └── PostgreSQLResultServices.cs  # NOVO ARQUIVO
├── 📁 Scripts/                       # NOVA PASTA
│   ├── Setup-AzurePostgreSQL.ps1    # NOVO ARQUIVO
│   ├── setup-azure-postgresql.sh    # NOVO ARQUIVO
│   └── init-database.sql            # NOVO ARQUIVO
├── 📁 .github/workflows/
│   ├── azure-deploy.yml             # EXISTENTE (manter)
│   └── azure-deploy-postgresql.yml  # NOVO ARQUIVO
├── 📁 Pages/                         # EXISTENTE (manter toda estrutura)
├── 📁 Managers/                      # EXISTENTE (manter)
├── 📁 Infrastructure/                # EXISTENTE (manter)
├── 📁 wwwroot/                       # EXISTENTE (manter)
├── CompetitionApp.csproj            # SUBSTITUIR por CompetitionApp_PostgreSQL.csproj
├── Program.cs                       # SUBSTITUIR por Program_PostgreSQL.cs
├── appsettings.json                 # SUBSTITUIR por appsettings_PostgreSQL.json
├── PostgreSQL-Migration-Guide.md    # NOVO ARQUIVO (raiz)
├── MIGRATION-SUMMARY.md             # NOVO ARQUIVO (raiz)
└── README.md                        # EXISTENTE (atualizar)
```

## 🔄 Ações Necessárias por Arquivo

### **SUBSTITUIR (Backup + Replace)**

1. **CompetitionApp.csproj**
   ```bash
   # Fazer backup
   cp CompetitionApp.csproj CompetitionApp_TableStorage.csproj.bak
   
   # Substituir
   cp CompetitionApp_PostgreSQL.csproj CompetitionApp.csproj
   ```

2. **Program.cs**
   ```bash
   # Fazer backup
   cp Program.cs Program_TableStorage.cs.bak
   
   # Substituir
   cp Program_PostgreSQL.cs Program.cs
   ```

3. **appsettings.json**
   ```bash
   # Fazer backup
   cp appsettings.json appsettings_TableStorage.json.bak
   
   # Substituir
   cp appsettings_PostgreSQL.json appsettings.json
   ```

### **CRIAR NOVAS PASTAS**

```bash
# Criar pasta Data
mkdir -p Data

# Criar pasta Scripts
mkdir -p Scripts

# Criar pasta .github/workflows (se não existir)
mkdir -p .github/workflows
```

### **ADICIONAR NOVOS ARQUIVOS**

1. **Data/CompetitionDbContext.cs** - Contexto do Entity Framework
2. **Models/PostgreSQLModels.cs** - Novos modelos relacionais
3. **Services/PostgreSQLServices.cs** - Serviços para PostgreSQL
4. **Services/PostgreSQLResultServices.cs** - Serviços de resultados
5. **Scripts/** - Todos os scripts de configuração
6. **Documentação** - Guias na raiz do projeto

### **MANTER EXISTENTES (Para Compatibilidade)**

- Todos os arquivos em `Pages/`
- Todos os arquivos em `Managers/`
- Todos os arquivos em `Models/Entities/`
- Serviços antigos (para rollback se necessário)

## 🛠️ Script de Reorganização

Crie este script para automatizar a organização:

```bash
#!/bin/bash
# reorganize-files.sh

echo "Reorganizando arquivos para migração PostgreSQL..."

# 1. Criar backups dos arquivos que serão substituídos
echo "Criando backups..."
cp CompetitionApp.csproj CompetitionApp_TableStorage.csproj.bak
cp Program.cs Program_TableStorage.cs.bak
cp appsettings.json appsettings_TableStorage.json.bak

# 2. Criar novas pastas
echo "Criando estrutura de pastas..."
mkdir -p Data
mkdir -p Scripts
mkdir -p .github/workflows

# 3. Mover arquivos para locais corretos
echo "Movendo arquivos..."

# DbContext
mv CompetitionDbContext.cs Data/

# Modelos PostgreSQL
mv PostgreSQLModels.cs Models/

# Serviços PostgreSQL
mv PostgreSQLServices.cs Services/
mv PostgreSQLResultServices.cs Services/

# Scripts
mv Setup-AzurePostgreSQL.ps1 Scripts/
mv setup-azure-postgresql.sh Scripts/
mv init-database.sql Scripts/
chmod +x Scripts/setup-azure-postgresql.sh

# Workflow
mv azure-deploy-postgresql.yml .github/workflows/

# 4. Substituir arquivos principais
echo "Substituindo arquivos principais..."
cp CompetitionApp_PostgreSQL.csproj CompetitionApp.csproj
cp Program_PostgreSQL.cs Program.cs
cp appsettings_PostgreSQL.json appsettings.json

# 5. Limpar arquivos temporários
echo "Limpando arquivos temporários..."
rm -f CompetitionApp_PostgreSQL.csproj
rm -f Program_PostgreSQL.cs
rm -f appsettings_PostgreSQL.json

echo "Reorganização concluída!"
echo ""
echo "Próximos passos:"
echo "1. dotnet restore"
echo "2. dotnet ef migrations add InitialCreate"
echo "3. Configurar connection string"
echo "4. dotnet ef database update"
```

## 📋 Checklist de Migração

### ✅ **Fase 1: Preparação**
- [ ] Fazer backup do projeto atual
- [ ] Criar branch para migração: `git checkout -b postgresql-migration`
- [ ] Executar script de reorganização

### ✅ **Fase 2: Configuração**
- [ ] Executar `dotnet restore`
- [ ] Configurar connection string
- [ ] Criar migração inicial: `dotnet ef migrations add InitialCreate`

### ✅ **Fase 3: Teste Local**
- [ ] Executar aplicação localmente
- [ ] Testar criação de competição
- [ ] Testar salvamento de resultados
- [ ] Verificar histórico

### ✅ **Fase 4: Deploy**
- [ ] Configurar Azure PostgreSQL
- [ ] Configurar secrets no GitHub
- [ ] Fazer push da branch
- [ ] Testar em staging

### ✅ **Fase 5: Produção**
- [ ] Merge para main
- [ ] Deploy automático
- [ ] Migrar dados existentes (se necessário)
- [ ] Validar funcionamento

## 🔄 Estratégia de Rollback

Se precisar voltar ao Table Storage:

```bash
# Restaurar arquivos originais
cp CompetitionApp_TableStorage.csproj.bak CompetitionApp.csproj
cp Program_TableStorage.cs.bak Program.cs
cp appsettings_TableStorage.json.bak appsettings.json

# Remover migrações
rm -rf Migrations/

# Restaurar dependências
dotnet restore
```

## 📝 Notas Importantes

1. **Compatibilidade**: Os serviços antigos são mantidos para facilitar rollback
2. **Gradual**: Você pode migrar gradualmente, testando cada componente
3. **Backup**: Sempre mantenha backups antes de substituir arquivos
4. **Testes**: Teste cada fase antes de prosseguir
5. **Documentação**: Mantenha a documentação atualizada

Esta organização garante que o projeto permaneça limpo e facilita tanto a migração quanto possíveis rollbacks.

