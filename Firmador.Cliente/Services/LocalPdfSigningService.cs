using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace Firmador.Cliente.Services;

public sealed class LocalPdfSigningService
{
    public async Task<FirmaDocumentoResultado> FirmarAsync(
        int documentoId,
        string archivoEntradaPdf,
        string directorioSalida,
        X509Certificate2 certificado,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(archivoEntradaPdf))
        {
            throw new FileNotFoundException("No se encontro el PDF a firmar.", archivoEntradaPdf);
        }

        if (!certificado.HasPrivateKey)
        {
            throw new InvalidOperationException("El certificado seleccionado no tiene clave privada disponible para firmar.");
        }

        Directory.CreateDirectory(directorioSalida);

        var nombreBase = $"{Path.GetFileNameWithoutExtension(archivoEntradaPdf)}_firmado";
        var archivoFirmado = Path.Combine(directorioSalida, $"{nombreBase}.pdf");
        var archivoFirmaCms = Path.Combine(directorioSalida, $"{nombreBase}.p7s");

        var contenidoPdf = await File.ReadAllBytesAsync(archivoEntradaPdf, cancellationToken);

        var cms = new SignedCms(new ContentInfo(contenidoPdf), detached: true);
        var firmante = new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, certificado)
        {
            DigestAlgorithm = new Oid("2.16.840.1.101.3.4.2.1"),
            IncludeOption = X509IncludeOption.EndCertOnly
        };

        cms.ComputeSignature(firmante);
        var firmaCms = cms.Encode();

        File.Copy(archivoEntradaPdf, archivoFirmado, overwrite: true);
        await File.WriteAllBytesAsync(archivoFirmaCms, firmaCms, cancellationToken);

        return new FirmaDocumentoResultado
        {
            DocumentoId = documentoId,
            ArchivoOriginal = archivoEntradaPdf,
            ArchivoFirmado = archivoFirmado,
            ArchivoFirmaCms = archivoFirmaCms
        };
    }
}
