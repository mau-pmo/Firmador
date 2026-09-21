using Firmador.ApiClient.Documentos;
using System.Text.Json;
using System.Security.Cryptography.X509Certificates;
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
            var url = Environment.GetEnvironmentVariable("FIRMADOR_API_BASE_URL");
            var tsaUrl = Environment.GetEnvironmentVariable("TSA_URL");
            var configuracion = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            using var json = JsonDocument.Parse(File.ReadAllText(configuracion));
            var permitirHttpDePrueba = json.RootElement.TryGetProperty("AllowInsecureHttp", out var opcionHttp)
                && opcionHttp.ValueKind == JsonValueKind.True;
            if (string.IsNullOrWhiteSpace(url))
            {
                url = json.RootElement.GetProperty("ApiBaseUrl").GetString();
            }
            if (string.IsNullOrWhiteSpace(url))
                throw new InvalidOperationException("Configure ApiBaseUrl en appsettings.json o FIRMADOR_API_BASE_URL antes de iniciar.");
            if (string.IsNullOrWhiteSpace(tsaUrl))
            {
                tsaUrl = json.RootElement.TryGetProperty("TsaUrl", out var opcionTsa)
                    ? opcionTsa.GetString()
                    : null;
            }
            if (string.IsNullOrWhiteSpace(tsaUrl))
                throw new InvalidOperationException("Configure TsaUrl en appsettings.json o TSA_URL antes de iniciar.");

            using var api = new FirmadorApiClient(url, permitirHttpDePrueba);
            X509Certificate2? certificadoSeleccionado = null;
            while (true)
            {
                while (true)
                {
                    using var login = new LoginForm();
                    if (login.ShowDialog() != DialogResult.OK) return;
                    try
                    {
                        api.IniciarSesionAsync(login.Usuario, login.Contrasena).GetAwaiter().GetResult();
                        break;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Inicio de sesión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                using var mainForm = new MainForm(
                    api,
                    new CertificateSelectorService(),
                    new WindowsPdfSigningService(tsaUrl),
                    new SolutionPaths(),
                    certificadoSeleccionado);
                Application.Run(mainForm);
                certificadoSeleccionado = mainForm.CertificadoSeleccionado;

                if (mainForm.SesionExpirada)
                {
                    continue;
                }

                try { api.CerrarSesionAsync().GetAwaiter().GetResult(); }
                catch { /* La sesión local se descarta aunque falle la revocación remota. */ }
                return;
            }
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
