using System;
using System.Collections.Generic;
using System.Threading.Tasks;


// Guarda a lista de corpos e roda a simulacao (movimento + colisoes).
internal class Universo
{
    public const int MaximoCorpos = 400;
    public const double MaximoSegundosPorIteracao = 3600;
    private const double MaximoPassoFisico = 10;

    public List<Corpo> Corpos = new List<Corpo>();
    public int QtdIteracoes;
    public double TempoEntreIteracoes; // segundos (dt)

    private Random rnd = new Random();

    // Distribui corpos aleatoriamente na tela (valores todos no SI)
    public void GerarAleatorio(int qtdCorpos, int qtdIteracoes, double dt)
    {
        QtdIteracoes = qtdIteracoes;
        TempoEntreIteracoes = dt;
        Corpos.Clear();

        // Uma célula por corpo mantém espaço entre as superfícies mesmo com 400 corpos.
        const int colunas = 20;
        const double larguraCelula = 1e7;
        int[] celulas = new int[MaximoCorpos];
        for (int i = 0; i < celulas.Length; i++)
            celulas[i] = i;
        rnd.Shuffle(celulas.AsSpan());

        for (int i = 0; i < qtdCorpos; i++)
        {
            double massa = 1e21 + rnd.NextDouble() * 9.9e22;        // 1e21 a 1e23 kg
            double densidade = 1000 + rnd.NextDouble() * 7000;      // 1000 a 8000 kg/m^3
            double x = (celulas[i] % colunas - 9.5) * larguraCelula;
            double y = (celulas[i] / colunas - 9.5) * larguraCelula;
            Vetor pos = new Vetor(x + (rnd.NextDouble() * 2 - 1) * 1e6,
                                  y + (rnd.NextDouble() * 2 - 1) * 1e6);
            Vetor vel = new Vetor((rnd.NextDouble() * 2 - 1) * 50,  // +-50 m/s
                                  (rnd.NextDouble() * 2 - 1) * 50);

            Corpos.Add(new Corpo("Corpo" + (i + 1), massa, densidade, pos, vel));
        }
    }

    // Um passo da simulacao: calcula posicao dos corpos no proximo instante
    public void Passo()
    {
        // Contatos existentes precisam ser fundidos antes de aplicar qualquer força.
        TratarColisoes(0);

        int passos = (int)Math.Ceiling(TempoEntreIteracoes / MaximoPassoFisico);
        double dt = TempoEntreIteracoes / passos;
        Vetor[] aceleracoes = new Vetor[Corpos.Count];
        for (int i = 0; i < passos; i++)
            AvancarFisica(dt, aceleracoes);
    }

    private void AvancarFisica(double dt, Vetor[] aceleracoes)
    {
        // Cada tarefa lê as mesmas posições e escreve apenas no seu índice.
        Parallel.For(0, Corpos.Count, i =>
        {
            Vetor forcaTotal = new Vetor(0, 0);
            for (int j = 0; j < Corpos.Count; j++)
            {
                if (i == j) continue; 
                forcaTotal = forcaTotal + Gravidade.Forca(Corpos[i], Corpos[j]);
            }
            aceleracoes[i] = Gravidade.Aceleracao(forcaTotal, Corpos[i].Massa);
        });

       
        for (int i = 0; i < Corpos.Count; i++)
        {
            Corpos[i].Vel = Corpos[i].Vel + aceleracoes[i] * dt;
            Corpos[i].Pos = Corpos[i].Pos + Corpos[i].Vel * dt;
        }

        
        // A fusão altera a lista, então as colisões mantêm a ordem original.
        TratarColisoes(dt);
    }

    
    private void TratarColisoes(double dt)
    {
        HashSet<Corpo> fundidos = null;
        for (int i = 0; i < Corpos.Count; i++)
        {
            for (int j = i + 1; j < Corpos.Count; j++)
            {
                Corpo a = Corpos[i];
                Corpo b = Corpos[j];
                Vetor separacao = b.Pos - a.Pos;
                Vetor deslocamento = (b.Vel - a.Vel) * dt;
                double quadrado = deslocamento.X * deslocamento.X + deslocamento.Y * deslocamento.Y;
                if (quadrado > 0 && (fundidos == null || (!fundidos.Contains(a) && !fundidos.Contains(b))))
                {
                    // A menor distância ao longo do passo detecta corpos que se atravessariam.
                    double projecao = separacao.X * deslocamento.X + separacao.Y * deslocamento.Y;
                    separacao -= deslocamento * Math.Clamp(projecao / quadrado, 0, 1);
                }

                if (separacao.Modulo() <= a.Raio + b.Raio)
                {
                    Corpos[i] = Fundir(a, b);
                    // Um corpo novo ainda não percorreu uma trajetória neste passo.
                    (fundidos ??= new HashSet<Corpo>()).Add(Corpos[i]);
                    Corpos.RemoveAt(j);
                    // O novo corpo pode tocar um par que já foi examinado.
                    i = -1;
                    break;
                }
            }
        }
    }

    private Corpo Fundir(Corpo a, Corpo b)
    {
        double massaTotal = a.Massa + b.Massa;

       
        Vetor quantidadeMovimento = a.Vel * a.Massa + b.Vel * b.Massa;
        Vetor velFinal = quantidadeMovimento / massaTotal;

       
        Vetor posFinal = (a.Pos * a.Massa + b.Pos * b.Massa) / massaTotal;

        
        double densidadeFinal = massaTotal / (a.Volume + b.Volume);

        
        string nome = a.Massa >= b.Massa ? a.Nome : b.Nome;

        return new Corpo(nome, massaTotal, densidadeFinal, posFinal, velFinal);
    }

    
    public void Exibir(int iteracao)
    {
        Console.WriteLine("=== Iteracao " + iteracao + " (t = " + (iteracao * TempoEntreIteracoes) + " s) ===");
        foreach (Corpo c in Corpos)
        {
            Console.WriteLine(
                c.Nome +
                " | m=" + c.Massa.ToString("E3") + " kg" +
                " | raio=" + c.Raio.ToString("E3") + " m" +
                " | pos=(" + c.PosX.ToString("E3") + ", " + c.PosY.ToString("E3") + ") m" +
                " | vel=(" + c.VelX.ToString("F2") + ", " + c.VelY.ToString("F2") + ") m/s");
        }
        Console.WriteLine();
    }
}