using System;
using System.Globalization;
using System.IO;


internal abstract class Armazenamento
{
    public abstract void Salvar(Universo universo);
    public abstract Universo Carregar();
}


internal class ArmazenamentoTxt : Armazenamento
{
    private string caminho;

    public ArmazenamentoTxt(string caminho)
    {
        this.caminho = caminho;
    }

  
    public override void Salvar(Universo u)
    {
        
        CultureInfo ci = CultureInfo.InvariantCulture;

        using (StreamWriter sw = new StreamWriter(caminho))
        {
            sw.WriteLine(u.Corpos.Count.ToString(ci) + ";" +
                         u.QtdIteracoes.ToString(ci) + ";" +
                         u.TempoEntreIteracoes.ToString("R", ci));

            foreach (Corpo c in u.Corpos)
            {
                sw.WriteLine(c.Nome + ";" +
                             c.Massa.ToString("R", ci) + ";" +
                             c.Densidade.ToString("R", ci) + ";" +
                             c.PosX.ToString("R", ci) + ";" +
                             c.PosY.ToString("R", ci) + ";" +
                             c.VelX.ToString("R", ci) + ";" +
                             c.VelY.ToString("R", ci));
            }
        }
    }

    public override Universo Carregar()
    {
        CultureInfo ci = CultureInfo.InvariantCulture;
        Universo u = new Universo();

        string[] linhas = File.ReadAllLines(caminho);

      
        string[] cab = linhas[0].Split(';');
        int qtdCorpos = int.Parse(cab[0], ci);
        u.QtdIteracoes = int.Parse(cab[1], ci);
        u.TempoEntreIteracoes = double.Parse(cab[2], ci);

        for (int i = 1; i <= qtdCorpos; i++)
        {
            string[] p = linhas[i].Split(';');
            Corpo c = new Corpo(
                p[0],
                double.Parse(p[1], ci),
                double.Parse(p[2], ci),
                new Vetor(double.Parse(p[3], ci), double.Parse(p[4], ci)),
                new Vetor(double.Parse(p[5], ci), double.Parse(p[6], ci)));
            u.Corpos.Add(c);
        }

        return u;
    }
}