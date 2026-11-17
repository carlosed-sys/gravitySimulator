using System;

internal static class Gravidade
{
  
    public const double G = 6.674e-11;

    
    public static Vetor Forca(Corpo a, Corpo b)
    {
        Vetor diferenca = b.Pos - a.Pos;     
        double distancia = diferenca.Modulo();

        if (distancia == 0) return new Vetor(0, 0); 

        double intensidade = G * a.Massa * b.Massa / (distancia * distancia);
        return diferenca.Normalizado() * intensidade;
    }

  
    public static Vetor Aceleracao(Vetor forca, double massa)
    {
        return forca / massa;
    }
}