# Comparação de desempenho — GravitySimulator N1 e N2

A versão N1 calcula as forças em sequência e salva apenas o estado inicial e o final em TXT. A N2 distribui o cálculo das acelerações usando `Parallel.For`, mantém movimento e fusões na ordem original e grava o estado inicial e todas as iterações no MySQL.

## Método

- N1 a partir do commit `bab1eeca`. N2 a partir dos fontes desta entrega. Ambas foram compiladas em Release com o SDK .NET 8.0.419. O Linux de teste disponibilizou dois processadores lógicos ao .NET.
- Mesmos corpos iniciais em cada par de execuções, numa grade de 20 colunas com espaçamento de 10 milhões de metros. Massas entre `1e20` e `1.1e20` kg, densidades de 1.000 a 8.000 kg/m³ e velocidades de −50 a 50 m/s, com semente 42. Passo de 3 segundos.
- Medição com `Stopwatch`, uma rodada inicial de aquecimento e ordem alternada. Interface gráfica e impressão no console ficaram fora das duas medições.
- Somente física: sete execuções de 500 iterações por versão e por quantidade de corpos. A medição cobre `Passo()` e inclui movimento e fusões. Os estados finais da N1 e da N2 foram comparados campo a campo em todas as execuções.
- Física e gravação: cinco execuções de 100 iterações e 400 corpos por versão. A N1 escreveu dois TXT locais. A N2 abriu uma conexão MySQL 8.4.13 local, gravou 101 quadros e confirmou a transação. Isso corresponde a 800 linhas de corpos na N1 e 40.400 na N2 no cenário medido.

## Resultados

| Cenário | Mediana N1 | Mediana N2 |
|---|---:|---:|
| Física, 50 corpos e 500 iterações | 98,55 ms | 110,44 ms |
| Física, 400 corpos e 500 iterações | 3.122,80 ms | 3.118,38 ms |
| Física e gravação, 400 corpos e 100 iterações | 490,84 ms | 1.614,62 ms |

Com 50 corpos, a etapa física paralela ficou cerca de 12% mais lenta neste ambiente. Com 400 corpos, a diferença foi de 0,14% a favor da N2, pequena diante da variação entre rodadas. A medição com gravação levou 3,29 vezes o tempo da N1 e incluiu cerca de 50 vezes mais estados de corpos. O resultado mede o custo conjunto de calcular e persistir o histórico completo. Ele não isola a velocidade do MySQL em comparação com TXT.

As conclusões valem para o cenário medido. A quantidade de núcleos, a frequência de fusões e a latência do MySQL podem alterar os tempos.
