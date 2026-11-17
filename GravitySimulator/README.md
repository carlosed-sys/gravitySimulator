# GravitySimulator — N2

1. No MySQL 8.4, abra e execute `Banco/criar_banco.sql` como administrador. O script cria o banco e ativa o agendador de eventos. Também pode ser executado no terminal com `mysql -u root -p < Banco/criar_banco.sql`.
2. Em `GravitySimulator/BancoSimulacoes.cs`, ajuste a constante `Conexao` com os dados do MySQL local. O exemplo usa `localhost:3306` e `root` sem senha, para um ambiente local de teste.
3. No Windows com .NET 8, abra o projeto no Visual Studio ou execute `dotnet run --project GravitySimulator/GravitySimulator.csproj` a partir desta pasta.
4. Escolha **1** para simular de 1 a 400 corpos ou **2** para listar as simulações salvas e selecionar um ID para replay. Na janela, use espaço para pausar, setas para mudar a velocidade e a roda do mouse para ampliar ou reduzir. Fechar a janela de uma simulação salva os quadros processados até então. O replay sempre mostra os quadros que foram efetivamente gravados.

- [Modelos e estudo de volumetria](Banco/modelos-e-volumetria.md)
- [Relatório de desempenho N1 × N2](relatorio-desempenho.md)
