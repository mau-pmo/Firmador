using System.Diagnostics;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Firmador.ApiClient.Abstractions;
using Firmador.Cliente.Services;
using Firmador.Cliente.ViewModels;
using Firmador.Core.Common;
using Firmador.Core.Documentos;
using Firmador.Core.Firma;

namespace Firmador.Cliente;

public partial class MainForm : Form
{
    private const int PageSize = 10;

    private readonly IFirmadorApiClient _documentosApiClient;
    private readonly CertificateSelectorService _certificateSelectorService;
    private readonly IFirmaPdfService _pdfSigningService;
    private readonly SolutionPaths _solutionPaths;

    private PagedResult<DocumentoResumen>? _paginaActual;
    private bool _busquedaRealizada;
    private X509Certificate2? _certificadoSeleccionado;
    private readonly Dictionary<int, (string Version, byte[] Pdf, Guid Clave)> _enviosPendientes = [];

    public bool SesionExpirada { get; private set; }
    public X509Certificate2? CertificadoSeleccionado => _certificadoSeleccionado;

    public MainForm(
        IFirmadorApiClient documentosApiClient,
        CertificateSelectorService certificateSelectorService,
        IFirmaPdfService pdfSigningService,
        SolutionPaths solutionPaths,
        X509Certificate2? certificadoSeleccionado = null)
    {
        _documentosApiClient = documentosApiClient;
        _certificateSelectorService = certificateSelectorService;
        _pdfSigningService = pdfSigningService;
        _solutionPaths = solutionPaths;
        _certificadoSeleccionado = certificadoSeleccionado;

        InitializeComponent();
        AplicarEstilos();
        ConfigurarEnlaceColumnas();
        ConfigurarBotonBuscar();
        ConfigurarBotonFirmar();
        InicializarPantalla();
    }

    private void ConfigurarEnlaceColumnas()
    {
        // Enlazar en ejecución permite mostrar todas las columnas en el diseñador sin un origen de datos.
        colSeleccionar.DataPropertyName = nameof(DocumentoGridItem.Seleccionado);
        colTipoDocumento.DataPropertyName = nameof(DocumentoGridItem.TipoDocumento);
        colTitulo.DataPropertyName = nameof(DocumentoGridItem.Titulo);
    }

    private void ConfigurarBotonBuscar()
    {
        ConfigurarIcono(btnBuscar, "\uE721", TemaVisual.Azul);
    }

    private void ConfigurarBotonFirmar()
    {
        ConfigurarIcono(btnFirmarDocumentos, "\uE70F", Color.White);
    }

    private async Task CargarPaginaAsync(int numeroPagina)
    {
        ToggleControles(false);
        ActualizarEstadoGrilla("Buscando documentos…");

        try
        {
            _paginaActual = await _documentosApiClient.ObtenerDocumentosAsync(numeroPagina, PageSize);
            _busquedaRealizada = true;
            ActualizarGrilla();
            ActualizarPaginacion();
        }
        catch (SesionExpiradaException)
        {
            VolverAlLogin();
        }
        catch (Exception ex) when (ErroresConexion.EsFallaDeConexion(ex))
        {
            MessageBox.Show(ErroresConexion.Mensaje, "Buscar", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No fue posible buscar documentos.\n\n{ex.Message}", "Buscar", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            ToggleControles(true);
            ActualizarEstadoGrilla();
        }
    }

    private void ActualizarGrilla()
    {
        var items = _paginaActual?.Items
            .Select(documento => new DocumentoGridItem
            {
                Id = documento.Id,
                TipoDocumento = documento.TipoDocumento,
                Titulo = documento.Titulo,
                Hash = documento.Hash,
                Version = documento.Version
            })
            .ToList() ?? [];

        dgvDocumentos.DataSource = null;
        dgvDocumentos.DataSource = items;
        foreach (var nombre in new[] { nameof(DocumentoGridItem.Id), nameof(DocumentoGridItem.Hash), nameof(DocumentoGridItem.Version) })
            if (dgvDocumentos.Columns[nombre] is { } columna) columna.Visible = false;
        ActualizarSeleccionVisual();
        ActualizarEstadoGrilla();
    }

    private void ActualizarPaginacion()
    {
        if (!_busquedaRealizada || _paginaActual is null)
        {
            lblPagina.Text = "Sin búsqueda";
            lblTotalDocumentos.Text = "0 documentos";
            btnPaginaAnterior.Enabled = false;
            btnPaginaSiguiente.Enabled = false;
            return;
        }

        lblPagina.Text = $"Página {_paginaActual.PageNumber} de {Math.Max(1, _paginaActual.TotalPages)}";
        var desde = _paginaActual.Items.Count == 0 ? 0 : (_paginaActual.PageNumber - 1) * PageSize + 1;
        var hasta = desde == 0 ? 0 : desde + _paginaActual.Items.Count - 1;
        lblTotalDocumentos.Text = $"Mostrando {desde}–{hasta} de {_paginaActual.TotalCount} documentos";
        btnPaginaAnterior.Enabled = _paginaActual.PageNumber > 1;
        btnPaginaSiguiente.Enabled = _paginaActual.PageNumber < _paginaActual.TotalPages;
    }

    private void InicializarPantalla()
    {
        dgvDocumentos.DataSource = new List<DocumentoGridItem>();
        ActualizarPaginacion();
        ActualizarCertificadoSeleccionado();
        ActualizarSeleccionVisual();
        ActualizarEstadoGrilla();
    }

    private void ActualizarCertificadoSeleccionado()
    {
        var nombre = _certificadoSeleccionado?.GetNameInfo(X509NameType.SimpleName, false);
        if (string.IsNullOrWhiteSpace(nombre)) nombre = _certificadoSeleccionado?.Subject;
        lblCertificadoSeleccionado.Text = nombre ?? "Sin certificado seleccionado";
        var vigente = _certificadoSeleccionado is not null && DateTime.Now >= _certificadoSeleccionado.NotBefore
            && DateTime.Now <= _certificadoSeleccionado.NotAfter;
        lblCertificadoDetalle.Text = _certificadoSeleccionado is null ? ""
            : $"{(vigente ? "Vigente hasta" : "Vencimiento:")} {_certificadoSeleccionado.NotAfter:dd/MM/yyyy}";
        lblCertificadoEstado.Text = _certificadoSeleccionado is null ? "Sin seleccionar"
            : vigente ? "✓ Certificado seleccionado" : "⚠ Fuera de vigencia";
        lblCertificadoEstado.ForeColor = vigente ? TemaVisual.Correcto : TemaVisual.Advertencia;
        btnSeleccionarCertificado.Text = _certificadoSeleccionado is null ? "Seleccionar certificado" : "Cambiar certificado";
        lblResumenCertificado.Text = _certificadoSeleccionado is null ? "Sin certificado seleccionado" : $"Certificado: {nombre}";
        toolTip.SetToolTip(lblCertificadoSeleccionado, _certificadoSeleccionado?.Subject ?? lblCertificadoSeleccionado.Text);
        toolTip.SetToolTip(lblResumenCertificado, lblResumenCertificado.Text);
    }

    private void ToggleControles(bool habilitado)
    {
        _operacionEnCurso = !habilitado;
        btnBuscar.Enabled = habilitado;
        btnSeleccionarCertificado.Enabled = habilitado;
        btnSalir.Enabled = habilitado;
        btnPaginaAnterior.Enabled = habilitado && _paginaActual is not null && _paginaActual.PageNumber > 1;
        btnPaginaSiguiente.Enabled = habilitado && _paginaActual is not null && _paginaActual.PageNumber < _paginaActual.TotalPages;
        dgvDocumentos.Enabled = habilitado;
        UseWaitCursor = !habilitado;
        ActualizarSeleccionVisual();
    }

    private List<DocumentoGridItem> ObtenerDocumentosSeleccionados()
    {
        dgvDocumentos.EndEdit();

        return dgvDocumentos.Rows
            .Cast<DataGridViewRow>()
            .Select(row => row.DataBoundItem as DocumentoGridItem)
            .Where(item => item is not null && item.Seleccionado)
            .Cast<DocumentoGridItem>()
            .ToList();
    }

    private void EstablecerSeleccionDocumentos(bool seleccionado)
    {
        dgvDocumentos.EndEdit();

        foreach (var item in dgvDocumentos.Rows
                     .Cast<DataGridViewRow>()
                     .Select(row => row.DataBoundItem as DocumentoGridItem)
                     .Where(item => item is not null))
        {
            item!.Seleccionado = seleccionado;
        }

        dgvDocumentos.Refresh();
        ActualizarSeleccionVisual();
    }

    private X509Certificate2? ObtenerOCapturarCertificado()
    {
        if (_certificadoSeleccionado is not null)
        {
            return _certificadoSeleccionado;
        }

        var certificado = _certificateSelectorService.SeleccionarCertificadoParaFirma(this);
        if (certificado is null)
        {
            return null;
        }

        _certificadoSeleccionado = certificado;
        ActualizarCertificadoSeleccionado();
        return _certificadoSeleccionado;
    }

    private void btnSeleccionarCertificado_Click(object sender, EventArgs e)
    {
        try
        {
            var certificado = ObtenerOCapturarCertificado(forzarNuevaSeleccion: true);
            if (certificado is null)
            {
                return;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"No fue posible seleccionar el certificado.\n\nDetalle: {ex.Message}",
                "Seleccionar certificado",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private X509Certificate2? ObtenerOCapturarCertificado(bool forzarNuevaSeleccion)
    {
        if (!forzarNuevaSeleccion)
        {
            return ObtenerOCapturarCertificado();
        }

        var certificado = _certificateSelectorService.SeleccionarCertificadoParaFirma(this);
        if (certificado is null)
        {
            return null;
        }

        _certificadoSeleccionado = certificado;
        ActualizarCertificadoSeleccionado();
        return _certificadoSeleccionado;
    }

    private async Task<List<string>> FirmarDocumentosSeleccionadosAsync(
        IReadOnlyCollection<DocumentoGridItem> documentosSeleccionados,
        X509Certificate2 certificado,
        CancellationToken cancellationToken = default)
    {
        var resultados = new List<string>();
        var directorioFirmados = _solutionPaths.ObtenerDirectorioFirmados();

        foreach (var documento in documentosSeleccionados)
        {
            lblResumenSeleccion.Text = $"Firmando documento {resultados.Count + 1} de {documentosSeleccionados.Count}…";
            try
            {
                var resumen = ObtenerResumen(documento.Id);
                if (!_enviosPendientes.TryGetValue(documento.Id, out var envio) || envio.Version != resumen.Version)
                {
                    var pdf = await _documentosApiClient.DescargarPdfAsync(resumen, cancellationToken);
                    string rutaPdf;
                    try
                    {
                        rutaPdf = GuardarPdfTemporal(resumen, pdf);
                    }
                    catch (IOException ex) when (EsPdfTemporalBloqueado(ex))
                    {
                        resultados.Add($"Documento {documento.Id}: No se pudo firmar. Cierre el PDF del documento y vuelva a intentar");
                        continue;
                    }
                    try
                    {
                        var firmado = await _pdfSigningService.FirmarAsync(
                            documento.Id, rutaPdf, directorioFirmados, certificado, cancellationToken);
                        envio = (resumen.Version, await File.ReadAllBytesAsync(firmado.ArchivoFirmado, cancellationToken), Guid.NewGuid());
                        _enviosPendientes[documento.Id] = envio;
                    }
                    finally { File.Delete(rutaPdf); }
                }
                await _documentosApiClient.EnviarPdfFirmadoAsync(resumen, envio.Pdf, envio.Clave, cancellationToken);
                _enviosPendientes.Remove(documento.Id);
                resultados.Add($"Documento {documento.Id}: recibido por el sistema web.");
            }
            catch (SesionExpiradaException) { throw; }
            catch (ServidorTsaNoDisponibleException) { throw; }
            catch (Exception ex) when (ErroresConexion.EsFallaDeConexion(ex)) { throw; }
            catch (HashDocumentoNoCoincideException)
            {
                resultados.Add(ConstruirMensajeDocumentoModificado(documento));
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                resultados.Add(ConstruirMensajeDocumentoModificado(documento));
            }
            catch (Exception ex) { resultados.Add($"Documento {documento.Id}: {ex.Message}"); }
        }

        return resultados;
    }

    private static string ConstruirMensajeFirmas(IReadOnlyCollection<string> resultados)
    {
        return string.Join("\n\n", resultados);
    }

    private static bool EsPdfTemporalBloqueado(IOException exception) =>
        exception.HResult == unchecked((int)0x80070020) || // ERROR_SHARING_VIOLATION
        exception.HResult == unchecked((int)0x80070021);   // ERROR_LOCK_VIOLATION

    private static string ConstruirMensajeDocumentoModificado(DocumentoGridItem documento) =>
        $"No se pudo firmar el documento {documento.Id} ya que ha sido modificado. " +
        "Por favor vuelva a buscar la lista de documentos a firmar.";

    private async void btnPaginaAnterior_Click(object sender, EventArgs e)
    {
        if (_paginaActual is null || _paginaActual.PageNumber <= 1)
        {
            return;
        }

        await CargarPaginaAsync(_paginaActual.PageNumber - 1);
    }

    private async void btnPaginaSiguiente_Click(object sender, EventArgs e)
    {
        if (_paginaActual is null || _paginaActual.PageNumber >= _paginaActual.TotalPages)
        {
            return;
        }

        await CargarPaginaAsync(_paginaActual.PageNumber + 1);
    }

    private async void btnBuscar_Click(object sender, EventArgs e)
    {
        await CargarPaginaAsync(1);
    }

    private async void btnFirmarDocumentos_Click(object sender, EventArgs e)
    {
        if (!_busquedaRealizada)
        {
            MessageBox.Show(
                "Primero debe buscar los documentos a firmar.",
                "Firmar",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        var documentosSeleccionados = ObtenerDocumentosSeleccionados();
        if (documentosSeleccionados.Count == 0)
        {
            MessageBox.Show(
                "Seleccione al menos un documento en la tabla.",
                "Firmar",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        try
        {
            var certificado = ObtenerOCapturarCertificado();
            if (certificado is null)
            {
                MessageBox.Show(
                    "No se selecciono ningun certificado.",
                    "Firmar",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            ToggleControles(false);
            var resultados = await FirmarDocumentosSeleccionadosAsync(documentosSeleccionados, certificado);

            MessageBox.Show(
                ConstruirMensajeFirmas(resultados),
                "Firmar",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            await CargarPaginaAsync(_paginaActual?.PageNumber ?? 1);
        }
        catch (ServidorTsaNoDisponibleException ex)
        {
            MessageBox.Show(ex.Message, "Firmar", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (SesionExpiradaException)
        {
            VolverAlLogin();
        }
        catch (Exception ex) when (ErroresConexion.EsFallaDeConexion(ex))
        {
            MessageBox.Show(ErroresConexion.Mensaje, "Firmar", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"No fue posible firmar los documentos.\n\nDetalle: {ex.Message}",
                "Firmar",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            ToggleControles(true);
        }
    }

    private void btnMarcarTodos_Click(object sender, EventArgs e)
    {
        EstablecerSeleccionDocumentos(true);
    }

    private void btnLimpiarSeleccion_Click(object sender, EventArgs e)
    {
        EstablecerSeleccionDocumentos(false);
    }

    private void btnSalir_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void dgvDocumentos_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
    {
        if (dgvDocumentos.IsCurrentCellDirty)
        {
            dgvDocumentos.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }
    }

    private async void dgvDocumentos_CellContentClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || (e.ColumnIndex != colVerPdf.Index && e.ColumnIndex != colVerParticipantes.Index))
        {
            return;
        }

        if (dgvDocumentos.Rows[e.RowIndex].DataBoundItem is not DocumentoGridItem documento)
        {
            return;
        }

        if (e.ColumnIndex == colVerParticipantes.Index)
        {
            using var participantes = new ParticipantesForm(_documentosApiClient, documento.Id, documento.Titulo);
            participantes.ShowDialog(this);
            if (participantes.SesionExpirada) VolverAlLogin();
            return;
        }

        try
        {
            var resumen = ObtenerResumen(documento.Id);
            var pdf = await _documentosApiClient.DescargarPdfAsync(resumen);
            var rutaPdf = GuardarPdfTemporal(resumen, pdf);

            Process.Start(new ProcessStartInfo
            {
                FileName = rutaPdf,
                UseShellExecute = true
            });
        }
        catch (SesionExpiradaException)
        {
            VolverAlLogin();
        }
        catch (Exception ex) when (ErroresConexion.EsFallaDeConexion(ex))
        {
            MessageBox.Show(ErroresConexion.Mensaje, "Ver PDF", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"No fue posible abrir el PDF.\n\nDetalle: {ex.Message}",
                "Ver PDF",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private DocumentoResumen ObtenerResumen(int id) =>
        _paginaActual?.Items.FirstOrDefault(item => item.Id == id)
        ?? throw new InvalidOperationException("El documento ya no está en la página actual. Busque nuevamente.");

    private static string GuardarPdfTemporal(DocumentoResumen documento, byte[] pdf)
    {
        var carpeta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Firmador", "temporales");
        Directory.CreateDirectory(carpeta);
        var ruta = Path.Combine(carpeta, $"{documento.Id}-{documento.Hash}.pdf");
        File.WriteAllBytes(ruta, pdf);
        return ruta;
    }

    private void VolverAlLogin()
    {
        if (SesionExpirada)
        {
            return;
        }

        SesionExpirada = true;
        MessageBox.Show(
            "La sesión venció. Inicie sesión nuevamente.",
            "Sesión vencida",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
        Close();
    }

}
