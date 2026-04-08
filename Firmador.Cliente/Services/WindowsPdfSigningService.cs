using System.Security.Cryptography.X509Certificates;
using Firmador.Core.Firma;
using PdfSharp.Drawing;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Signatures;

namespace Firmador.Cliente.Services;

public sealed class WindowsPdfSigningService : IFirmaPdfService
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

        cancellationToken.ThrowIfCancellationRequested();
        Directory.CreateDirectory(directorioSalida);

        var nombreBase = $"{Path.GetFileNameWithoutExtension(archivoEntradaPdf)}_firmado";
        var archivoFirmado = Path.Combine(directorioSalida, $"{nombreBase}.pdf");
        await Task.Yield();

        using var document = PdfReader.Open(archivoEntradaPdf, PdfDocumentOpenMode.Modify);
        var options = CrearOpcionesFirma(certificado);
        var signer = new PdfSharpDefaultSigner(certificado, PdfMessageDigestType.SHA256);

        DigitalSignatureHandler.ForDocument(document, signer, options);
        document.Save(archivoFirmado);

        return new FirmaDocumentoResultado
        {
            DocumentoId = documentoId,
            ArchivoOriginal = archivoEntradaPdf,
            ArchivoFirmado = archivoFirmado
        };
    }

    private static DigitalSignatureOptions CrearOpcionesFirma(X509Certificate2 certificado)
    {
        return new DigitalSignatureOptions
        {
            AppName = Application.ProductName ?? "Firmador",
            ContactInfo = certificado.GetNameInfo(X509NameType.SimpleName, false) ?? certificado.Subject,
            Location = Environment.MachineName,
            Reason = "Firma digital del documento",
            PageIndex = 0,
            Rectangle = new XRect(0, 0, 0, 0)
        };
    }
}
