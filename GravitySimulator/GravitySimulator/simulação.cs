using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;


internal class JanelaSimulador : Form
{
    private Universo universo;
    private System.Windows.Forms.Timer timer;
    private int iteracaoAtual = 0;

    
    private const double LadoUniverso = 3e8;
    private double areaVisivel = LadoUniverso * 1.05;

    
    private int passosPorQuadro = 5;
    private bool pausado = false;

    
    private const int TAMANHO_RASTRO = 300;
    private Dictionary<string, List<Vetor>> rastros = new Dictionary<string, List<Vetor>>();

    
    private Color[] cores = {
        Color.DeepSkyBlue, Color.Orange, Color.LimeGreen, Color.HotPink,
        Color.Yellow, Color.MediumPurple, Color.Tomato, Color.Aquamarine
    };

    public JanelaSimulador(Universo universo)
    {
        this.universo = universo;

        Text = "Simulador Gravitacional 2D";
        ClientSize = new Size(800, 800);
        BackColor = Color.Black;
        DoubleBuffered = true; 

        GuardarRastro(); 

        timer = new System.Windows.Forms.Timer();
        timer.Interval = 16; 
        timer.Tick += Timer_Tick;
        timer.Start();
    }

   
    private void Timer_Tick(object sender, EventArgs e)
    {
        if (!pausado && iteracaoAtual < universo.QtdIteracoes)
        {
            for (int i = 0; i < passosPorQuadro && iteracaoAtual < universo.QtdIteracoes; i++)
            {
                universo.Passo();
                iteracaoAtual++;
            }
            GuardarRastro();
        }
        Invalidate(); 
    }

  
    private void GuardarRastro()
    {
        foreach (Corpo c in universo.Corpos)
        {
            if (!rastros.ContainsKey(c.Nome))
                rastros[c.Nome] = new List<Vetor>();

            List<Vetor> lista = rastros[c.Nome];
            lista.Add(c.Pos);
            if (lista.Count > TAMANHO_RASTRO)
                lista.RemoveAt(0); 
        }
    }

    
    private PointF ParaTela(Vetor p, double escala)
    {
        float x = ClientSize.Width / 2f + (float)(p.X * escala);
        float y = ClientSize.Height / 2f - (float)(p.Y * escala); 
        return new PointF(x, y);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        // Escala: converte metros em pixels
        double escala = Math.Min(ClientSize.Width, ClientSize.Height) / areaVisivel;

        // A borda é fixa no espaço do universo, com centro em (0, 0).
        double metadeLado = LadoUniverso / 2;
        PointF canto = ParaTela(new Vetor(-metadeLado, metadeLado), escala);
        float ladoPx = (float)(LadoUniverso * escala);
        g.DrawRectangle(Pens.DimGray, canto.X, canto.Y, ladoPx, ladoPx);

        foreach (Corpo c in universo.Corpos)
        {
            Color cor = cores[Math.Abs(c.Nome.GetHashCode()) % cores.Length];

            // Rastro (linha da trajetoria)
            List<Vetor> lista = rastros[c.Nome];
            if (lista.Count >= 2)
            {
                PointF[] pontos = new PointF[lista.Count];
                for (int i = 0; i < lista.Count; i++)
                    pontos[i] = ParaTela(lista[i], escala);

                using (Pen caneta = new Pen(Color.FromArgb(120, cor), 1))
                    g.DrawLines(caneta, pontos);
            }

            
            PointF pos = ParaTela(c.Pos, escala);
            float raioPx = (float)Math.Max(4, c.Raio * escala);

            using (SolidBrush pincel = new SolidBrush(cor))
                g.FillEllipse(pincel, pos.X - raioPx, pos.Y - raioPx, raioPx * 2, raioPx * 2);

            g.DrawString(c.Nome, Font, Brushes.White, pos.X + raioPx + 2, pos.Y - raioPx);
        }

       
        string estado = pausado ? "PAUSADO" : (iteracaoAtual >= universo.QtdIteracoes ? "FIM" : "Rodando");
        g.DrawString("Iteracao: " + iteracaoAtual + " / " + universo.QtdIteracoes +
                     "   Corpos: " + universo.Corpos.Count +
                     "   Velocidade: " + passosPorQuadro + "x   [" + estado + "]",
                     Font, Brushes.White, 10, 10);
        g.DrawString("Espaco = pausar | Cima/Baixo = velocidade | Scroll = zoom",
                     Font, Brushes.Gray, 10, 30);
    }

    
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.KeyCode == Keys.Space)
            pausado = !pausado;
        else if (e.KeyCode == Keys.Up)
            passosPorQuadro = Math.Min(passosPorQuadro * 2, 1024);
        else if (e.KeyCode == Keys.Down)
            passosPorQuadro = Math.Max(passosPorQuadro / 2, 1);
    }

    
    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);

        if (e.Delta > 0)
            areaVisivel *= 0.8;  // aproxima
        else
            areaVisivel *= 1.25; // afasta
    }
}