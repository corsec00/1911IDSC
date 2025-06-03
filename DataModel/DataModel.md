# Modelo de Dados para Azure Table Storage

## Visão Geral

O Azure Table Storage é um serviço NoSQL que armazena dados estruturados não relacionais na nuvem. Para nossa aplicação de competição, precisamos projetar um modelo de dados que permita:

1. Armazenar histórico de competições
2. Armazenar histórico de participantes e seus resultados
3. Consultar histórico por participante
4. Consultar histórico por competição
5. Manter relacionamentos entre competições e participantes

## Estrutura do Azure Table Storage

O Azure Table Storage organiza dados em:
- **Tabelas**: Coleções de entidades
- **Entidades**: Conjuntos de propriedades (similar a linhas em bancos de dados relacionais)
- **Propriedades**: Pares nome-valor (similar a colunas)

Cada entidade deve ter:
- **PartitionKey**: Determina como os dados são distribuídos
- **RowKey**: Identificador único dentro de uma partição
- Juntos, PartitionKey e RowKey formam a chave primária composta

## Tabelas Propostas

### 1. CompetitionTable

Armazena informações sobre cada competição.

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| PartitionKey | String | "Competition" (fixo) |
| RowKey | String | ID único da competição (GUID) |
| Name | String | Nome da competição |
| Date | DateTime | Data da competição |
| Description | String | Descrição da competição |
| CreatedAt | DateTime | Data de criação do registro |

### 2. ParticipantTable

Armazena informações sobre cada participante.

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| PartitionKey | String | "Participant" (fixo) |
| RowKey | String | ID único do participante (GUID) |
| Name | String | Nome do participante |
| Email | String | Email do participante (opcional) |
| CreatedAt | DateTime | Data de criação do registro |

### 3. ResultTable

Armazena os resultados de cada participante em cada rodada de uma competição.

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| PartitionKey | String | ID da competição (para consultas por competição) |
| RowKey | String | ID do participante + "_" + Número da rodada |
| ParticipantId | String | ID do participante (referência) |
| ParticipantName | String | Nome do participante (desnormalizado para facilitar consultas) |
| CompetitionId | String | ID da competição (referência) |
| CompetitionName | String | Nome da competição (desnormalizado para facilitar consultas) |
| RoundNumber | Int | Número da rodada (1 ou 2) |
| TimeInSeconds | Decimal | Tempo base em segundos |
| BravoCount | Int | Contagem de penalidades Bravo |
| CharlieCount | Int | Contagem de penalidades Charlie |
| MissCount | Int | Contagem de penalidades Miss |
| FaltaCount | Int | Contagem de penalidades Falta |
| VitimaCount | Int | Contagem de penalidades Vítima |
| PlateCount | Int | Contagem de penalidades Plate |
| TotalTime | Decimal | Tempo total calculado |
| IsEliminated | Boolean | Indica se o participante foi eliminado |
| CreatedAt | DateTime | Data de criação do registro |
| UpdatedAt | DateTime | Data da última atualização |

### 4. FinalResultTable

Armazena os resultados finais de cada participante em uma competição.

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| PartitionKey | String | ID da competição (para consultas por competição) |
| RowKey | String | ID do participante |
| ParticipantId | String | ID do participante (referência) |
| ParticipantName | String | Nome do participante (desnormalizado para facilitar consultas) |
| CompetitionId | String | ID da competição (referência) |
| CompetitionName | String | Nome da competição (desnormalizado para facilitar consultas) |
| Round1Time | Decimal | Tempo total da primeira rodada |
| Round2Time | Decimal | Tempo total da segunda rodada |
| BestTime | Decimal | Melhor tempo entre as duas rodadas |
| BestRound | Int | Rodada com o melhor tempo (1 ou 2) |
| Position | Int | Posição final na competição |
| CreatedAt | DateTime | Data de criação do registro |
| UpdatedAt | DateTime | Data da última atualização |

## Estratégia de Particionamento

1. **CompetitionTable e ParticipantTable**: Usam chaves de partição fixas para facilitar a recuperação de todas as competições ou participantes.

2. **ResultTable e FinalResultTable**: Usam o ID da competição como chave de partição para otimizar consultas por competição.

## Consultas Comuns

1. **Obter todas as competições**:
   - Filtrar por PartitionKey = "Competition"

2. **Obter todos os participantes**:
   - Filtrar por PartitionKey = "Participant"

3. **Obter resultados de uma competição específica**:
   - Filtrar ResultTable por PartitionKey = [ID da competição]

4. **Obter resultados de um participante específico em todas as competições**:
   - Consulta mais complexa que requer uma varredura de tabela com filtro secundário em ParticipantId
   - Alternativa: criar uma tabela adicional com PartitionKey = ID do participante

5. **Obter resultados finais de uma competição**:
   - Filtrar FinalResultTable por PartitionKey = [ID da competição]

## Considerações

1. **Desnormalização**: Algumas propriedades são desnormalizadas (como nomes) para facilitar consultas e exibição.

2. **Consultas por participante**: Para otimizar consultas por participante, podemos considerar uma tabela adicional ou índices secundários.

3. **Histórico de alterações**: Para rastrear alterações nos resultados, incluímos campos de timestamp (CreatedAt, UpdatedAt).

4. **Escalabilidade**: O modelo suporta um grande número de competições e participantes sem problemas de desempenho.
