using System.Security.Cryptography.X509Certificates;

namespace Firmador.Core.Firma;

public interface IFirmaPdfService
{
    Task<FirmaDocumentoResultado> FirmarAsync(
        int documentoId,
        string archivoEntradaPdf,
        string directorioSalida,
        X509Certificate2 certificado,
        CancellationToken cancellationToken = default);
}
