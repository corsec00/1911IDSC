#!/bin/bash

# Script para criar Azure Database for PostgreSQL usando Azure CLI
# Execute este script no Azure Cloud Shell ou com Azure CLI instalado

# Parâmetros (modifique conforme necessário)
RESOURCE_GROUP_NAME="RG-1911SC"
LOCATION="centralus"
SERVER_NAME="1911databaseserver"
DATABASE_NAME="idsc1911competitiondb"
ADMIN_USERNAME="postgreadmin"
$ADMIN_PASSWORD="gjNi14kQ"
APP_SERVICE_PLAN_NAME="ASP-RG1911SC-9ca2"
WEB_APP_NAME="1911IDSC"

# Função para exibir ajuda
show_help() {
    echo "Uso: $0 [opções]"
    echo ""
    echo "Opções:"
    echo "  -g, --resource-group    Nome do Resource Group (obrigatório)"
    echo "  -l, --location         Localização do Azure (padrão: eastus)"
    echo "  -s, --server-name      Nome do servidor PostgreSQL (obrigatório)"
    echo "  -d, --database-name    Nome do banco de dados (padrão: competitiondb)"
    echo "  -u, --admin-username   Nome do usuário administrador (obrigatório)"
    echo "  -p, --admin-password   Senha do administrador (obrigatório)"
    echo "  -a, --app-service-plan Nome do App Service Plan (opcional)"
    echo "  -w, --web-app-name     Nome do Web App (opcional)"
    echo "  -h, --help             Exibir esta ajuda"
    echo ""
    echo "Exemplo:"
    echo "  $0 -g myResourceGroup -s mypostgresql -u myadmin -p MyPassword123!"
}

# Processar argumentos da linha de comando
while [[ $# -gt 0 ]]; do
    case $1 in
        -g|--resource-group)
            RESOURCE_GROUP_NAME="$2"
            shift 2
            ;;
        -l|--location)
            LOCATION="$2"
            shift 2
            ;;
        -s|--server-name)
            SERVER_NAME="$2"
            shift 2
            ;;
        -d|--database-name)
            DATABASE_NAME="$2"
            shift 2
            ;;
        -u|--admin-username)
            ADMIN_USERNAME="$2"
            shift 2
            ;;
        -p|--admin-password)
            ADMIN_PASSWORD="$2"
            shift 2
            ;;
        -a|--app-service-plan)
            APP_SERVICE_PLAN_NAME="$2"
            shift 2
            ;;
        -w|--web-app-name)
            WEB_APP_NAME="$2"
            shift 2
            ;;
        -h|--help)
            show_help
            exit 0
            ;;
        *)
            echo "Opção desconhecida: $1"
            show_help
            exit 1
            ;;
    esac
done

# Validar parâmetros obrigatórios
if [[ -z "$RESOURCE_GROUP_NAME" || -z "$SERVER_NAME" || -z "$ADMIN_USERNAME" || -z "$ADMIN_PASSWORD" ]]; then
    echo "Erro: Parâmetros obrigatórios não fornecidos."
    show_help
    exit 1
fi

echo "=== CONFIGURAÇÃO DO AZURE DATABASE FOR POSTGRESQL ==="
echo "Resource Group: $RESOURCE_GROUP_NAME"
echo "Location: $LOCATION"
echo "Server Name: $SERVER_NAME"
echo "Database Name: $DATABASE_NAME"
echo "Admin Username: $ADMIN_USERNAME"
echo ""

# Verificar se está logado no Azure
echo "1. Verificando login no Azure..."
if ! az account show &> /dev/null; then
    echo "Erro: Não está logado no Azure. Execute 'az login' primeiro."
    exit 1
fi

echo "✓ Logado no Azure com sucesso!"

# 2. Criar Resource Group
echo ""
echo "2. Criando Resource Group..."
if az group show --name "$RESOURCE_GROUP_NAME" &> /dev/null; then
    echo "✓ Resource Group '$RESOURCE_GROUP_NAME' já existe."
else
    az group create --name "$RESOURCE_GROUP_NAME" --location "$LOCATION"
    if [[ $? -eq 0 ]]; then
        echo "✓ Resource Group '$RESOURCE_GROUP_NAME' criado com sucesso!"
    else
        echo "✗ Erro ao criar Resource Group."
        exit 1
    fi
fi

# 3. Criar servidor PostgreSQL
# echo ""
# echo "3. Criando servidor PostgreSQL..."
# if az postgres server show --resource-group "$RESOURCE_GROUP_NAME" --name "$SERVER_NAME" &> /dev/null; then
#     echo "✓ Servidor PostgreSQL '$SERVER_NAME' já existe."
# else
#     az postgres server create \
#         --resource-group "$RESOURCE_GROUP_NAME" \
#         --name "$SERVER_NAME" \
#         --location "$LOCATION" \
#         --admin-user "$ADMIN_USERNAME" \
#         --admin-password "$ADMIN_PASSWORD" \
#         --sku-name GP_Gen5_2 \
#         --storage-size 5120 \
#         --version 13
    
#     if [[ $? -eq 0 ]]; then
#         echo "✓ Servidor PostgreSQL '$SERVER_NAME' criado com sucesso!"
#     else
#         echo "✗ Erro ao criar servidor PostgreSQL."
#         exit 1
#     fi
# fi

# 4. Configurar regras de firewall
echo ""
echo "4. Configurando regras de firewall..."

# Permitir acesso do Azure
# az postgres server firewall-rule create --resource-group "$RESOURCE_GROUP_NAME" --server "$SERVER_NAME" --name "AllowAzureServices" --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0 &> /dev/null

# Permitir acesso local
# MY_IP=$(curl -s https://api.ipify.org)
# az postgres server firewall-rule create --resource-group "$RESOURCE_GROUP_NAME" --server "$SERVER_NAME" --name "AllowMyIP" --start-ip-address "$MY_IP" --end-ip-address "$MY_IP" &> /dev/null

echo "✓ Regras de firewall configuradas!"
echo "  - Serviços do Azure: permitido"
echo "  - Seu IP atual ($MY_IP): permitido"

# 5. Criar banco de dados
# echo ""
# echo "5. Criando banco de dados..."
# if az postgres db show --resource-group "$RESOURCE_GROUP_NAME" --server-name "$SERVER_NAME" --name "$DATABASE_NAME" &> /dev/null; then
#     echo "✓ Banco de dados '$DATABASE_NAME' já existe."
# else
#     az postgres db create \
#         --resource-group "$RESOURCE_GROUP_NAME" \
#         --server-name "$SERVER_NAME" \
#         --name "$DATABASE_NAME"
#     if [[ $? -eq 0 ]]; then
#         echo "✓ Banco de dados '$DATABASE_NAME' criado com sucesso!"
#     else
#         echo "✗ Erro ao criar banco de dados."
#         exit 1
#     fi
# fi

# 6. Gerar connection string
echo ""
echo "6. Gerando connection string..."
CONNECTION_STRING="Host=${SERVER_NAME}.postgres.database.azure.com;Database=${DATABASE_NAME};Username=${ADMIN_USERNAME}@${SERVER_NAME};Password=${ADMIN_PASSWORD};SSL Mode=Require;"

# 7. Criar App Service (se especificado)
# if [[ -n "$APP_SERVICE_PLAN_NAME" && -n "$WEB_APP_NAME" ]]; then
#     echo ""
#     echo "7. Criando App Service..."
    
    # Criar App Service Plan
#     if az appservice plan show --resource-group "$RESOURCE_GROUP_NAME" --name "$APP_SERVICE_PLAN_NAME" &> /dev/null; then
#         echo "✓ App Service Plan '$APP_SERVICE_PLAN_NAME' já existe."
#     else
#         az appservice plan create \
#             --resource-group "$RESOURCE_GROUP_NAME" \
#             --name "$APP_SERVICE_PLAN_NAME" \
#             --location "$LOCATION" \
#             --sku B1 \
#             --is-linux
        
#         echo "✓ App Service Plan '$APP_SERVICE_PLAN_NAME' criado!"
#     fi
    
    # Criar Web App
#     if az webapp show --resource-group "$RESOURCE_GROUP_NAME" --name "$WEB_APP_NAME" &> /dev/null; then
#         echo "✓ Web App '$WEB_APP_NAME' já existe."
#     else
#         az webapp create \
#             --resource-group "$RESOURCE_GROUP_NAME" \
#             --plan "$APP_SERVICE_PLAN_NAME" \
#             --name "$WEB_APP_NAME" \
#             --runtime "DOTNETCORE:8.0"
        
#         echo "✓ Web App '$WEB_APP_NAME' criado!"
#     fi
    
    # Configurar connection string
    az webapp config appsettings set --resource-group "$RESOURCE_GROUP_NAME" --name "$WEB_APP_NAME" --settings DATABASE_CONNECTION_STRING="$CONNECTION_STRING"
    
    echo "✓ Connection string configurada no App Service!"
# fi

# 8. Exibir informações finais
echo ""
echo "=== INFORMAÇÕES DE CONEXÃO ==="
echo "Server: ${SERVER_NAME}.postgres.database.azure.com"
echo "Database: $DATABASE_NAME"
echo "Username: ${ADMIN_USERNAME}@${SERVER_NAME}"
echo "Connection String:"
echo "$CONNECTION_STRING"
echo ""

echo "=== PRÓXIMOS PASSOS ==="
echo "1. Configure a variável de ambiente DATABASE_CONNECTION_STRING:"
echo "   export DATABASE_CONNECTION_STRING=\"$CONNECTION_STRING\""
echo ""
echo "2. Execute as migrações do Entity Framework:"
echo "   dotnet ef database update"
echo ""
echo "3. Para GitHub Actions, adicione os seguintes secrets:"
echo "   - DATABASE_CONNECTION_STRING"
if [[ -n "$WEB_APP_NAME" ]]; then
    echo "   - AZURE_WEBAPP_PUBLISH_PROFILE"
fi
echo ""

echo "=== CONFIGURAÇÃO CONCLUÍDA ==="

