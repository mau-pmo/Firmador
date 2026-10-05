using System.Globalization;
using Firmador.ApiClient.Abstractions;
using Firmador.Cliente.Services;
using Firmador.Core.Documentos;

namespace Firmador.Cliente;

public sealed class ParticipantesForm : Form
{
    private readonly IFirmadorApiClient _api;
    private readonly int _documentoId;
    private readonly CancellationTokenSource _cierre = new();
    private readonly TableLayoutPanel _contenido;
    private readonly Panel _desplazamiento;
    private readonly Button _reintentar;
    private readonly List<IDisposable> _recursosVisuales = [];
    private readonly Font _fuenteTitulo;
    private readonly Font _fuenteDetalle;
    private readonly Bitmap _iconoEstado;
    private bool _cargando;
    private bool _recursosLiberados;

    public bool SesionExpirada { get; private set; }

    public ParticipantesForm(IFirmadorApiClient api, int documentoId, string titulo)
    {
        _api = api;
        _documentoId = documentoId;
        SuspendLayout();
        Text = "Participantes del documento";
        AutoScaleDimensions = new SizeF(96, 96);
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = CrearFuente(10);
        _fuenteTitulo = CrearFuente(14, FontStyle.Bold);
        _fuenteDetalle = CrearFuente(9);
        BackColor = TemaVisual.Fondo;
        ForeColor = TemaVisual.Texto;
        ClientSize = new Size(680, 600);
        MinimumSize = new Size(400, 350);
        StartPosition = FormStartPosition.CenterParent;
        ShowInTaskbar = false;
        MinimizeBox = false;
        MaximizeBox = false;

        var principal = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Margin = Padding.Empty
        };
        principal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        principal.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        principal.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        principal.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));

        var cabecera = new TableLayoutPanel
        {
            Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, RowCount = 1,
            Padding = new Padding(24, 20, 24, 20), Margin = Padding.Empty,
            BackColor = TemaVisual.Superficie
        };
        cabecera.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 44));
        cabecera.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        var icono = TemaVisual.CrearIcono("\uE716", 32, TemaVisual.Azul);
        _recursosVisuales.Add(icono);
        _iconoEstado = TemaVisual.CrearIcono("\uE946", 24, TemaVisual.Azul);
        _recursosVisuales.Add(_iconoEstado);
        cabecera.Controls.Add(new PictureBox
        {
            Image = icono, SizeMode = PictureBoxSizeMode.Zoom, Size = new Size(32, 32),
            Anchor = AnchorStyles.Top | AnchorStyles.Left, Margin = new Padding(0, 2, 0, 0), TabStop = false
        }, 0, 0);
        var titulos = CrearLista();
        AgregarFila(titulos, CrearTexto("Participantes del documento", _fuenteTitulo, TemaVisual.Texto));
        AgregarFila(titulos, CrearTexto(titulo, Font, TemaVisual.TextoSecundario, new Padding(0, 8, 0, 0)));
        cabecera.Controls.Add(titulos, 1, 0);
        principal.Controls.Add(cabecera, 0, 0);

        var botones = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, Padding = new Padding(24, 16, 24, 16),
            FlowDirection = FlowDirection.RightToLeft, WrapContents = false,
            Margin = Padding.Empty, BackColor = TemaVisual.Superficie
        };
        botones.Paint += (_, e) =>
        {
            using var lapiz = new Pen(TemaVisual.Borde);
            e.Graphics.DrawLine(lapiz, 0, 0, botones.Width, 0);
        };
        var cerrar = new Button
        {
            Text = "Cerrar", Size = new Size(108, 36), DialogResult = DialogResult.Cancel,
            Margin = Padding.Empty, TabIndex = 1
        };
        _reintentar = new Button
        {
            Text = "Reintentar", Size = new Size(108, 36), Visible = false,
            Margin = new Padding(0, 0, 8, 0), TabIndex = 0
        };
        TemaVisual.EstilarBoton(cerrar);
        TemaVisual.EstilarBoton(_reintentar, principal: true);
        _reintentar.Click += async (_, _) => await CargarAsync();
        botones.Controls.Add(cerrar);
        botones.Controls.Add(_reintentar);
        CancelButton = cerrar;
        principal.Controls.Add(botones, 0, 2);

        _desplazamiento = new Panel
        {
            Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(24), Margin = Padding.Empty
        };
        _contenido = CrearLista();
        _desplazamiento.Controls.Add(_contenido);
        principal.Controls.Add(_desplazamiento, 0, 1);
        Controls.Add(principal);
        MostrarEstado("Cargando participantes…");
        Shown += async (_, _) => await CargarAsync();
        FormClosing += (_, _) => _cierre.Cancel();
        ResumeLayout(true);
    }

    private async Task CargarAsync()
    {
        if (_cargando || _cierre.IsCancellationRequested) return;
        _cargando = true;
        _reintentar.Visible = false;
        MostrarEstado("Cargando participantes…");
        var cancellationToken = _cierre.Token;
        try
        {
            var participantes = await _api.ObtenerParticipantesAsync(_documentoId, cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
            MostrarParticipantes(participantes);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        catch (SesionExpiradaException)
        {
            if (cancellationToken.IsCancellationRequested) return;
            SesionExpirada = true;
            Close();
        }
        catch (Exception ex)
        {
            if (cancellationToken.IsCancellationRequested) return;
            MostrarEstado(ErroresConexion.EsFallaDeConexion(ex)
                ? ErroresConexion.Mensaje
                : $"No fue posible consultar los participantes.\n\n{ex.Message}", error: true);
            _reintentar.Visible = true;
        }
        finally { _cargando = false; }
    }

    private void MostrarParticipantes(DocumentoParticipantes participantes)
    {
        _contenido.SuspendLayout();
        try
        {
            LimpiarParticipantes();
            AgregarSeccion("Creador", [new FilaParticipante(Nombre(participantes.Creador?.Nombre))]);
            AgregarSeccion("Editor", [new FilaParticipante(Nombre(participantes.Editor?.Nombre))]);
            AgregarSeccion("Revisores", participantes.Revisores.Count == 0
                ? [new FilaParticipante("Sin revisores")]
                : participantes.Revisores.OrderBy(p => p.Orden).Select(p => new FilaParticipante(
                    Nombre(p.Nombre), Estado(p.Estado, "reviewed", "revisado", p.RevisadoAt),
                    ColorEstado(p.Estado, "reviewed"))));
            AgregarSeccion("Firmantes", participantes.Firmantes.Count == 0
                ? [new FilaParticipante("Sin firmantes")]
                : participantes.Firmantes.OrderBy(p => p.Orden).Select(p => new FilaParticipante(
                    $"{(p.Tipo == "group" ? "Grupo: " : string.Empty)}{Nombre(p.Nombre)}",
                    EstadoFirmante(p), ColorEstado(p.Estado, "signed"))));
        }
        finally { _contenido.ResumeLayout(true); }
        _desplazamiento.AutoScrollPosition = Point.Empty;
    }

    private static string Nombre(string? nombre) => string.IsNullOrWhiteSpace(nombre) ? "No informado" : nombre;

    private static Color ColorEstado(string? estado, string completado) => estado == "pending"
        ? TemaVisual.Advertencia : estado == completado ? TemaVisual.Correcto : TemaVisual.TextoSecundario;

    private static string EstadoFirmante(FirmanteDocumento firmante)
    {
        if (firmante.Tipo != "group" || firmante.Estado != "signed")
            return Estado(firmante.Estado, "signed", "firmado", firmante.FirmadoAt);

        // El grupo puede completarse con la firma de solo algunos miembros.
        var nombres = firmante.Miembros.Where(miembro => miembro.Estado == "signed")
            .Select(miembro => Nombre(miembro.Nombre)).ToArray();
        return nombres.Length == 0
            ? "firmado — firmantes no informados"
            : $"firmado por: {string.Join(", ", nombres)}";
    }

    private static string Estado(string? estado, string completado, string accion, DateTimeOffset? fecha)
    {
        if (estado == "pending") return "pendiente";
        if (estado == completado)
            return fecha.HasValue
                ? $"{accion} el: {fecha.Value.ToString("d/M/yyyy", CultureInfo.InvariantCulture)}"
                : $"{accion} — fecha no informada";
        return string.IsNullOrWhiteSpace(estado) ? "estado no informado" : estado;
    }

    private sealed record FilaParticipante(string Nombre, string? Detalle = null, Color? Color = null);

    private void AgregarSeccion(string titulo, IEnumerable<FilaParticipante> filas)
    {
        var tarjeta = new PanelBorde
        {
            Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(Escalar(16)), Margin = new Padding(0, 0, 0, Escalar(16))
        };
        var lista = CrearLista();
        AgregarFila(lista, CrearTexto(titulo, _fuenteTitulo, TemaVisual.Texto, new Padding(0, 0, 0, Escalar(12))));
        var primera = true;
        foreach (var fila in filas)
        {
            if (!primera)
                AgregarFila(lista, new Panel
                {
                    Height = Escalar(1), Dock = DockStyle.Top, BackColor = TemaVisual.Borde,
                    Margin = new Padding(0, Escalar(12), 0, Escalar(12))
                });
            primera = false;
            var sinDatos = fila.Nombre == "No informado" || fila.Nombre == "Sin revisores" || fila.Nombre == "Sin firmantes";
            AgregarFila(lista, CrearTexto(fila.Nombre, Font, sinDatos ? TemaVisual.TextoSecundario : TemaVisual.Texto));
            if (fila.Detalle is not null)
                AgregarFila(lista, CrearTexto(fila.Detalle, _fuenteDetalle, fila.Color ?? TemaVisual.TextoSecundario,
                    new Padding(0, Escalar(4), 0, 0)));
        }
        tarjeta.Controls.Add(lista);
        AgregarFila(_contenido, tarjeta);
    }

    private void MostrarEstado(string texto, bool error = false)
    {
        LimpiarParticipantes();
        var tarjeta = new PanelBorde
        {
            Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(Escalar(20)), Margin = Padding.Empty
        };
        var lista = CrearLista();
        AgregarFila(lista, new PictureBox
        {
            Image = _iconoEstado, SizeMode = PictureBoxSizeMode.Zoom, Size = new Size(Escalar(24), Escalar(24)),
            Margin = new Padding(0, 0, 0, Escalar(12)), TabStop = false
        });
        AgregarFila(lista, CrearTexto(texto, Font, error ? TemaVisual.Texto : TemaVisual.TextoSecundario));
        tarjeta.Controls.Add(lista);
        AgregarFila(_contenido, tarjeta);
        _desplazamiento.AutoScrollPosition = Point.Empty;
    }

    private static TableLayoutPanel CrearLista()
    {
        var lista = new TableLayoutPanel
        {
            Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1, RowCount = 0, Margin = Padding.Empty
        };
        lista.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        // El ancho disponible determina el ajuste de línea, incluso al aparecer el scroll.
        lista.SizeChanged += (_, _) =>
        {
            foreach (Control control in lista.Controls)
            {
                if (control is not Label etiqueta) continue;
                var ancho = Math.Max(1, lista.ClientSize.Width - lista.Padding.Horizontal - etiqueta.Margin.Horizontal);
                if (etiqueta.MaximumSize.Width != ancho) etiqueta.MaximumSize = new Size(ancho, 0);
            }
        };
        return lista;
    }

    private static Label CrearTexto(string texto, Font fuente, Color color, Padding? margen = null) => new()
    {
        Text = texto, Font = fuente, ForeColor = color, AutoSize = true,
        Dock = DockStyle.Top, Margin = margen ?? Padding.Empty, UseMnemonic = false
    };

    private static void AgregarFila(TableLayoutPanel lista, Control control)
    {
        var fila = lista.RowCount++;
        lista.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        if (control is Label etiqueta)
            etiqueta.MaximumSize = new Size(Math.Max(1, lista.ClientSize.Width - lista.Padding.Horizontal - etiqueta.Margin.Horizontal), 0);
        lista.Controls.Add(control, 0, fila);
    }

    private void LimpiarParticipantes()
    {
        while (_contenido.Controls.Count > 0)
        {
            var control = _contenido.Controls[0];
            _contenido.Controls.Remove(control);
            control.Dispose();
        }
        _contenido.RowStyles.Clear();
        _contenido.RowCount = 0;
    }

    private int Escalar(int valor) => (int)Math.Round(valor * DeviceDpi / 96F);

    private Font CrearFuente(float puntos, FontStyle estilo = FontStyle.Regular)
    {
        var fuente = new Font("Segoe UI", puntos, estilo);
        _recursosVisuales.Add(fuente);
        return fuente;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_recursosLiberados)
        {
            _recursosLiberados = true;
            _cierre.Cancel();
            _cierre.Dispose();
            base.Dispose(disposing);
            foreach (var recurso in _recursosVisuales) recurso.Dispose();
            _recursosVisuales.Clear();
            return;
        }
        base.Dispose(disposing);
    }
}
