# Banco de Dados II — Simulador Gravitacional

## Modelo relacional

- **simulacao** (**id**, criada_em, encerrada_em, corpos_iniciais, iteracoes_previstas, ultima_iteracao, segundos_por_iteracao)
- **corpo_iteracao** (**simulacao_id**, **iteracao**, **ordem**, nome, massa, densidade, pos_x, pos_y, vel_x, vel_y, volume, raio)

Os campos em negrito compõem as chaves primárias. A chave estrangeira `corpo_iteracao.simulacao_id` referencia `simulacao.id`. Uma simulação gravada possui muitos estados de corpos, e cada estado pertence a uma simulação. O quadro 0 é o estado inicial. A coluna `ordem` preserva a ordem dos corpos dentro de cada quadro. Depois de uma fusão, os próximos quadros contêm somente os corpos resultantes.

## Modelo lógico

| Entidade | Atributos e domínios |
|---|---|
| Simulação | ID inteiro, datas de início e fim, contagem inicial de corpos (1 a 400), contagens de iterações prevista e gravada e intervalo positivo em segundos. |
| Estado do corpo | ID da simulação, iteração e ordem inteiras, nome textual, massa em kg, densidade em kg/m³, posição em m, velocidade em m/s, volume em m³ e raio em m. Os valores físicos usam ponto flutuante de precisão dupla. |

A cardinalidade é `simulacao 1 → N corpo_iteracao`. O simulador confirma a transação após gravar o último quadro e atualizar a simulação. O replay reconstrói o volume e o raio a partir dos mesmos valores de massa e densidade gravados.

## Modelo físico

O [script de criação](criar_banco.sql) implementa o banco `gravity_simulator` em MySQL 8.4 com InnoDB e UTF-8 (`utf8mb4`). A chave primária composta da tabela de estados atende à leitura por simulação, quadro e ordem. O índice `idx_simulacao_encerrada` atende à listagem recente e à busca dos registros vencidos. A chave estrangeira remove os estados junto com a simulação. O evento diário chama a procedure `excluir_simulacoes_antigas`, que remove as simulações encerradas há mais de um mês em lotes de até cinco.

## Volumetria

O enunciado prevê 50 simulações por dia e até 400 corpos por simulação. Como o número de iterações é escolhido ao executar, o estudo usa dois cenários, nenhum deles como limite da aplicação. O cálculo considera o quadro inicial e supõe que os 400 corpos sobrevivem até o fim.

| Iterações por simulação | Estados por simulação | Estados por dia | Estados em 30 dias | Estimativa dos estados em 30 dias |
|---:|---:|---:|---:|---:|
| 100 | 40.400 | 2.020.000 | 60.600.000 | 9,70 GB |
| 1.000 | 400.400 | 20.020.000 | 600.600.000 | 96,10 GB |

Fórmulas: `400 × (iterações + 1)` por simulação e `50 × 30 = 1.500` simulações no período. A estimativa usa 160 bytes por estado para planejamento, incluindo os 64 bytes dos oito campos `DOUBLE`, as chaves, o nome e espaço estimado para overhead do InnoDB. Os valores são decimais e dependem do preenchimento das páginas, do tamanho dos nomes e da frequência das fusões. O calendário e o horário do evento diário afetam a janela real de retenção. Logs, espaço temporário e backups demandam capacidade adicional.
