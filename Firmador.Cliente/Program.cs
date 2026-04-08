using Firmador.ApiClient.Documentos;
using Firmador.Cliente.Services;
using Firmador.Core.Firma;

namespace Firmador.Cliente;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        Application.Run(
            new MainForm(
                new MockDocumentosApiClient(),
                new CertificateSelectorService(),
                new WindowsPdfSigningService(),
                new SolutionPaths()));
    }
}
