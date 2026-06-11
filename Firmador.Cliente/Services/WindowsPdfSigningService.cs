using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Firmador.Core.Firma;
using iText.Kernel.Pdf;
using iText.Signatures;
using Org.BouncyCastle.X509;
using IOFileStream = System.IO.FileStream;
using IOPath = System.IO.Path;
using Rectangle = iText.Kernel.Geom.Rectangle;

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

        var nombreBase = $"{IOPath.GetFileNameWithoutExtension(archivoEntradaPdf)}_firmado";
        var archivoFirmado = IOPath.Combine(directorioSalida, $"{nombreBase}.pdf");
        await Task.Yield();

        using var reader = new PdfReader(archivoEntradaPdf);
        using var output = new IOFileStream(archivoFirmado, FileMode.Create, FileAccess.Write, FileShare.None);

        var signer = new PdfSigner(reader, output, new StampingProperties().UseAppendMode());
        ConfigurarFirma(signer, certificado);

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

    private static void ConfigurarFirma(PdfSigner signer, X509Certificate2 certificado)
    {
        var nombreCertificado = certificado.GetNameInfo(X509NameType.SimpleName, false);

        signer.SetFieldName($"Signature_{DateTime.UtcNow:yyyyMMddHHmmssfff}");
        signer.GetSignatureAppearance()
            .SetReason("Firma digital del documento")
            .SetLocation(Environment.MachineName)
            .SetContact(string.IsNullOrWhiteSpace(nombreCertificado) ? certificado.Subject : nombreCertificado)
            .SetSignatureCreator(Application.ProductName ?? "Firmador")
            .SetPageRect(new Rectangle(0, 0, 0, 0))
            .SetPageNumber(1);
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
}
