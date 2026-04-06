using Firmador.ApiClient.Documentos;
using Firmador.Cliente.Services;

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
                new LocalPdfSigningService(),
                new SolutionPaths()));
    }
}
