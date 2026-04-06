using System.Security.Cryptography.X509Certificates;

namespace Firmador.Cliente.Services;

public sealed class CertificateSelectorService
{
    public X509Certificate2? SeleccionarCertificadoParaFirma(IWin32Window owner)
    {
        using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

        var candidatos = new X509Certificate2Collection();

        foreach (var certificado in store.Certificates.Cast<X509Certificate2>())
        {
            if (!certificado.HasPrivateKey)
            {
                continue;
            }

            if (DateTime.Now < certificado.NotBefore || DateTime.Now > certificado.NotAfter)
            {
                continue;
            }

            candidatos.Add(certificado);
        }

        if (candidatos.Count == 0)
        {
            throw new InvalidOperationException("No se encontraron certificados vigentes con clave privada en el almacen personal del usuario.");
        }

        var seleccion = X509Certificate2UI.SelectFromCollection(
            candidatos,
            "Seleccionar certificado",
            "Seleccione el certificado con el que desea firmar los documentos.",
            X509SelectionFlag.SingleSelection);

        return seleccion.Count > 0 ? seleccion[0] : null;
    }
}
