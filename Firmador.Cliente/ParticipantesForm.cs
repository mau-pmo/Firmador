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
    private bool _cargando;
    private bool _recursosLiberados;

    public bool SesionExpirada { get; private set; }

    public ParticipantesForm(IFirmadorApiClient api, int documentoId, string titulo)
    {
        _api = api;
        _documentoId = documentoId;
        Text = "Participantes del documento";
        AutoScaleMode = AutoScaleMode.Font;
        Font = new Font("Segoe UI", 10);
        ClientSize = new Size(640, 580);
        MinimumSize = new Size(400, 350);
        StartPosition = FormStartPosition.CenterParent;
        ShowInTaskbar = false;
        MinimizeBox = false;
        MaximizeBox = false;

        var botones = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom, Height = 58, Padding = new Padding(12),
            FlowDirection = FlowDirection.RightToLeft
        };
        var cerrar = new Button { Text = "Cerrar", AutoSize = true, DialogResult = DialogResult.Cancel };
        _reintentar = new Button { Text = "Reintentar", AutoSize = true, Visible = false };
        _reintentar.Click += async (_, _) => await CargarAsync();
        botones.Controls.Add(cerrar);
        botones.Controls.Add(_reintentar);
        CancelButton = cerrar;

        _desplazamiento = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(20) };
        _contenido = new TableLayoutPanel
        {
            Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1, RowCount = 0
        };
        _contenido.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _desplazamiento.Controls.Add(_contenido);
        Controls.Add(_desplazamiento);
        Controls.Add(botones);
        _contenido.SizeChanged += (_, _) => AjustarAnchos();
        AgregarTexto(titulo, true);
        AgregarTexto("Cargando participantes…");
        Shown += async (_, _) => await CargarAsync();
        FormClosing += (_, _) => _cierre.Cancel();
    }

    private async Task CargarAsync()
    {
        if (_cargando || _cierre.IsCancellationRequested) return;
        _cargando = true;
        _reintentar.Visible = false;
        // Conservar el título al reemplazar el estado o los participantes.
        LimpiarParticipantes();
        AgregarTexto("Cargando participantes…");
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
            LimpiarParticipantes();
            AgregarTexto(ErroresConexion.EsFallaDeConexion(ex)
                ? ErroresConexion.Mensaje
                : $"No fue posible consultar los participantes.\n\n{ex.Message}");
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
            AgregarSeccion("Creador", [Nombre(participantes.Creador?.Nombre)]);
            AgregarSeccion("Editor", [Nombre(participantes.Editor?.Nombre)]);
            AgregarSeccion("Revisores", participantes.Revisores.Count == 0
                ? ["Sin revisores"]
                : participantes.Revisores.OrderBy(p => p.Orden).Select(p =>
                    $"{Nombre(p.Nombre)} — {Estado(p.Estado, "reviewed", "revisado", p.RevisadoAt)}"));
            AgregarSeccion("Firmantes", participantes.Firmantes.Count == 0
                ? ["Sin firmantes"]
                : participantes.Firmantes.OrderBy(p => p.Orden).Select(p =>
                    $"{(p.Tipo == "group" ? "Grupo: " : string.Empty)}{Nombre(p.Nombre)} — {EstadoFirmante(p)}"));
        }
        finally { _contenido.ResumeLayout(true); }
        _desplazamiento.AutoScrollPosition = Point.Empty;
    }

    private static string Nombre(string? nombre) => string.IsNullOrWhiteSpace(nombre) ? "No informado" : nombre;

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

    private void AgregarSeccion(string titulo, IEnumerable<string> lineas)
    {
        AgregarTexto(titulo + ":", true);
        foreach (var linea in lineas) AgregarTexto(linea);
    }

    private void AgregarTexto(string texto, bool destacado = false)
    {
        var etiqueta = new Label
        {
            Text = texto, AutoSize = true, Anchor = AnchorStyles.Left | AnchorStyles.Right,
            Margin = destacado ? new Padding(0, 14, 0, 6) : new Padding(0, 0, 0, 8),
            MaximumSize = new Size(Math.Max(1, _contenido.ClientSize.Width), 0)
        };
        if (destacado) etiqueta.Font = new Font(Font, FontStyle.Bold);
        var fila = _contenido.RowCount++;
        _contenido.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _contenido.Controls.Add(etiqueta, 0, fila);
    }

    private void LimpiarParticipantes()
    {
        for (var i = _contenido.Controls.Count - 1; i > 0; i--)
        {
            var control = _contenido.Controls[i];
            _contenido.Controls.Remove(control);
            control.Dispose();
        }
        _contenido.RowStyles.Clear();
        _contenido.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _contenido.RowCount = 1;
    }

    private void AjustarAnchos()
    {
        var ancho = Math.Max(1, _contenido.ClientSize.Width);
        foreach (Control control in _contenido.Controls)
            if (control.MaximumSize.Width != ancho) control.MaximumSize = new Size(ancho, 0);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_recursosLiberados)
        {
            _recursosLiberados = true;
            _cierre.Cancel();
            _cierre.Dispose();
        }
        base.Dispose(disposing);
    }
}
