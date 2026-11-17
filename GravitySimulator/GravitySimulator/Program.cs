using System;
using System.Windows.Forms;
using MySqlConnector;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);

        try
        {
            Console.WriteLine("1 - Novo universo aleatório");
            Console.WriteLine("2 - Replay de uma simulação");
            Console.Write("Opção: ");
            switch (Console.ReadLine())
            {
                case "1":
                    Simular();
                    break;
                case "2":
                    Reproduzir();
                    break;
            }
        }
        catch (MySqlException erro)
        {
            Console.WriteLine("Erro ao acessar o MySQL: " + erro.Message);
        }
    }

    private static void Simular()
    {
        Console.Write("Quantidade de corpos (1 a 400): ");
        if (!int.TryParse(Console.ReadLine(), out int corpos) || corpos < 1 || corpos > Universo.MaximoCorpos)
        {
            Console.WriteLine("Quantidade de corpos inválida.");
            return;
        }

        Console.Write("Quantidade de iterações: ");
        if (!int.TryParse(Console.ReadLine(), out int iteracoes) || iteracoes < 1 || iteracoes == int.MaxValue)
        {
            Console.WriteLine("Quantidade de iterações inválida.");
            return;
        }

        Console.Write($"Tempo entre iterações (até {Universo.MaximoSegundosPorIteracao} s): ");
        if (!double.TryParse(Console.ReadLine(), out double dt) || !double.IsFinite(dt) || dt <= 0 || dt > Universo.MaximoSegundosPorIteracao)
        {
            Console.WriteLine("Intervalo inválido.");
            return;
        }

        Console.Write("Ver em modo visual? (s/n): ");
        bool modoVisual = string.Equals(Console.ReadLine(), "s", StringComparison.OrdinalIgnoreCase);
        Universo universo = new Universo();
        universo.GerarAleatorio(corpos, iteracoes, dt);

        using BancoSimulacoes banco = new BancoSimulacoes();
        using MySqlTransaction transacao = banco.IniciarGravacao(universo, out long id);
        banco.GravarQuadro(transacao, id, 0, universo.Corpos);
        int ultimaIteracao = 0;

        void Avancar(int iteracao)
        {
            universo.Passo();
            banco.GravarQuadro(transacao, id, iteracao, universo.Corpos);
            ultimaIteracao = iteracao;
        }

        if (modoVisual)
        {
            using JanelaSimulador janela = new JanelaSimulador(universo, Avancar);
            Application.Run(janela);
        }
        else
        {
            universo.Exibir(0);
            for (int i = 1; i <= iteracoes; i++)
            {
                Avancar(i);
                universo.Exibir(i);
            }
        }

        // Se a janela fechar antes do fim, o replay vai até o último quadro gravado.
        banco.ConcluirGravacao(transacao, id, ultimaIteracao);
        Console.WriteLine("Simulação salva no MySQL com ID " + id);
    }

    private static void Reproduzir()
    {
        using BancoSimulacoes banco = new BancoSimulacoes();
        if (!banco.Listar())
        {
            Console.WriteLine("Nenhuma simulação gravada.");
            return;
        }

        Console.Write("Digite o ID da simulação: ");
        if (!long.TryParse(Console.ReadLine(), out long id) || id < 1)
        {
            Console.WriteLine("ID inválido.");
            return;
        }

        using MySqlTransaction leitura = banco.IniciarLeitura();
        Universo universo = banco.Carregar(leitura, id);
        if (universo == null)
        {
            Console.WriteLine("Simulação indisponível.");
            return;
        }

        // O replay lê um quadro por vez e nunca recalcula a física.
        using var quadros = banco.LerQuadros(leitura, id).GetEnumerator();
        if (!quadros.MoveNext())
            return;

        universo.Corpos = quadros.Current;
        using JanelaSimulador janela = new JanelaSimulador(universo, _ =>
        {
            if (quadros.MoveNext())
                universo.Corpos = quadros.Current;
        });
        Application.Run(janela);
    }
}
