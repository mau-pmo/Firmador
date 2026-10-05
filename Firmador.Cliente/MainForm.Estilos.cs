using Firmador.Cliente.ViewModels;

namespace Firmador.Cliente;

public partial class MainForm
{
    private readonly List<IDisposable> _recursosVisuales = [];
    private bool _operacionEnCurso;
    private Bitmap? _iconoPdf;
    private Bitmap? _iconoIntervinientes;

    private void AplicarEstilos()
    {
        BackColor = TemaVisual.Fondo;
        ForeColor = TemaVisual.Texto;
        layoutPrincipal.BackColor = TemaVisual.Superficie;
        lblAplicacion.Font = CrearFuente(21, FontStyle.Bold);
        lblDocumentosTitulo.Font = CrearFuente(14, FontStyle.Bold);
        var destacada = CrearFuente(11, FontStyle.Bold);
        lblCertificadoSeleccionado.Font = destacada;
        lblResumenSeleccion.Font = destacada;
        var secundaria = CrearFuente(9, FontStyle.Regular);
        foreach (var label in new[] { lblCertificadoTitulo, lblCertificadoDetalle,
                     lblCertificadoEstado, lblSeleccion, lblTotalDocumentos, lblPagina, lblResumenCertificado })
        {
            label.Font = secundaria;
            label.ForeColor = TemaVisual.TextoSecundario;
        }
        lblEstadoGrilla.ForeColor = TemaVisual.TextoSecundario;
        lblEstadoGrilla.BackColor = TemaVisual.Superficie;
        layoutCertificado.Paint += PintarSeparador;
        layoutDocumentos.Paint += PintarSeparador;
        layoutFirma.Paint += PintarSeparador;
        panelGrilla.Paint += PintarBordeGrilla;
        panelGrilla.Resize += (_, _) => panelGrilla.Invalidate();

        foreach (var button in new[] { btnBuscar, btnSeleccionarCertificado, btnPaginaAnterior, btnPaginaSiguiente })
            TemaVisual.EstilarBoton(button);
        foreach (var button in new[] { btnSalir, btnMarcarTodos, btnLimpiarSeleccion })
            TemaVisual.EstilarBoton(button, discreto: true);
        btnMarcarTodos.Font = secundaria;
        btnLimpiarSeleccion.Font = secundaria;
        TemaVisual.EstilarBoton(btnFirmarDocumentos, principal: true);
        btnFirmarDocumentos.Font = CrearFuente(10, FontStyle.Bold);
        btnFirmarDocumentos.EnabledChanged += (_, _) =>
        {
            btnFirmarDocumentos.BackColor = btnFirmarDocumentos.Enabled ? TemaVisual.Azul : TemaVisual.Fondo;
        };

        // Glifos de la biblioteca de iconos de Windows, sin una dependencia adicional.
        picAplicacion.Image = CrearIconoWindows("\uE70F", 30, TemaVisual.Azul);
        picCertificado.Image = CrearIconoWindows("\uEB95", 30, TemaVisual.Azul);
        _iconoPdf = CrearIconoWindows("\uE8A5", 18, TemaVisual.Azul);
        _iconoIntervinientes = CrearIconoWindows("\uE716", 18, TemaVisual.Azul);
        ConfigurarIcono(btnSalir, "\uE8BB", TemaVisual.Azul);
        ConfigurarIcono(btnPaginaAnterior, "\uE76B", TemaVisual.Azul);
        ConfigurarIcono(btnPaginaSiguiente, "\uE76C", TemaVisual.Azul);
        toolTip.SetToolTip(btnPaginaAnterior, "Página anterior");
        toolTip.SetToolTip(btnPaginaSiguiente, "Página siguiente");

        dgvDocumentos.BackgroundColor = TemaVisual.Superficie;
        dgvDocumentos.BorderStyle = BorderStyle.None;
        dgvDocumentos.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        dgvDocumentos.GridColor = TemaVisual.Borde;
        dgvDocumentos.EnableHeadersVisualStyles = false;
        dgvDocumentos.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        dgvDocumentos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        dgvDocumentos.ColumnHeadersHeight = Escalar(40);
        dgvDocumentos.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = TemaVisual.Fondo, ForeColor = TemaVisual.Texto,
            SelectionBackColor = TemaVisual.Fondo, SelectionForeColor = TemaVisual.Texto,
            Font = CrearFuente(10, FontStyle.Bold), Padding = new Padding(Escalar(12), 0, 0, 0)
        };
        dgvDocumentos.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = TemaVisual.Superficie, ForeColor = TemaVisual.Texto,
            SelectionBackColor = TemaVisual.Superficie, SelectionForeColor = TemaVisual.Texto,
            Padding = new Padding(Escalar(12), 0, Escalar(6), 0)
        };
        dgvDocumentos.RowTemplate.Height = Escalar(40);
        colSeleccionar.DefaultCellStyle.Padding = new Padding(0);
        colSeleccionar.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        foreach (var columna in new[] { colVerPdf, colVerParticipantes })
        {
            columna.LinkColor = TemaVisual.Azul;
            columna.ActiveLinkColor = TemaVisual.AzulOscuro;
            columna.VisitedLinkColor = TemaVisual.Azul;
            columna.TrackVisitedState = false;
            columna.LinkBehavior = LinkBehavior.HoverUnderline;
            columna.DefaultCellStyle.Padding = new Padding(Escalar(36), 0, Escalar(6), 0);
        }
        AjustarMedidasGrilla();
        dgvDocumentos.DpiChangedAfterParent += (_, _) => AjustarMedidasGrilla();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        AjustarMedidasGrilla();
        AjustarVentanaAlMonitor();
    }

    protected override void OnDpiChanged(DpiChangedEventArgs e)
    {
        base.OnDpiChanged(e);
        AjustarMedidasGrilla();
        if (IsHandleCreated) BeginInvoke(new Action(AjustarVentanaAlMonitor));
    }

    private void AjustarVentanaAlMonitor()
    {
        if (WindowState != FormWindowState.Normal) return;
        var area = Screen.FromHandle(Handle).WorkingArea;
        var margen = Escalar(12);
        var disponible = new Size(Math.Max(1, area.Width - margen * 2), Math.Max(1, area.Height - margen * 2));
        MinimumSize = new Size(Math.Min(Escalar(900), disponible.Width), Math.Min(Escalar(580), disponible.Height));
        var tamano = new Size(Math.Min(Width, disponible.Width), Math.Min(Height, disponible.Height));
        var izquierda = Math.Clamp(Left, area.Left + margen, area.Right - margen - tamano.Width);
        var arriba = Math.Clamp(Top, area.Top + margen, area.Bottom - margen - tamano.Height);
        Bounds = new Rectangle(new Point(izquierda, arriba), tamano);
    }

    private void AjustarMedidasGrilla()
    {
        dgvDocumentos.SuspendLayout();
        try
        {
            dgvDocumentos.RowTemplate.Height = Escalar(40);
            dgvDocumentos.ColumnHeadersHeight = Escalar(40);
            dgvDocumentos.DefaultCellStyle.Padding = new Padding(Escalar(12), 0, Escalar(6), 0);
            dgvDocumentos.ColumnHeadersDefaultCellStyle.Padding = new Padding(Escalar(12), 0, 0, 0);
            colSeleccionar.MinimumWidth = Escalar(48);
            colSeleccionar.Width = colSeleccionar.MinimumWidth;
            colTipoDocumento.MinimumWidth = Escalar(210);
            colTipoDocumento.Width = colTipoDocumento.MinimumWidth;
            colTitulo.MinimumWidth = Escalar(220);
            foreach (var columna in new[] { colVerPdf, colVerParticipantes })
            {
                columna.DefaultCellStyle.Padding = new Padding(Escalar(36), 0, Escalar(6), 0);
                var cabecera = TextRenderer.MeasureText(columna.HeaderText, dgvDocumentos.ColumnHeadersDefaultCellStyle.Font).Width + Escalar(24);
                var contenido = TextRenderer.MeasureText(columna.Text, dgvDocumentos.Font).Width + Escalar(48);
                columna.MinimumWidth = Math.Max(Escalar(140), Math.Max(cabecera, contenido));
                columna.Width = columna.MinimumWidth;
            }
            foreach (DataGridViewRow fila in dgvDocumentos.Rows) fila.Height = Escalar(40);
        }
        finally { dgvDocumentos.ResumeLayout(); }
    }

    private int Escalar(int valor) => (int)Math.Round(valor * DeviceDpi / 96F);

    private Font CrearFuente(float puntos, FontStyle estilo)
    {
        var fuente = new Font("Segoe UI", puntos, estilo);
        _recursosVisuales.Add(fuente);
        return fuente;
    }

    private Bitmap CrearIconoWindows(string glifo, int tamano, Color color)
    {
        var imagen = TemaVisual.CrearIcono(glifo, Escalar(tamano), color);
        _recursosVisuales.Add(imagen);
        return imagen;
    }

    private void ConfigurarIcono(Button boton, string glifo, Color color)
    {
        boton.Image = CrearIconoWindows(glifo, 18, color);
        boton.ImageAlign = string.IsNullOrEmpty(boton.Text) ? ContentAlignment.MiddleCenter : ContentAlignment.MiddleLeft;
        boton.TextImageRelation = TextImageRelation.ImageBeforeText;
        boton.Padding = new Padding(Escalar(8), 0, Escalar(8), 0);
    }

    private void PintarSeparador(object? sender, PaintEventArgs e)
    {
        if (sender is not Control control) return;
        using var lapiz = new Pen(TemaVisual.Borde);
        e.Graphics.DrawLine(lapiz, 0, 0, control.Width, 0);
    }

    private void PintarBordeGrilla(object? sender, PaintEventArgs e)
    {
        if (panelGrilla.ClientSize.Width < 2 || panelGrilla.ClientSize.Height < 2) return;
        using var lapiz = new Pen(TemaVisual.Borde);
        e.Graphics.DrawRectangle(lapiz, 0, 0, panelGrilla.ClientSize.Width - 1, panelGrilla.ClientSize.Height - 1);
    }

    private void ActualizarSeleccionVisual()
    {
        var cantidad = dgvDocumentos.Rows.Cast<DataGridViewRow>()
            .Count(fila => fila.DataBoundItem is DocumentoGridItem { Seleccionado: true });
        lblSeleccion.Text = $"{cantidad} seleccionados";
        lblResumenSeleccion.Text = cantidad == 0 ? "Sin documentos seleccionados"
            : cantidad == 1 ? "1 documento seleccionado" : $"{cantidad} documentos seleccionados";
        btnFirmarDocumentos.Text = cantidad == 0 ? "Firmar documentos"
            : cantidad == 1 ? "Firmar 1 documento" : $"Firmar {cantidad} documentos";
        btnFirmarDocumentos.Enabled = !_operacionEnCurso && cantidad > 0;
        btnMarcarTodos.Enabled = !_operacionEnCurso && dgvDocumentos.Rows.Count > 0;
        btnLimpiarSeleccion.Enabled = !_operacionEnCurso && cantidad > 0;
        dgvDocumentos.Invalidate();
    }

    private void ActualizarEstadoGrilla(string? mensaje = null)
    {
        lblEstadoGrilla.Text = mensaje ?? (_busquedaRealizada
            ? "No se encontraron documentos pendientes"
            : "Buscá los documentos pendientes de firma");
        var mostrarMensaje = mensaje is not null || dgvDocumentos.Rows.Count == 0;
        lblEstadoGrilla.Visible = mostrarMensaje;
        dgvDocumentos.Visible = !mostrarMensaje;
        if (mostrarMensaje) lblEstadoGrilla.BringToFront();
    }

    private void dgvDocumentos_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0 && e.ColumnIndex == colSeleccionar.Index) ActualizarSeleccionVisual();
    }

    private void dgvDocumentos_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || e.CellStyle is null) return;
        // Padding.Empty en la columna hereda el margen general; anularlo en el estilo efectivo.
        if (e.ColumnIndex == colSeleccionar.Index) e.CellStyle.Padding = Padding.Empty;
        var marcada = dgvDocumentos.Rows[e.RowIndex].DataBoundItem is DocumentoGridItem { Seleccionado: true };
        // El foco conserva su indicador nativo; el relleno representa solo el checkbox.
        e.CellStyle.BackColor = marcada ? TemaVisual.Celeste : TemaVisual.Superficie;
        e.CellStyle.SelectionBackColor = e.CellStyle.BackColor;
        e.CellStyle.SelectionForeColor = TemaVisual.Texto;
    }

    private void dgvDocumentos_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || e.Graphics is null) return;
        var icono = e.ColumnIndex == colVerPdf.Index ? _iconoPdf
            : e.ColumnIndex == colVerParticipantes.Index ? _iconoIntervinientes : null;
        if (icono is null) return;
        e.Paint(e.ClipBounds, DataGridViewPaintParts.All);
        var lado = Escalar(18);
        e.Graphics.DrawImage(icono, new Rectangle(e.CellBounds.Left + Escalar(12),
            e.CellBounds.Top + (e.CellBounds.Height - lado) / 2, lado, lado));
        e.Handled = true;
    }
}
