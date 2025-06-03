# Guia de Integração com Azure Table Storage e Key Vault

Este documento foi copiado da documentação original para o diretório da aplicação para facilitar a referência.

## Configuração do Azure

1. **Criar uma conta de armazenamento do Azure**:
   - Acesse o [Portal do Azure](https://portal.azure.com)
   - Clique em "Criar um recurso" > "Armazenamento" > "Conta de armazenamento"
   - Preencha os detalhes necessários:
     - Assinatura: Sua assinatura do Azure
     - Grupo de recursos: Crie um novo ou use um existente
     - Nome da conta de armazenamento: Um nome único globalmente
     - Localização: Escolha a região mais próxima
     - Desempenho: Standard
     - Redundância: Localmente redundante (LRS)
   - Clique em "Revisar + criar" e depois em "Criar"

2. **Criar um Azure Key Vault**:
   - No Portal do Azure, clique em "Criar um recurso" > "Segurança" > "Key Vault"
   - Preencha os detalhes necessários:
     - Assinatura: Sua assinatura do Azure
     - Grupo de recursos: O mesmo usado para a conta de armazenamento
     - Nome do Key Vault: Um nome único globalmente
     - Região: A mesma região da conta de armazenamento
     - Plano de preços: Standard
   - Clique em "Revisar + criar" e depois em "Criar"

3. **Configurar a string de conexão no Key Vault**:
   - Acesse a conta de armazenamento criada
   - No menu lateral, vá para "Chaves de acesso"
   - Copie a string de conexão (Connection string)
   - Acesse o Key Vault criado
   - No menu lateral, vá para "Segredos"
   - Clique em "Gerar/Importar"
   - Nome do segredo: `AzureTableStorageConnectionString`
   - Valor do segredo: Cole a string de conexão copiada
   - Clique em "Criar"

4. **Configurar a identidade gerenciada para o App Service**:
   - Acesse o App Service onde a aplicação está hospedada
   - No menu lateral, vá para "Identidade"
   - Na guia "Atribuído pelo sistema", altere o status para "Ativado"
   - Clique em "Salvar"
   - Copie o "ID do objeto" gerado

5. **Conceder permissões ao App Service para acessar o Key Vault**:
   - Acesse o Key Vault criado
   - No menu lateral, vá para "Políticas de acesso"
   - Clique em "Adicionar política de acesso"
   - Em "Permissões de segredo", selecione "Obter" e "Listar"
   - Em "Selecionar principal", cole o ID do objeto copiado
   - Clique em "Adicionar" e depois em "Salvar"

## Configuração da Aplicação

1. **Atualizar o arquivo appsettings.json**:
   - Abra o arquivo `appsettings.json` na raiz do projeto
   - Atualize o valor de `VaultUri` com o URI do seu Key Vault:
   ```json
   "KeyVault": {
     "VaultUri": "https://seu-key-vault-name.vault.azure.net/"
   }
   ```
   - Substitua `seu-key-vault-name` pelo nome do seu Key Vault

2. **Configurar variáveis de ambiente no App Service**:
   - Acesse o App Service onde a aplicação está hospedada
   - No menu lateral, vá para "Configuração"
   - Na guia "Configurações do aplicativo", clique em "Nova configuração do aplicativo"
   - Nome: `ASPNETCORE_ENVIRONMENT`
   - Valor: `Production`
   - Clique em "OK" e depois em "Salvar"

## Verificação da Configuração

Para verificar se a configuração foi realizada corretamente:

1. Acesse a aplicação após a implantação
2. Navegue até a página "Histórico de Competições"
3. Você deve ver uma lista de competições (vazia se for a primeira execução)
4. Tente criar uma nova competição e verificar se ela aparece no histórico

Se ocorrer algum erro, verifique os logs do App Service para diagnóstico.
