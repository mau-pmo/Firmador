using Firmador.Core.Documentos;

namespace Firmador.ApiClient.Abstractions;

public interface IFirmadorApiClient : IDocumentosApiClient
{
    Task<DocumentoParticipantes> ObtenerParticipantesAsync(int documentoId, CancellationToken cancellationToken = default);
    Task<string> IniciarSesionAsync(string usuario, string contrasena, CancellationToken cancellationToken = default);
    Task<byte[]> DescargarPdfAsync(DocumentoResumen documento, CancellationToken cancellationToken = default);
    Task EnviarPdfFirmadoAsync(DocumentoResumen documento, byte[] pdfFirmado, Guid claveIdempotencia, CancellationToken cancellationToken = default);
    Task CerrarSesionAsync(CancellationToken cancellationToken = default);
}
