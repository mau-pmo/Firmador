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
        Application.ThreadException += (_, e) => MostrarErrorNoControlado(e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception exception)
            {
                MostrarErrorNoControlado(exception);
            }
        };

        try
        {
            Application.Run(
                new MainForm(
                    new MockDocumentosApiClient(),
                    new CertificateSelectorService(),
                    new WindowsPdfSigningService(),
                    new SolutionPaths()));
        }
        catch (Exception ex)
        {
            MostrarErrorNoControlado(ex);
        }
    }

    private static void MostrarErrorNoControlado(Exception exception)
    {
        MessageBox.Show(
            $"La aplicacion no pudo continuar.\n\nDetalle: {exception.Message}",
            "Firmador",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }
}
