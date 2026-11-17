using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using MySqlConnector;

internal sealed class BancoSimulacoes : IDisposable
{
    // Conta local de desenvolvimento. Ajuste usuário e senha para a sua instalação.
    private const string Conexao = "Server=localhost;Port=3306;Database=gravity_simulator;User ID=root;Password=;";
    private readonly MySqlConnection conexao = new MySqlConnection(Conexao);

    public BancoSimulacoes()
    {
        conexao.Open();
    }

    public MySqlTransaction IniciarGravacao(Universo universo, out long id)
    {
        MySqlTransaction transacao = conexao.BeginTransaction();
        try
        {
            using MySqlCommand comando = new MySqlCommand(
                "INSERT INTO simulacao (corpos_iniciais, iteracoes_previstas, segundos_por_iteracao) VALUES (@corpos, @iteracoes, @dt)",
                conexao, transacao);
            comando.Parameters.AddWithValue("@corpos", universo.Corpos.Count);
            comando.Parameters.AddWithValue("@iteracoes", universo.QtdIteracoes);
            comando.Parameters.AddWithValue("@dt", universo.TempoEntreIteracoes);
            comando.ExecuteNonQuery();
            id = comando.LastInsertedId;
            return transacao;
        }
        catch
        {
            transacao.Dispose();
            throw;
        }
    }

    public void GravarQuadro(MySqlTransaction transacao, long id, int iteracao, List<Corpo> corpos)
    {
        // Um INSERT por quadro reduz as idas ao banco. A ordem preserva o desenho.
        using MySqlCommand comando = new MySqlCommand("", conexao, transacao);
        comando.Parameters.AddWithValue("@id", id);
        comando.Parameters.AddWithValue("@iteracao", iteracao);
        StringBuilder sql = new StringBuilder(
            "INSERT INTO corpo_iteracao (simulacao_id, iteracao, ordem, nome, massa, densidade, pos_x, pos_y, vel_x, vel_y, volume, raio) VALUES ");

        for (int i = 0; i < corpos.Count; i++)
        {
            Corpo corpo = corpos[i];
            if (i > 0)
                sql.Append(", ");
            sql.Append($"(@id, @iteracao, {i}, @nome{i}, @massa{i}, @densidade{i}, @x{i}, @y{i}, @vx{i}, @vy{i}, @volume{i}, @raio{i})");
            comando.Parameters.AddWithValue("@nome" + i, corpo.Nome);
            comando.Parameters.AddWithValue("@massa" + i, corpo.Massa);
            comando.Parameters.AddWithValue("@densidade" + i, corpo.Densidade);
            comando.Parameters.AddWithValue("@x" + i, corpo.PosX);
            comando.Parameters.AddWithValue("@y" + i, corpo.PosY);
            comando.Parameters.AddWithValue("@vx" + i, corpo.VelX);
            comando.Parameters.AddWithValue("@vy" + i, corpo.VelY);
            comando.Parameters.AddWithValue("@volume" + i, corpo.Volume);
            comando.Parameters.AddWithValue("@raio" + i, corpo.Raio);
        }

        comando.CommandText = sql.ToString();
        comando.ExecuteNonQuery();
    }

    public void ConcluirGravacao(MySqlTransaction transacao, long id, int ultimaIteracao)
    {
        using MySqlCommand comando = new MySqlCommand(
            "UPDATE simulacao SET ultima_iteracao = @iteracao, encerrada_em = CURRENT_TIMESTAMP(6) WHERE id = @id",
            conexao, transacao);
        comando.Parameters.AddWithValue("@iteracao", ultimaIteracao);
        comando.Parameters.AddWithValue("@id", id);
        comando.ExecuteNonQuery();

        // A simulação só fica disponível após todos os quadros serem confirmados.
        transacao.Commit();
    }

    public bool Listar()
    {
        using MySqlCommand comando = new MySqlCommand(
            "SELECT id, encerrada_em, corpos_iniciais, ultima_iteracao, iteracoes_previstas FROM simulacao ORDER BY encerrada_em DESC, id DESC",
            conexao);
        using MySqlDataReader dados = comando.ExecuteReader();
        bool temSimulacoes = dados.HasRows;
        while (dados.Read())
        {
            Console.WriteLine($"{dados.GetInt64(0)} - {dados.GetDateTime(1):dd/MM/yyyy HH:mm:ss}" +
                              $" | {dados.GetInt32(2)} corpos iniciais | {dados.GetInt32(3)}/{dados.GetInt32(4)} iterações");
        }
        return temSimulacoes;
    }

    public MySqlTransaction IniciarLeitura()
    {
        // A limpeza diária pode remover a simulação enquanto o replay está aberto.
        return conexao.BeginTransaction(IsolationLevel.RepeatableRead);
    }

    public Universo Carregar(MySqlTransaction transacao, long id)
    {
        using MySqlCommand comando = new MySqlCommand(
            "SELECT ultima_iteracao, segundos_por_iteracao FROM simulacao WHERE id = @id",
            conexao, transacao);
        comando.Parameters.AddWithValue("@id", id);
        using MySqlDataReader dados = comando.ExecuteReader();

        if (!dados.Read())
            return null;

        return new Universo
        {
            QtdIteracoes = dados.GetInt32(0),
            TempoEntreIteracoes = dados.GetDouble(1)
        };
    }

    public IEnumerable<List<Corpo>> LerQuadros(MySqlTransaction transacao, long id)
    {
        using MySqlCommand comando = new MySqlCommand(
            "SELECT iteracao, nome, massa, densidade, pos_x, pos_y, vel_x, vel_y " +
            "FROM corpo_iteracao WHERE simulacao_id = @id ORDER BY iteracao, ordem",
            conexao, transacao);
        comando.Parameters.AddWithValue("@id", id);
        using MySqlDataReader dados = comando.ExecuteReader();

        List<Corpo> corpos = new List<Corpo>();
        int iteracao = 0;
        while (dados.Read())
        {
            int proximaIteracao = dados.GetInt32(0);
            if (proximaIteracao != iteracao)
            {
                yield return corpos;
                corpos = new List<Corpo>();
                iteracao = proximaIteracao;
            }

            corpos.Add(new Corpo(dados.GetString(1), dados.GetDouble(2), dados.GetDouble(3),
                new Vetor(dados.GetDouble(4), dados.GetDouble(5)),
                new Vetor(dados.GetDouble(6), dados.GetDouble(7))));
        }

        if (corpos.Count > 0)
            yield return corpos;
    }

    public void Dispose()
    {
        conexao.Dispose();
    }
}
