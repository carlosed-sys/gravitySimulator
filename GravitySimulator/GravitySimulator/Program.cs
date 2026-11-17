using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

public class Program
{
    // Pasta onde ficam todos os universos salvos (fica do lado do .exe)
    private static string pasta = Path.Combine(AppContext.BaseDirectory, "Salvos");

    [STAThread] 
    public static void Main()
    {
        Directory.CreateDirectory(pasta); 

        Universo universo;

        Console.WriteLine("1 - Novo universo aleatorio");
        Console.WriteLine("2 - Carregar universo salvo");
        Console.Write("Opcao: ");
        string opcao = Console.ReadLine();

        if (opcao == "2")
        {
            universo = EscolherSalvo();
            if (universo == null) return; 
        }
        else
        {
            Console.Write("Quantidade de corpos: ");
            int qtd = int.Parse(Console.ReadLine());
            Console.Write("Quantidade de iteracoes: ");
            int iter = int.Parse(Console.ReadLine());
            Console.Write("Tempo entre iteracoes (s): ");
            double dt = double.Parse(Console.ReadLine());

            universo = new Universo();
            universo.GerarAleatorio(qtd, iter, dt);

           
            Salvar(universo, "inicial");
            Console.WriteLine();
        }

        Console.Write("Ver em modo visual? (s/n): ");
        if (Console.ReadLine().ToLower() == "s")
        {
            
            Application.Run(new JanelaSimulador(universo));
        }
        else
        {
            universo.Simular(); 
        }

        
        Salvar(universo, "final");
    }

    
    private static void Salvar(Universo universo, string prefixo)
    {
        string nome = prefixo + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
        string caminho = Path.Combine(pasta, nome);

        Armazenamento arq = new ArmazenamentoTxt(caminho);
        arq.Salvar(universo);

        Console.WriteLine("Universo salvo em: " + caminho);
    }

    // Lista os arquivos da pasta Salvos 
    private static Universo EscolherSalvo()
    {
        string[] arquivos = Directory.GetFiles(pasta, "*.txt")
                                     .OrderByDescending(f => File.GetLastWriteTime(f))
                                     .ToArray();

        if (arquivos.Length == 0)
        {
            Console.WriteLine("Nenhum universo salvo em: " + pasta);
            return null;
        }

        Console.WriteLine("\nUniversos salvos:");
        for (int i = 0; i < arquivos.Length; i++)
        {
            Console.WriteLine((i + 1) + " - " + Path.GetFileName(arquivos[i]) +
                              "  (" + File.GetLastWriteTime(arquivos[i]).ToString("dd/MM/yyyy HH:mm:ss") + ")");
        }

        Console.Write("Escolha o numero: ");
        int escolha;
        if (!int.TryParse(Console.ReadLine(), out escolha) || escolha < 1 || escolha > arquivos.Length)
        {
            Console.WriteLine("Opcao invalida.");
            return null;
        }

        Armazenamento arq = new ArmazenamentoTxt(arquivos[escolha - 1]);
        return arq.Carregar();
    }
}