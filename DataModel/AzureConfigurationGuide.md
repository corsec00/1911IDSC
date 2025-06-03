# Guia de Configuração do Azure para Armazenamento de Dados

Este guia fornece instruções passo a passo para configurar os recursos do Azure necessários para a aplicação de competição: Azure Storage Account (Table Storage) e Azure Key Vault.

## Pré-requisitos

1. Uma conta do Azure ativa
2. Azure CLI instalado ou acesso ao Azure Portal
3. Permissões para criar recursos no Azure

## 1. Criar um Grupo de Recursos

Primeiro, crie um grupo de recursos para organizar todos os recursos relacionados à aplicação.

### Usando o Azure Portal

1. Acesse o [Portal do Azure](https://portal.azure.com)
2. Clique em "Criar um recurso" > "Grupo de recursos"
3. Preencha os seguintes campos:
   - **Assinatura**: Selecione sua assinatura
   - **Grupo de recursos**: `competition-app-resources`
   - **Região**: Selecione a região mais próxima de você (ex: Brazil South)
4. Clique em "Revisar + criar" e depois em "Criar"

### Usando o Azure CLI

```bash
# Fazer login no Azure
az login

# Criar grupo de recursos
az group create --name competition-app-resources --location brazilsouth
```

## 2. Criar uma Conta de Armazenamento do Azure

Em seguida, crie uma conta de armazenamento que hospedará as tabelas do Azure Table Storage.

### Usando o Azure Portal

1. No Portal do Azure, clique em "Criar um recurso" > "Armazenamento" > "Conta de armazenamento"
2. Preencha os seguintes campos:
   - **Assinatura**: Selecione sua assinatura
   - **Grupo de recursos**: `competition-app-resources`
   - **Nome da conta de armazenamento**: `competitionappstorageXXXXX` (substitua XXXXX por números aleatórios para garantir um nome único)
   - **Região**: A mesma região do grupo de recursos
   - **Desempenho**: Standard
   - **Redundância**: Armazenamento com redundância local (LRS) - opção mais econômica
3. Clique em "Revisar + criar" e depois em "Criar"
4. Aguarde a implantação ser concluída

### Usando o Azure CLI

```bash
# Criar conta de armazenamento (substitua XXXXX por números aleatórios)
az storage account create \
  --name competitionappstorageXXXXX \
  --resource-group competition-app-resources \
  --location brazilsouth \
  --sku Standard_LRS \
  --kind StorageV2
```

## 3. Obter a String de Conexão da Conta de Armazenamento

Você precisará da string de conexão para configurar o acesso ao Azure Table Storage.

### Usando o Azure Portal

1. No Portal do Azure, navegue até a conta de armazenamento que você criou
2. No menu lateral esquerdo, clique em "Chaves de acesso"
3. Clique em "Mostrar chaves" e copie a "String de conexão" da key1 ou key2

### Usando o Azure CLI

```bash
# Obter string de conexão (substitua XXXXX pelo nome da sua conta de armazenamento)
az storage account show-connection-string \
  --name competitionappstorageXXXXX \
  --resource-group competition-app-resources
```

## 4. Criar um Azure Key Vault

Agora, crie um Azure Key Vault para armazenar com segurança a string de conexão da conta de armazenamento.

### Usando o Azure Portal

1. No Portal do Azure, clique em "Criar um recurso" > "Segurança" > "Key Vault"
2. Preencha os seguintes campos:
   - **Assinatura**: Selecione sua assinatura
   - **Grupo de recursos**: `competition-app-resources`
   - **Nome do Key Vault**: `competition-app-vault-XXXXX` (substitua XXXXX por números aleatórios para garantir um nome único)
   - **Região**: A mesma região do grupo de recursos
   - **Plano de preços**: Standard
3. Clique em "Revisar + criar" e depois em "Criar"
4. Aguarde a implantação ser concluída

### Usando o Azure CLI

```bash
# Criar Key Vault (substitua XXXXX por números aleatórios)
az keyvault create \
  --name competition-app-vault-XXXXX \
  --resource-group competition-app-resources \
  --location brazilsouth
```

## 5. Armazenar a String de Conexão no Key Vault

Armazene a string de conexão da conta de armazenamento como um segredo no Key Vault.

### Usando o Azure Portal

1. No Portal do Azure, navegue até o Key Vault que você criou
2. No menu lateral esquerdo, clique em "Segredos"
3. Clique em "Gerar/Importar"
4. Preencha os seguintes campos:
   - **Opções de upload**: Manual
   - **Nome**: `AzureTableStorageConnectionString`
   - **Valor**: Cole a string de conexão da conta de armazenamento que você copiou anteriormente
5. Clique em "Criar"

### Usando o Azure CLI

```bash
# Armazenar string de conexão no Key Vault (substitua os valores conforme necessário)
az keyvault secret set \
  --vault-name competition-app-vault-XXXXX \
  --name AzureTableStorageConnectionString \
  --value "DefaultEndpointsProtocol=https;AccountName=competitionappstorageXXXXX;AccountKey=your-account-key;EndpointSuffix=core.windows.net"
```

## 6. Configurar Políticas de Acesso ao Key Vault

Configure as políticas de acesso para permitir que sua aplicação acesse os segredos no Key Vault.

### Usando o Azure Portal

1. No Portal do Azure, navegue até o Key Vault que você criou
2. No menu lateral esquerdo, clique em "Políticas de acesso"
3. Clique em "Adicionar política de acesso"
4. Em "Permissões de segredo", selecione "Obter" e "Listar"
5. Em "Selecionar principal", pesquise e selecione a identidade gerenciada da sua aplicação web (se já estiver configurada) ou sua conta de usuário para testes
6. Clique em "Adicionar"

### Usando o Azure CLI

```bash
# Configurar política de acesso (substitua os valores conforme necessário)
az keyvault set-policy \
  --name competition-app-vault-XXXXX \
  --resource-group competition-app-resources \
  --object-id <object-id-da-identidade-gerenciada-ou-usuario> \
  --secret-permissions get list
```

## 7. Criar Tabelas no Azure Table Storage

Crie as tabelas necessárias para a aplicação no Azure Table Storage.

### Usando o Azure Portal

1. No Portal do Azure, navegue até a conta de armazenamento que você criou
2. No menu lateral esquerdo, clique em "Serviço de tabela" > "Tabelas"
3. Clique em "Tabela" para criar uma nova tabela
4. Crie as seguintes tabelas:
   - `Competitions`
   - `Participants`
   - `Results`
   - `FinalResults`

### Usando o Azure CLI

```bash
# Criar tabelas (substitua XXXXX pelo nome da sua conta de armazenamento)
az storage table create --name Competitions --account-name competitionappstorageXXXXX
az storage table create --name Participants --account-name competitionappstorageXXXXX
az storage table create --name Results --account-name competitionappstorageXXXXX
az storage table create --name FinalResults --account-name competitionappstorageXXXXX
```

## 8. Configurar a Aplicação Web

Atualize o arquivo `appsettings.json` da sua aplicação com as configurações necessárias.

```json
{
  "KeyVault": {
    "VaultUri": "https://competition-app-vault-XXXXX.vault.azure.net/"
  },
  "AzureTableStorage": {
    "ConnectionStringSecretName": "AzureTableStorageConnectionString"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

## 9. Configurar Identidade Gerenciada para a Aplicação Web

Para que sua aplicação web acesse o Key Vault de forma segura, configure uma identidade gerenciada.

### Usando o Azure Portal

1. No Portal do Azure, navegue até sua aplicação web (App Service)
2. No menu lateral esquerdo, clique em "Identidade"
3. Na guia "Atribuído pelo sistema", altere o status para "Ativado"
4. Clique em "Salvar"
5. Copie o "ID do objeto" gerado
6. Volte ao Key Vault e adicione uma política de acesso para esta identidade, conforme descrito na etapa 6

### Usando o Azure CLI

```bash
# Habilitar identidade gerenciada para a aplicação web
az webapp identity assign --name YourWebAppName --resource-group competition-app-resources

# Obter o ID do objeto da identidade gerenciada
objectId=$(az webapp identity show --name YourWebAppName --resource-group competition-app-resources --query principalId --output tsv)

# Configurar política de acesso no Key Vault
az keyvault set-policy \
  --name competition-app-vault-XXXXX \
  --resource-group competition-app-resources \
  --object-id $objectId \
  --secret-permissions get list
```

## 10. Implantar a Aplicação no Azure App Service

Finalmente, implante sua aplicação no Azure App Service.

### Usando o Azure Portal

1. No Portal do Azure, clique em "Criar um recurso" > "Web" > "Aplicativo Web"
2. Preencha os seguintes campos:
   - **Assinatura**: Selecione sua assinatura
   - **Grupo de recursos**: `competition-app-resources`
   - **Nome**: `competition-app-XXXXX` (substitua XXXXX por números aleatórios para garantir um nome único)
   - **Publicar**: Código
   - **Pilha de runtime**: .NET 6 (LTS)
   - **Sistema operacional**: Windows
   - **Região**: A mesma região do grupo de recursos
   - **Plano do Serviço de Aplicativo**: Crie um novo plano ou selecione um existente
3. Clique em "Revisar + criar" e depois em "Criar"
4. Aguarde a implantação ser concluída
5. Configure a implantação contínua a partir do seu repositório Git ou implante manualmente usando o Visual Studio ou o Azure CLI

### Usando o Azure CLI

```bash
# Criar plano do App Service
az appservice plan create \
  --name competition-app-plan \
  --resource-group competition-app-resources \
  --sku B1 \
  --is-linux false

# Criar aplicativo web
az webapp create \
  --name competition-app-XXXXX \
  --resource-group competition-app-resources \
  --plan competition-app-plan \
  --runtime "DOTNET|6.0"
```

## 11. Verificar a Implantação

Após a implantação, verifique se a aplicação está funcionando corretamente:

1. Acesse a URL da sua aplicação web: `https://competition-app-XXXXX.azurewebsites.net`
2. Verifique se a aplicação está se conectando corretamente ao Azure Table Storage
3. Teste a criação de competições, registro de participantes e resultados
4. Verifique se os dados estão sendo persistidos no Azure Table Storage

## 12. Monitoramento e Solução de Problemas

### Logs da Aplicação

1. No Portal do Azure, navegue até sua aplicação web
2. No menu lateral esquerdo, clique em "Logs" ou "Log stream" para visualizar os logs em tempo real
3. Verifique se há erros relacionados à conexão com o Azure Table Storage ou o Key Vault

### Verificar Dados nas Tabelas

1. No Portal do Azure, navegue até sua conta de armazenamento
2. No menu lateral esquerdo, clique em "Serviço de tabela" > "Tabelas"
3. Selecione uma tabela para visualizar seus dados
4. Use o "Storage Explorer" para uma experiência mais rica ao explorar os dados

## Estimativa de Custos

Para uma aplicação de pequeno a médio porte, os custos mensais estimados são:

1. **Azure Storage Account (Table Storage)**:
   - Armazenamento: ~$0.05 por GB/mês (primeiros 100 GB)
   - Transações: ~$0.00036 por 10.000 transações
   - Para uma aplicação com poucos dados e tráfego moderado: **$1-5 por mês**

2. **Azure Key Vault**:
   - Operações: $0.03 por 10.000 operações
   - Para uso típico: **$0-1 por mês**

3. **Azure App Service (Plano Básico B1)**:
   - **~$13-15 por mês**

**Total estimado**: **$14-21 por mês**

Esta é uma estimativa conservadora e os custos reais podem variar dependendo do uso. O Azure Table Storage é significativamente mais econômico que o Cosmos DB para este cenário, conforme solicitado.

## Conclusão

Você configurou com sucesso o Azure Table Storage e o Azure Key Vault para sua aplicação de competição. A aplicação agora armazena dados de forma persistente e segura na nuvem, permitindo o acesso ao histórico de competições e participantes.

Para reduzir custos ainda mais, considere:
- Usar um plano de App Service compartilhado (F1 gratuito ou D1 compartilhado) para ambientes de desenvolvimento
- Configurar regras de exclusão automática para dados antigos que não são mais necessários
- Monitorar o uso e ajustar os recursos conforme necessário
