using Firmador.ApiClient.Documentos;
using System.Text.Json;
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

            using var api = new FirmadorApiClient(url, permitirHttpDePrueba);
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
            Application.Run(new MainForm(api, new CertificateSelectorService(), new WindowsPdfSigningService(), new SolutionPaths()));
            try { api.CerrarSesionAsync().GetAwaiter().GetResult(); }
            catch { /* La sesión local se descarta aunque falle la revocación remota. */ }
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
