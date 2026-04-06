using Firmador.ApiClient.Abstractions;
using Firmador.Core.Common;
using Firmador.Core.Documentos;

namespace Firmador.ApiClient.Documentos;

public sealed class MockDocumentosApiClient : IDocumentosApiClient
{
    private static readonly IReadOnlyList<DocumentoResumen> Documentos =
    [
        new() { Id = 1, TipoDocumento = "Contrato", Titulo = "Contrato Marco 2026", Hash = "HASH-0001-A1B2C3D4" },
        new() { Id = 2, TipoDocumento = "Factura", Titulo = "Factura Proveedor 0001-00001234", Hash = "HASH-0002-B2C3D4E5" },
        new() { Id = 3, TipoDocumento = "Recibo", Titulo = "Recibo Honorarios Marzo", Hash = "HASH-0003-C3D4E5F6" },
        new() { Id = 4, TipoDocumento = "Informe", Titulo = "Informe Financiero Q1", Hash = "HASH-0004-D4E5F6G7" },
        new() { Id = 5, TipoDocumento = "Convenio", Titulo = "Convenio de Confidencialidad", Hash = "HASH-0005-E5F6G7H8" },
        new() { Id = 6, TipoDocumento = "Orden de compra", Titulo = "OC Equipamiento Oficina", Hash = "HASH-0006-F6G7H8I9" },
        new() { Id = 7, TipoDocumento = "Contrato", Titulo = "Contrato de Servicios", Hash = "HASH-0007-G7H8I9J0" },
        new() { Id = 8, TipoDocumento = "Factura", Titulo = "Factura Cliente ABC", Hash = "HASH-0008-H8I9J0K1" },
        new() { Id = 9, TipoDocumento = "Acta", Titulo = "Acta de Reunion Comercial", Hash = "HASH-0009-I9J0K1L2" },
        new() { Id = 10, TipoDocumento = "Certificado", Titulo = "Certificado de Entrega", Hash = "HASH-0010-J0K1L2M3" },
        new() { Id = 11, TipoDocumento = "Informe", Titulo = "Informe Tecnico", Hash = "HASH-0011-K1L2M3N4" },
        new() { Id = 12, TipoDocumento = "Contrato", Titulo = "Addenda Contrato Principal", Hash = "HASH-0012-L2M3N4O5" }
    ];

    public async Task<PagedResult<DocumentoResumen>> ObtenerDocumentosAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        await Task.Delay(250, cancellationToken);

        var paginaNormalizada = Math.Max(1, pageNumber);
        var tamanoNormalizado = Math.Max(1, pageSize);

        var items = Documentos
            .Skip((paginaNormalizada - 1) * tamanoNormalizado)
            .Take(tamanoNormalizado)
            .ToList();

        return new PagedResult<DocumentoResumen>
        {
            Items = items,
            PageNumber = paginaNormalizada,
            PageSize = tamanoNormalizado,
            TotalCount = Documentos.Count
        };
    }
}
