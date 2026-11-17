using System;


internal struct Vetor
{
    public double X;
    public double Y;

    public Vetor(double x, double y)
    {
        X = x;
        Y = y;
    }

  
    public static Vetor operator +(Vetor a, Vetor b) => new Vetor(a.X + b.X, a.Y + b.Y);
    public static Vetor operator -(Vetor a, Vetor b) => new Vetor(a.X - b.X, a.Y - b.Y);
    public static Vetor operator *(Vetor a, double k) => new Vetor(a.X * k, a.Y * k);
    public static Vetor operator /(Vetor a, double k) => new Vetor(a.X / k, a.Y / k);

    
    public double Modulo()
    {
        return Math.Sqrt(X * X + Y * Y);
    }

    
    public Vetor Normalizado()
    {
        double m = Modulo();
        if (m == 0) return new Vetor(0, 0); 
        return new Vetor(X / m, Y / m);
    }
}