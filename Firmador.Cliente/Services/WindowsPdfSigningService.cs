using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Firmador.Core.Firma;
using iText.Forms;
using iText.Forms.Fields;
using iText.Kernel.Pdf;
using iText.Signatures;
using Org.BouncyCastle.X509;
using IOFileStream = System.IO.FileStream;
using IOPath = System.IO.Path;
using PdfRectangle = iText.Kernel.Geom.Rectangle;

namespace Firmador.Cliente.Services;

public sealed class WindowsPdfSigningService : IFirmaPdfService
{
    private const float SignatureWidth = 180;
    private const float SignatureHeight = 56;
    private const float SignatureMargin = 36;
    private const float SignatureGap = 8;

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

        var nombreBase = $"{IOPath.GetFileNameWithoutExtension(archivoEntradaPdf)}_firmado";
        var archivoFirmado = IOPath.Combine(directorioSalida, $"{nombreBase}.pdf");
        await Task.Yield();

        var ubicacionFirma = ObtenerPrimeraUbicacionLibreDesdeUltimaPagina(archivoEntradaPdf);

        using var reader = new PdfReader(archivoEntradaPdf);
        using var output = new IOFileStream(archivoFirmado, FileMode.Create, FileAccess.Write, FileShare.None);

        var signer = new PdfSigner(reader, output, new StampingProperties().UseAppendMode());
        ConfigurarFirma(signer, certificado, ubicacionFirma);

        var firma = new WindowsCertificateSignature(certificado);
        var cadena = CrearCadenaCertificados(certificado);

        signer.SignDetached(
            firma,
            cadena,
            crlList: null,
            ocspClient: null,
            tsaClient: null,
            estimatedSize: 0,
            sigtype: PdfSigner.CryptoStandard.CADES);

        return new FirmaDocumentoResultado
        {
            DocumentoId = documentoId,
            ArchivoOriginal = archivoEntradaPdf,
            ArchivoFirmado = archivoFirmado
        };
    }

    private static void ConfigurarFirma(PdfSigner signer, X509Certificate2 certificado, SignaturePlacement ubicacionFirma)
    {
        var nombreCertificado = certificado.GetNameInfo(X509NameType.SimpleName, false);
        var contacto = string.IsNullOrWhiteSpace(nombreCertificado) ? certificado.Subject : nombreCertificado;

        signer.SetFieldName($"Signature_{DateTime.UtcNow:yyyyMMddHHmmssfff}");
        signer.GetSignatureAppearance()
            .SetReason("Firma digital del documento")
            .SetLocation(Environment.MachineName)
            .SetContact(contacto)
            .SetSignatureCreator(Application.ProductName ?? "Firmador")
            .SetLayer2Text($"Firmado digitalmente por {contacto}\nFecha: {DateTime.Now:dd/MM/yyyy HH:mm}")
            .SetPageRect(ubicacionFirma.Rectangle)
            .SetPageNumber(ubicacionFirma.PageNumber);
    }

    private static SignaturePlacement ObtenerPrimeraUbicacionLibreDesdeUltimaPagina(string archivoEntradaPdf)
    {
        using var pdf = new PdfDocument(new PdfReader(archivoEntradaPdf));
        for (var pageNumber = pdf.GetNumberOfPages(); pageNumber >= 1; pageNumber--)
        {
            var page = pdf.GetPage(pageNumber);
            var pageSize = page.GetPageSize();
            var ubicacionesOcupadas = ObtenerUbicacionesFirmasExistentes(pdf, pageNumber);

            foreach (var ubicacion in CrearUbicacionesCandidatas(pageSize))
            {
                if (!ubicacionesOcupadas.Any(ocupada => RectangulosSeSuperponen(ubicacion, ocupada)))
                {
                    return new SignaturePlacement(pageNumber, ubicacion);
                }
            }
        }

        throw new InvalidOperationException("No se encontro un espacio libre para ubicar la firma visible en el PDF.");
    }

    private static IReadOnlyCollection<PdfRectangle> ObtenerUbicacionesFirmasExistentes(PdfDocument pdf, int pageNumber)
    {
        var acroForm = PdfAcroForm.GetAcroForm(pdf, false);
        if (acroForm is null)
        {
            return [];
        }

        var ocupadas = new List<PdfRectangle>();
        foreach (var field in acroForm.GetFormFields().Values.OfType<PdfSignatureFormField>())
        {
            foreach (var widget in field.GetWidgets())
            {
                var widgetPageNumber = pdf.GetPageNumber(widget.GetPage());
                if (widgetPageNumber != pageNumber)
                {
                    continue;
                }

                var rectangle = widget.GetRectangle()?.ToRectangle();
                if (rectangle is not null && rectangle.GetWidth() > 0 && rectangle.GetHeight() > 0)
                {
                    ocupadas.Add(rectangle);
                }
            }
        }

        return ocupadas;
    }

    private static IEnumerable<PdfRectangle> CrearUbicacionesCandidatas(PdfRectangle pageSize)
    {
        var columns = Math.Max(
            1,
            (int)MathF.Floor((pageSize.GetWidth() - (SignatureMargin * 2) + SignatureGap) / (SignatureWidth + SignatureGap)));
        var rows = Math.Max(
            1,
            (int)MathF.Floor((pageSize.GetHeight() - (SignatureMargin * 2) + SignatureGap) / (SignatureHeight + SignatureGap)));

        for (var row = 0; row < rows; row++)
        {
            var y = SignatureMargin + row * (SignatureHeight + SignatureGap);
            for (var column = 0; column < columns; column++)
            {
                var x = SignatureMargin + column * (SignatureWidth + SignatureGap);
                yield return new PdfRectangle(x, y, SignatureWidth, SignatureHeight);
            }
        }
    }

    private static bool RectangulosSeSuperponen(PdfRectangle a, PdfRectangle b)
    {
        return a.GetX() < b.GetX() + b.GetWidth()
            && a.GetX() + a.GetWidth() > b.GetX()
            && a.GetY() < b.GetY() + b.GetHeight()
            && a.GetY() + a.GetHeight() > b.GetY();
    }

    private static Org.BouncyCastle.X509.X509Certificate[] CrearCadenaCertificados(X509Certificate2 certificado)
    {
        using var cadenaWindows = new X509Chain();
        cadenaWindows.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        cadenaWindows.Build(certificado);

        var parser = new X509CertificateParser();
        var elementos = cadenaWindows.ChainElements
            .Cast<X509ChainElement>()
            .Select(elemento => parser.ReadCertificate(elemento.Certificate.RawData))
            .ToArray();

        return elementos.Length > 0
            ? elementos
            : [parser.ReadCertificate(certificado.RawData)];
    }

    private sealed class WindowsCertificateSignature : IExternalSignature
    {
        private readonly X509Certificate2 _certificado;

        public WindowsCertificateSignature(X509Certificate2 certificado)
        {
            _certificado = certificado;
        }

        public string GetHashAlgorithm() => DigestAlgorithms.SHA256;

        public string GetEncryptionAlgorithm()
        {
            if (_certificado.GetRSAPrivateKey() is not null)
            {
                return "RSA";
            }

            if (_certificado.GetECDsaPrivateKey() is not null)
            {
                return "ECDSA";
            }

            throw new InvalidOperationException("El certificado seleccionado no usa una clave privada RSA o ECDSA compatible.");
        }

        public byte[] Sign(byte[] message)
        {
            using var rsa = _certificado.GetRSAPrivateKey();
            if (rsa is not null)
            {
                return rsa.SignData(message, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }

            using var ecdsa = _certificado.GetECDsaPrivateKey();
            if (ecdsa is not null)
            {
                return ecdsa.SignData(message, HashAlgorithmName.SHA256);
            }

            throw new InvalidOperationException("El certificado seleccionado no tiene una clave privada compatible para firmar.");
        }
    }

    private sealed record SignaturePlacement(int PageNumber, PdfRectangle Rectangle);
}
