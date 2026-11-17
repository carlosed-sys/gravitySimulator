using System;


public class Corpo
{
   
    public const double DENSIDADE_MAXIMA = 4e17;

    public string Nome;
    public double Massa;      // kg

    private double densidade; // kg/m^3
    public double Densidade
    {
        get { return densidade; }
        
        set { densidade = Math.Min(value, DENSIDADE_MAXIMA); }
    }

    public Vetor Pos;  // m
    public Vetor Vel;  // m/s

   
    public double PosX { get { return Pos.X; } }
    public double PosY { get { return Pos.Y; } }
    public double VelX { get { return Vel.X; } }
    public double VelY { get { return Vel.Y; } }

    
    public double Volume
    {
        get { return Massa / Densidade; }
    }

  
    public double Raio
    {
        get { return Math.Cbrt(3 * Volume / (4 * Math.PI)); }
    }

    public Corpo(string nome, double massa, double densidade, Vetor pos, Vetor vel)
    {
        Nome = nome;
        Massa = massa;
        Densidade = densidade;
        Pos = pos;
        Vel = vel;
    }
}