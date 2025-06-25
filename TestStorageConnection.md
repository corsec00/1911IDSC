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

## Melhorias implementadas

1. **Salvamento automático**: Dados são salvos automaticamente durante as rodadas
2. **Tratamento de erros**: Aplicação continua funcionando mesmo se Azure Storage falhar
3. **Logs detalhados**: Console mostra o que está acontecendo
4. **Fallback para emulador**: Se não configurado, usa emulador local
5. **Criação automática de participantes**: Participantes são criados automaticamente quando necessário

