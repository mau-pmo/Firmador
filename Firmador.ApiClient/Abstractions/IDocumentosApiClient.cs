using Firmador.Core.Common;
using Firmador.Core.Documentos;

namespace Firmador.ApiClient.Abstractions;

public interface IDocumentosApiClient
{
    Task<PagedResult<DocumentoResumen>> ObtenerDocumentosAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}
