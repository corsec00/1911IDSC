# Integração de Banco de Dados para CompetitionApp

Este projeto implementa a integração de um banco de dados persistente (Azure Table Storage) para a aplicação CompetitionApp, permitindo o armazenamento e consulta de histórico de competições e participantes.

## Visão Geral

A solução implementa:

1. **Armazenamento persistente** usando Azure Table Storage (escolhido por seu baixo custo)
2. **Gerenciamento seguro de credenciais** usando Azure Key Vault
3. **Histórico de competições e participantes** com consultas eficientes
4. **Guia passo a passo** para configuração dos recursos no Azure

## Estrutura do Projeto

Este diretório contém a documentação e código necessários para implementar a integração com banco de dados:

- `DataModel.md` - Modelo de dados para Azure Table Storage
- `AzureStorageIntegration.md` - Código de integração com Azure Table Storage e Key Vault
- `ApplicationLogicUpdate.md` - Atualização da lógica da aplicação para usar armazenamento persistente
- `AzureConfigurationGuide.md` - Guia passo a passo para configuração dos recursos no Azure

## Por que Azure Table Storage?

Após análise comparativa entre Azure Table Storage e Cosmos DB, o Azure Table Storage foi escolhido por:

1. **Custo significativamente menor** - Ideal para aplicações com orçamento limitado
2. **Simplicidade** - API fácil de usar para operações CRUD
3. **Escalabilidade adequada** - Suficiente para o volume de dados esperado
4. **Baixo custo de transações** - Econômico mesmo com uso frequente

## Modelo de Dados

O modelo de dados foi projetado para otimizar consultas por competição e participante:

1. **CompetitionTable** - Armazena informações sobre competições
2. **ParticipantTable** - Armazena informações sobre participantes
3. **ResultTable** - Armazena resultados de cada rodada
4. **FinalResultTable** - Armazena resultados finais de cada competição

## Segurança

As credenciais de acesso ao Azure Table Storage são armazenadas de forma segura no Azure Key Vault, seguindo as melhores práticas de segurança:

1. **Sem credenciais hardcoded** no código-fonte
2. **Acesso baseado em identidade** usando identidades gerenciadas do Azure
3. **Princípio de privilégio mínimo** para acesso aos recursos

## Implementação

A implementação mantém compatibilidade com o fluxo atual da aplicação, adicionando:

1. **Persistência transparente** - Dados são salvos automaticamente no Azure Table Storage
2. **Novas páginas de histórico** - Para visualizar competições e resultados anteriores
3. **Gerenciamento de competições** - Possibilidade de alternar entre competições

## Estimativa de Custos

Para uma aplicação de pequeno a médio porte:

- **Azure Table Storage**: $1-5/mês
- **Azure Key Vault**: $0-1/mês
- **Total (excluindo App Service)**: $1-6/mês

## Próximos Passos

Para implementar esta solução:

1. Siga o guia de configuração do Azure em `AzureConfigurationGuide.md`
2. Adicione os novos arquivos e atualize os existentes conforme documentado
3. Implante a aplicação atualizada no Azure App Service
