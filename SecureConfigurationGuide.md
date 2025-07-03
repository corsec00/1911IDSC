# Guia de Configuração Segura

Este documento explica como configurar a aplicação CompetitionApp de forma segura, especialmente para armazenar credenciais sensíveis como a connection string do Azure Storage.

## Configuração da Connection String do Azure Storage

### Método Recomendado: Variáveis de Ambiente

A forma mais segura de configurar a connection string do Azure Storage é através de variáveis de ambiente. Isso evita que credenciais sensíveis sejam armazenadas no código-fonte ou em arquivos de configuração que possam ser acidentalmente compartilhados.

#### No Ambiente de Desenvolvimento

1. Configure a variável de ambiente `AZURE_STORAGE_CONNECTION_STRING` no seu sistema:

   **Windows (PowerShell):**
   ```powershell
   $env:AZURE_STORAGE_CONNECTION_STRING="DefaultEndpointsProtocol=https;AccountName=suaconta;AccountKey=suachave;EndpointSuffix=core.windows.net"
   ```

   **Windows (CMD):**
   ```cmd
   set AZURE_STORAGE_CONNECTION_STRING=DefaultEndpointsProtocol=https;AccountName=suaconta;AccountKey=suachave;EndpointSuffix=core.windows.net
   ```

   **Linux/macOS:**
   ```bash
   export AZURE_STORAGE_CONNECTION_STRING="DefaultEndpointsProtocol=https;AccountName=suaconta;AccountKey=suachave;EndpointSuffix=core.windows.net"
   ```

2. Para desenvolvimento local, você também pode usar o emulador de Azure Storage:
   ```
   UseDevelopmentStorage=true
   ```

#### No Azure App Service

1. No portal do Azure, navegue até o seu App Service
2. Vá para "Configuração" > "Configurações do aplicativo"
3. Adicione uma nova configuração:
   - Nome: `AZURE_STORAGE_CONNECTION_STRING`
   - Valor: Sua connection string completa
4. Clique em "Salvar"

### Método Alternativo: User Secrets (Apenas para Desenvolvimento)

Para desenvolvimento local, você também pode usar o User Secrets do ASP.NET Core:

1. Clique com o botão direito no projeto no Visual Studio
2. Selecione "Gerenciar Segredos do Usuário"
3. Adicione a connection string:
   ```json
   {
     "AzureTableStorage": {
       "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=suaconta;AccountKey=suachave;EndpointSuffix=core.windows.net"
     }
   }
   ```

## Segurança e Boas Práticas

1. **Nunca comite credenciais** diretamente no código-fonte ou em arquivos de configuração
2. **Regenere suas chaves periodicamente** no Azure Portal
3. **Use identidades gerenciadas** quando possível para autenticação no Azure
4. **Configure permissões mínimas necessárias** para sua conta de armazenamento
5. **Monitore o acesso** à sua conta de armazenamento usando logs de diagnóstico

## Configuração do appsettings.json

O arquivo `appsettings.json` já está configurado para usar variáveis de ambiente:

```json
{
  "AzureTableStorage": {
    "UseEnvironmentVariable": true,
    "EnvironmentVariableName": "AZURE_STORAGE_CONNECTION_STRING"
  }
}
```

Não modifique estas configurações para incluir credenciais diretamente no arquivo.

## Verificando a Configuração

Para verificar se a aplicação está usando a connection string correta:

1. Inicie a aplicação
2. Navegue até a página de histórico de competições
3. Se os dados forem carregados corretamente, a conexão está funcionando
4. Se ocorrer um erro, verifique se a variável de ambiente está configurada corretamente

# Teste de Conexão com Azure Storage

## Como testar se os dados estão sendo salvos

### 1. Verificar logs da aplicação
A aplicação agora inclui logs que mostram:
- Se está usando emulador local ou Azure Storage real
- Quando dados são salvos com sucesso
- Erros de conexão ou salvamento

### 2. Configurar a connection string
Para usar Azure Storage real, configure a variável de ambiente:
```bash
export AZURE_STORAGE_CONNECTION_STRING="DefaultEndpointsProtocol=https;AccountName=SEU_ACCOUNT;AccountKey=SUA_CHAVE;EndpointSuffix=core.windows.net"
```

### 3. Verificar se os dados aparecem no histórico
1. Complete uma competição (Rodadas 1 e 2)
2. Vá para Resultados Finais
3. Clique em "Salvar no Azure Storage"
4. Acesse "Histórico" > "Histórico de Competições"
5. Verifique se a competição aparece na lista

### 4. Verificar no Azure Portal
1. Acesse o Azure Portal
2. Vá para sua Storage Account
3. Navegue para "Storage browser" > "Tables"
4. Verifique se existem as tabelas:
   - CompetitionTable
   - ParticipantTable
   - ResultTable
   - FinalResultTable