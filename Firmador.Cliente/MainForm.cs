using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using Firmador.ApiClient.Abstractions;
using Firmador.Cliente.Services;
using Firmador.Cliente.ViewModels;
using Firmador.Core.Common;
using Firmador.Core.Documentos;

namespace Firmador.Cliente;

public partial class MainForm : Form
{
    private const int PageSize = 10;

    private readonly IDocumentosApiClient _documentosApiClient;
    private readonly CertificateSelectorService _certificateSelectorService;
    private readonly LocalPdfSigningService _pdfSigningService;
    private readonly SolutionPaths _solutionPaths;

    private PagedResult<DocumentoResumen>? _paginaActual;
    private bool _busquedaRealizada;
    private X509Certificate2? _certificadoSeleccionado;

    public MainForm(
        IDocumentosApiClient documentosApiClient,
        CertificateSelectorService certificateSelectorService,
        LocalPdfSigningService pdfSigningService,
        SolutionPaths solutionPaths)
    {
        _documentosApiClient = documentosApiClient;
        _certificateSelectorService = certificateSelectorService;
        _pdfSigningService = pdfSigningService;
        _solutionPaths = solutionPaths;

        InitializeComponent();
        InicializarPantalla();
    }

    private async Task CargarPaginaAsync(int numeroPagina)
    {
        ToggleControles(false);

        try
        {
            _paginaActual = await _documentosApiClient.ObtenerDocumentosAsync(numeroPagina, PageSize);
            _busquedaRealizada = true;
            ActualizarGrilla();
            ActualizarPaginacion();
        }
        finally
        {
            ToggleControles(true);
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
                Hash = documento.Hash
            })
            .ToList() ?? [];

        dgvDocumentos.DataSource = null;
        dgvDocumentos.DataSource = items;
    }

    private void ActualizarPaginacion()
    {
        if (!_busquedaRealizada || _paginaActual is null)
        {
            lblPagina.Text = "Sin busqueda";
            btnPaginaAnterior.Enabled = false;
            btnPaginaSiguiente.Enabled = false;
            return;
        }

        lblPagina.Text = $"Pagina {_paginaActual.PageNumber} de {_paginaActual.TotalPages}";
        btnPaginaAnterior.Enabled = _paginaActual.PageNumber > 1;
        btnPaginaSiguiente.Enabled = _paginaActual.PageNumber < _paginaActual.TotalPages;
    }

    private void InicializarPantalla()
    {
        dgvDocumentos.DataSource = new List<DocumentoGridItem>();
        ActualizarPaginacion();
        ActualizarCertificadoSeleccionado();
    }

    private void ActualizarCertificadoSeleccionado()
    {
        lblCertificadoSeleccionado.Text = _certificadoSeleccionado is null
            ? "No seleccionado"
            : $"{_certificadoSeleccionado.GetNameInfo(X509NameType.SimpleName, false)} | {_certificadoSeleccionado.Thumbprint}";
    }

    private void ToggleControles(bool habilitado)
    {
        btnBuscar.Enabled = habilitado;
        btnFirmarDocumentos.Enabled = habilitado;
        btnSalir.Enabled = habilitado;
        btnPaginaAnterior.Enabled = habilitado && _paginaActual is not null && _paginaActual.PageNumber > 1;
        btnPaginaSiguiente.Enabled = habilitado && _paginaActual is not null && _paginaActual.PageNumber < _paginaActual.TotalPages;
        dgvDocumentos.Enabled = habilitado;
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

    private async Task<List<FirmaDocumentoResultado>> FirmarDocumentosSeleccionadosAsync(
        IReadOnlyCollection<DocumentoGridItem> documentosSeleccionados,
        X509Certificate2 certificado,
        CancellationToken cancellationToken = default)
    {
        var resultados = new List<FirmaDocumentoResultado>();
        var directorioFirmados = _solutionPaths.ObtenerDirectorioFirmados();

        foreach (var documento in documentosSeleccionados)
        {
            var rutaPdf = _solutionPaths.ObtenerRutaDocumentoPdf(documento.Id);
            var resultado = await _pdfSigningService.FirmarAsync(
                documento.Id,
                rutaPdf,
                directorioFirmados,
                certificado,
                cancellationToken);

            resultados.Add(resultado);
        }

        return resultados;
    }

    private static string ConstruirMensajeFirmas(IReadOnlyCollection<FirmaDocumentoResultado> resultados)
    {
        var lineas = resultados
            .Select(resultado =>
                $"Documento {resultado.DocumentoId}: {Path.GetFileName(resultado.ArchivoFirmado)} y {Path.GetFileName(resultado.ArchivoFirmaCms)}")
            .ToList();

        return "Documentos firmados correctamente.\n\n" + string.Join('\n', lineas);
    }

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

    private void dgvDocumentos_CellContentClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex != colVerPdf.Index)
        {
            return;
        }

        if (dgvDocumentos.Rows[e.RowIndex].DataBoundItem is not DocumentoGridItem documento)
        {
            return;
        }

        try
        {
            var rutaPdf = _solutionPaths.ObtenerRutaDocumentoPdf(documento.Id);
            if (!File.Exists(rutaPdf))
            {
                MessageBox.Show(
                    $"No se encontro el archivo {documento.Id}.pdf en la carpeta docs.",
                    "Ver PDF",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = rutaPdf,
                UseShellExecute = true
            });
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
}
