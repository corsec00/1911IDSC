#!/bin/bash

# Script para reorganizar arquivos da migração PostgreSQL
# Execute este script na raiz do projeto CompetitionApp

set -e  # Parar em caso de erro

echo "🚀 REORGANIZAÇÃO DE ARQUIVOS - MIGRAÇÃO POSTGRESQL"
echo "=================================================="
echo ""

# Verificar se estamos na pasta correta
if [ ! -f "CompetitionApp.csproj" ]; then
    echo "❌ Erro: Execute este script na raiz do projeto CompetitionApp"
    echo "   (onde está localizado o arquivo CompetitionApp.csproj)"
    exit 1
fi

echo "📁 Projeto encontrado: $(pwd)"
echo ""

# Função para criar backup com timestamp
create_backup() {
    local file=$1
    local backup_name="${file}_backup_$(date +%Y%m%d_%H%M%S)"
    
    if [ -f "$file" ]; then
        cp "$file" "$backup_name"
        echo "   ✅ Backup criado: $backup_name"
    fi
}

# 1. CRIAR BACKUPS
echo "1️⃣ Criando backups dos arquivos existentes..."
create_backup "CompetitionApp.csproj"
create_backup "Program.cs"
create_backup "appsettings.json"
echo ""

# 2. CRIAR ESTRUTURA DE PASTAS
echo "2️⃣ Criando estrutura de pastas..."

# Criar pasta Data se não existir
if [ ! -d "Data" ]; then
    mkdir -p Data
    echo "   ✅ Pasta Data/ criada"
else
    echo "   ℹ️  Pasta Data/ já existe"
fi

# Criar pasta Scripts se não existir
if [ ! -d "Scripts" ]; then
    mkdir -p Scripts
    echo "   ✅ Pasta Scripts/ criada"
else
    echo "   ℹ️  Pasta Scripts/ já existe"
fi

# Criar pasta .github/workflows se não existir
if [ ! -d ".github/workflows" ]; then
    mkdir -p .github/workflows
    echo "   ✅ Pasta .github/workflows/ criada"
else
    echo "   ℹ️  Pasta .github/workflows/ já existe"
fi

echo ""

# 3. MOVER ARQUIVOS PARA LOCAIS CORRETOS
echo "3️⃣ Organizando arquivos..."

# Função para mover arquivo se existir
move_if_exists() {
    local source=$1
    local destination=$2
    
    if [ -f "$source" ]; then
        mv "$source" "$destination"
        echo "   ✅ $source → $destination"
    else
        echo "   ⚠️  Arquivo não encontrado: $source"
    fi
}

# DbContext
move_if_exists "CompetitionDbContext.cs" "Data/"

# Modelos PostgreSQL
move_if_exists "PostgreSQLModels.cs" "Models/"

# Serviços PostgreSQL
move_if_exists "PostgreSQLServices.cs" "Services/"
move_if_exists "PostgreSQLResultServices.cs" "Services/"

# Scripts
move_if_exists "Setup-AzurePostgreSQL.ps1" "Scripts/"
move_if_exists "setup-azure-postgresql.sh" "Scripts/"
move_if_exists "init-database.sql" "Scripts/"

# Tornar script executável
if [ -f "Scripts/setup-azure-postgresql.sh" ]; then
    chmod +x Scripts/setup-azure-postgresql.sh
    echo "   ✅ Script bash tornado executável"
fi

# Workflow
move_if_exists "azure-deploy-postgresql.yml" ".github/workflows/"

echo ""

# 4. SUBSTITUIR ARQUIVOS PRINCIPAIS
echo "4️⃣ Substituindo arquivos principais..."

# Função para substituir arquivo
replace_file() {
    local source=$1
    local target=$2
    
    if [ -f "$source" ]; then
        cp "$source" "$target"
        rm "$source"
        echo "   ✅ $target atualizado"
    else
        echo "   ⚠️  Arquivo fonte não encontrado: $source"
    fi
}

replace_file "CompetitionApp_PostgreSQL.csproj" "CompetitionApp.csproj"
replace_file "Program_PostgreSQL.cs" "Program.cs"
replace_file "appsettings_PostgreSQL.json" "appsettings.json"

echo ""

# 5. ORGANIZAR DOCUMENTAÇÃO
echo "5️⃣ Organizando documentação..."

# Manter documentação na raiz
docs_in_root=(
    "PostgreSQL-Migration-Guide.md"
    "MIGRATION-SUMMARY.md"
    "FILE-ORGANIZATION-GUIDE.md"
)

for doc in "${docs_in_root[@]}"; do
    if [ -f "$doc" ]; then
        echo "   ✅ $doc mantido na raiz"
    fi
done

echo ""

# 6. LIMPEZA
echo "6️⃣ Limpando arquivos temporários..."

# Remover arquivos temporários que podem ter sobrado
temp_files=(
    "*_PostgreSQL.*"
    "*_Debug.*"
    "*.tmp"
)

for pattern in "${temp_files[@]}"; do
    if ls $pattern 1> /dev/null 2>&1; then
        rm -f $pattern
        echo "   ✅ Removidos: $pattern"
    fi
done

echo ""

# 7. VERIFICAÇÃO FINAL
echo "7️⃣ Verificação da estrutura final..."

echo ""
echo "📁 ESTRUTURA ATUAL DO PROJETO:"
echo "├── Data/"
if [ -d "Data" ]; then
    ls Data/ | sed 's/^/│   ├── /'
fi

echo "├── Models/"
if [ -d "Models" ]; then
    ls Models/ | head -5 | sed 's/^/│   ├── /'
    if [ $(ls Models/ | wc -l) -gt 5 ]; then
        echo "│   └── ... ($(ls Models/ | wc -l) arquivos total)"
    fi
fi

echo "├── Services/"
if [ -d "Services" ]; then
    ls Services/ | head -5 | sed 's/^/│   ├── /'
    if [ $(ls Services/ | wc -l) -gt 5 ]; then
        echo "│   └── ... ($(ls Services/ | wc -l) arquivos total)"
    fi
fi

echo "├── Scripts/"
if [ -d "Scripts" ]; then
    ls Scripts/ | sed 's/^/│   ├── /'
fi

echo "├── .github/workflows/"
if [ -d ".github/workflows" ]; then
    ls .github/workflows/ | sed 's/^/│   ├── /'
fi

echo "└── Arquivos principais:"
echo "    ├── CompetitionApp.csproj"
echo "    ├── Program.cs"
echo "    ├── appsettings.json"
echo "    └── *.md (documentação)"

echo ""

# 8. PRÓXIMOS PASSOS
echo "✅ REORGANIZAÇÃO CONCLUÍDA COM SUCESSO!"
echo ""
echo "🎯 PRÓXIMOS PASSOS:"
echo ""
echo "1️⃣ Restaurar dependências:"
echo "   dotnet restore"
echo ""
echo "2️⃣ Configurar connection string:"
echo "   export DATABASE_CONNECTION_STRING=\"sua_connection_string\""
echo "   # ou editar appsettings.json"
echo ""
echo "3️⃣ Criar migração inicial:"
echo "   dotnet ef migrations add InitialCreate"
echo ""
echo "4️⃣ Aplicar migrações:"
echo "   dotnet ef database update"
echo ""
echo "5️⃣ Testar aplicação:"
echo "   dotnet run"
echo ""
echo "📚 Para mais detalhes, consulte:"
echo "   - PostgreSQL-Migration-Guide.md"
echo "   - MIGRATION-SUMMARY.md"
echo "   - FILE-ORGANIZATION-GUIDE.md"
echo ""
echo "🔄 Para rollback, use os arquivos de backup criados."

