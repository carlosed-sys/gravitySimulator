using System;
using System.Collections.Generic;
using System.Threading.Tasks;


// Guarda a lista de corpos e roda a simulacao (movimento + colisoes).
internal class Universo
{
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

        for (int i = 1; i <= qtdCorpos; i++)
        {
            double massa = 1e23 + rnd.NextDouble() * 1e25;          // 1e23 a ~1e25 kg
            double densidade = 1000 + rnd.NextDouble() * 7000;      // 1000 a 8000 kg/m^3
            Vetor pos = new Vetor((rnd.NextDouble() * 2 - 1) * 1e8, // +-1e8 m
                                  (rnd.NextDouble() * 2 - 1) * 1e8);
            Vetor vel = new Vetor((rnd.NextDouble() * 2 - 1) * 50,  // +-50 m/s
                                  (rnd.NextDouble() * 2 - 1) * 50);

            Corpos.Add(new Corpo("Corpo" + i, massa, densidade, pos, vel));
        }
    }

    // Um passo da simulacao: calcula posicao dos corpos no proximo instante
    public void Passo()
    {
        double dt = TempoEntreIteracoes;

      
        Vetor[] aceleracoes = new Vetor[Corpos.Count];
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
        TratarColisoes();
    }

    
    private void TratarColisoes()
    {
        for (int i = 0; i < Corpos.Count; i++)
        {
            for (int j = i + 1; j < Corpos.Count; j++)
            {
                double distancia = (Corpos[j].Pos - Corpos[i].Pos).Modulo();

                if (distancia <= Corpos[i].Raio + Corpos[j].Raio)
                {
                    Corpos[i] = Fundir(Corpos[i], Corpos[j]);
                    Corpos.RemoveAt(j);
                    j--; 
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