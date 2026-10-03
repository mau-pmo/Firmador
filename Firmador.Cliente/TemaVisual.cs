namespace Firmador.Cliente;

internal static class TemaVisual
{
    internal static readonly Color Azul = Color.FromArgb(20, 99, 173);
    internal static readonly Color AzulOscuro = Color.FromArgb(15, 76, 134);
    internal static readonly Color Celeste = Color.FromArgb(234, 244, 252);
    internal static readonly Color Fondo = Color.FromArgb(245, 247, 250);
    internal static readonly Color Superficie = Color.White;
    internal static readonly Color Texto = Color.FromArgb(36, 49, 61);
    internal static readonly Color TextoSecundario = Color.FromArgb(82, 96, 109);
    internal static readonly Color Borde = Color.FromArgb(220, 227, 235);
    internal static readonly Color Correcto = Color.FromArgb(30, 117, 64);
    internal static readonly Color Advertencia = Color.FromArgb(145, 94, 0);

    internal static void EstilarBoton(Button boton, bool principal = false, bool discreto = false)
    {
        boton.FlatStyle = FlatStyle.Flat;
        boton.UseVisualStyleBackColor = false;
        boton.BackColor = principal ? Azul : Superficie;
        boton.ForeColor = principal ? Color.White : Azul;
        boton.FlatAppearance.BorderSize = discreto || principal ? 0 : 1;
        boton.FlatAppearance.BorderColor = Borde;
        boton.FlatAppearance.MouseOverBackColor = principal ? AzulOscuro : Celeste;
        boton.FlatAppearance.MouseDownBackColor = principal ? AzulOscuro : Celeste;
    }

    internal static Bitmap CrearIcono(string glifo, int lado, Color color)
    {
        var imagen = new Bitmap(lado, lado);
        using var graphics = Graphics.FromImage(imagen);
        using var fuente = new Font("Segoe MDL2 Assets", lado * 0.8F, FontStyle.Regular, GraphicsUnit.Pixel);
        using var pincel = new SolidBrush(color);
        using var formato = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
        graphics.DrawString(glifo, fuente, pincel, new RectangleF(0, 0, lado, lado), formato);
        return imagen;
    }
}

// Borde fino independiente del tema nativo de Windows y del tamaño del control.
internal sealed class PanelBorde : Panel
{
    internal bool ResaltarFoco { get; init; }

    internal PanelBorde()
    {
        BackColor = TemaVisual.Superficie;
        SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (ClientSize.Width < 2 || ClientSize.Height < 2) return;
        using var lapiz = new Pen(ResaltarFoco && ContainsFocus ? TemaVisual.Azul : TemaVisual.Borde);
        e.Graphics.DrawRectangle(lapiz, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
    }
}

