#!/bin/bash

# Script para desabilitar o workflow antigo do GitHub Actions
# Execute na raiz do projeto

set -e

echo "🔄 DESABILITANDO WORKFLOW ANTIGO DO GITHUB ACTIONS"
echo "================================================="
echo ""

OLD_WORKFLOW=".github/workflows/azure-deploy.yml"
NEW_WORKFLOW=".github/workflows/azure-deploy-postgresql.yml"

# Verificar se estamos na pasta correta
if [ ! -d ".github" ]; then
    echo "❌ Erro: Execute este script na raiz do projeto"
    echo "   (onde está localizada a pasta .github/)"
    exit 1
fi

echo "📁 Projeto encontrado: $(pwd)"
echo ""

# Verificar se os arquivos existem
if [ ! -f "$OLD_WORKFLOW" ]; then
    echo "ℹ️  Workflow antigo não encontrado: $OLD_WORKFLOW"
    echo "   Provavelmente já foi removido ou renomeado."
else
    echo "✅ Workflow antigo encontrado: $OLD_WORKFLOW"
fi

if [ ! -f "$NEW_WORKFLOW" ]; then
    echo "⚠️  Novo workflow não encontrado: $NEW_WORKFLOW"
    echo "   Certifique-se de que o arquivo azure-deploy-postgresql.yml existe"
else
    echo "✅ Novo workflow encontrado: $NEW_WORKFLOW"
fi

echo ""

# Se o workflow antigo existe, desabilitá-lo
if [ -f "$OLD_WORKFLOW" ]; then
    echo "1️⃣ Criando backup do workflow antigo..."
    
    # Criar backup com timestamp
    BACKUP_FILE="${OLD_WORKFLOW}.backup_$(date +%Y%m%d_%H%M%S)"
    cp "$OLD_WORKFLOW" "$BACKUP_FILE"
    echo "   ✅ Backup criado: $BACKUP_FILE"
    
    echo ""
    echo "2️⃣ Desabilitando workflow antigo..."
    
    # Criar versão desabilitada
    cat > "$OLD_WORKFLOW" << 'EOF'
name: "[DEPRECATED] Azure Deploy - Table Storage"

# 🚨 ESTE WORKFLOW FOI SUBSTITUÍDO POR azure-deploy-postgresql.yml
# Desabilitado automaticamente para evitar conflitos

on:
  workflow_dispatch:
    inputs:
      force_deprecated:
        description: 'ATENÇÃO: Este workflow está DEPRECATED. Digite "FORCE_DEPRECATED" para executar'
        required: true
        default: 'NÃO_EXECUTAR'
        type: choice
        options:
        - 'NÃO_EXECUTAR'
        - 'FORCE_DEPRECATED'

env:
  DEPRECATED_DATE: '2024-01-01'
  REPLACEMENT_WORKFLOW: 'azure-deploy-postgresql.yml'

jobs:
  deprecated-warning:
    runs-on: ubuntu-latest
    steps:
    - name: 🚨 Workflow Deprecated Warning
      run: |
        echo "=============================================="
        echo "🚨 ESTE WORKFLOW FOI SUBSTITUÍDO!"
        echo "=============================================="
        echo ""
        echo "❌ Workflow atual: azure-deploy.yml (Table Storage)"
        echo "✅ Novo workflow: azure-deploy-postgresql.yml (PostgreSQL)"
        echo ""
        echo "📅 Deprecated em: ${{ env.DEPRECATED_DATE }}"
        echo "🔄 Status: DESABILITADO"
        echo ""
        echo "Para usar a aplicação atualizada:"
        echo "1. Use o workflow: ${{ env.REPLACEMENT_WORKFLOW }}"
        echo "2. Configure os secrets do PostgreSQL"
        echo "3. Faça push para a branch main"
        echo ""
        echo "=============================================="
        
        if [ "${{ github.event.inputs.force_deprecated }}" != "FORCE_DEPRECATED" ]; then
          echo "❌ Execução cancelada automaticamente."
          echo "   Este workflow está desabilitado para evitar conflitos."
          echo ""
          echo "Para forçar execução (NÃO RECOMENDADO):"
          echo "- Selecione 'FORCE_DEPRECATED' no input"
          echo "- Execute manualmente via GitHub Actions UI"
          exit 1
        fi
        
        echo "⚠️ ATENÇÃO: Continuando com workflow DEPRECATED..."
        echo "   Isso pode causar conflitos com o novo sistema PostgreSQL!"
        sleep 5

  # Jobs originais comentados para evitar execução acidental
  # 
  # NOTA: Para restaurar funcionalidade completa, 
  # restaure do arquivo de backup criado automaticamente
  #
  # build:
  #   runs-on: ubuntu-latest
  #   needs: deprecated-warning
  #   if: github.event.inputs.force_deprecated == 'FORCE_DEPRECATED'
  #   steps:
  #   - name: Checkout
  #     uses: actions/checkout@v4
  #   # ... resto dos steps originais
  #
  # deploy:
  #   runs-on: ubuntu-latest  
  #   needs: build
  #   if: github.event.inputs.force_deprecated == 'FORCE_DEPRECATED'
  #   steps:
  #   - name: Deploy
  #     run: echo "Deploy do workflow deprecated..."
  #   # ... resto dos steps originais

  final-warning:
    runs-on: ubuntu-latest
    needs: deprecated-warning
    if: github.event.inputs.force_deprecated == 'FORCE_DEPRECATED'
    steps:
    - name: Final Warning
      run: |
        echo "🚨 WORKFLOW DEPRECATED EXECUTADO!"
        echo ""
        echo "Este workflow pode causar problemas:"
        echo "- Conflitos com PostgreSQL"
        echo "- Dados inconsistentes"
        echo "- Falhas de deploy"
        echo ""
        echo "Recomendação: Migre para azure-deploy-postgresql.yml"
EOF

    echo "   ✅ Workflow antigo desabilitado com sucesso"
    echo ""
    
    echo "3️⃣ Verificando resultado..."
    echo "   📄 Arquivo original: $OLD_WORKFLOW (desabilitado)"
    echo "   💾 Backup salvo em: $BACKUP_FILE"
    echo "   ✅ Novo workflow: $NEW_WORKFLOW (ativo)"
    echo ""
    
else
    echo "ℹ️  Workflow antigo não encontrado - nada para desabilitar"
    echo ""
fi

# Verificar estrutura final
echo "4️⃣ Estrutura atual dos workflows:"
echo ""
if [ -d ".github/workflows" ]; then
    echo "📁 .github/workflows/"
    for file in .github/workflows/*; do
        if [ -f "$file" ]; then
            filename=$(basename "$file")
            if [[ "$filename" == *"postgresql"* ]]; then
                echo "   ✅ $filename (ATIVO - PostgreSQL)"
            elif [[ "$filename" == "azure-deploy.yml" ]]; then
                echo "   ⚠️  $filename (DESABILITADO - Table Storage)"
            elif [[ "$filename" == *"backup"* ]]; then
                echo "   💾 $filename (BACKUP)"
            else
                echo "   📄 $filename"
            fi
        fi
    done
else
    echo "❌ Pasta .github/workflows não encontrada"
fi

echo ""
echo "✅ PROCESSO CONCLUÍDO!"
echo ""
echo "🎯 PRÓXIMOS PASSOS:"
echo ""
echo "1️⃣ Commit das alterações:"
echo "   git add .github/workflows/"
echo "   git commit -m 'Disable deprecated Table Storage workflow'"
echo ""
echo "2️⃣ Push para o repositório:"
echo "   git push origin main"
echo ""
echo "3️⃣ Verificar no GitHub:"
echo "   - Vá para Actions tab"
echo "   - Confirme que apenas azure-deploy-postgresql.yml está ativo"
echo "   - O workflow antigo aparecerá como 'workflow_dispatch' apenas"
echo ""
echo "🔄 ROLLBACK (se necessário):"
echo "   cp $BACKUP_FILE $OLD_WORKFLOW"
echo "   git add . && git commit -m 'Restore old workflow' && git push"
echo ""
echo "📚 DOCUMENTAÇÃO:"
echo "   Consulte GITHUB-WORKFLOWS-MANAGEMENT.md para mais detalhes"
echo ""
echo "🎉 Migração para PostgreSQL workflow concluída!"

