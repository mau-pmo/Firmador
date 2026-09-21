using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Firmador.ApiClient.Abstractions;
using Firmador.Core.Common;
using Firmador.Core.Documentos;

namespace Firmador.ApiClient.Documentos;

public sealed class FirmadorApiClient : IFirmadorApiClient, IDisposable
{
    private readonly HttpClient _http;
    private readonly SemaphoreSlim _renovacion = new(1, 1);
    private string? _accessToken;
    private string? _refreshToken;
    private DateTimeOffset _accessExpiresAt;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public FirmadorApiClient(string urlBase, bool permitirHttpDePrueba = false)
    {
        if (!Uri.TryCreate(urlBase.TrimEnd('/') + "/", UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttps && !(uri.Scheme == Uri.UriSchemeHttp && (uri.IsLoopback || permitirHttpDePrueba))))
        {
            throw new ArgumentException("La URL de la API debe ser HTTPS; para HTTP de prueba configure AllowInsecureHttp=true.", nameof(urlBase));
        }

        _http = new HttpClient { BaseAddress = uri, Timeout = TimeSpan.FromMinutes(2) };
    }

    public async Task<string> IniciarSesionAsync(string usuario, string contrasena, CancellationToken cancellationToken = default)
    {
        using var respuesta = await _http.PostAsJsonAsync("api/v1/sessions", new { username = usuario, password = contrasena }, cancellationToken);
        await VerificarRespuestaAsync(respuesta, cancellationToken);
        var sesion = await respuesta.Content.ReadFromJsonAsync<SessionResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidDataException("La API devolvió una sesión vacía.");
        GuardarTokens(sesion);
        return sesion.User?.DisplayName ?? usuario;
    }

    public async Task<PagedResult<DocumentoResumen>> ObtenerDocumentosAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        using var respuesta = await EnviarAutorizadoAsync(() => new HttpRequestMessage(HttpMethod.Get,
            $"api/v1/documents?status=pending-signature&pageNumber={pageNumber}&pageSize={pageSize}"), cancellationToken);
        var pagina = await respuesta.Content.ReadFromJsonAsync<DocumentPage>(JsonOptions, cancellationToken)
            ?? throw new InvalidDataException("La API devolvió un listado vacío.");
        return new PagedResult<DocumentoResumen>
        {
            Items = (pagina.Items ?? []).Select(item => new DocumentoResumen
            {
                Id = item.Id, TipoDocumento = item.Type, Titulo = item.Title,
                Version = item.Version, Hash = item.Sha256
            }).ToArray(),
            PageNumber = pagina.PageNumber, PageSize = pagina.PageSize, TotalCount = pagina.TotalCount
        };
    }

    public async Task<byte[]> DescargarPdfAsync(DocumentoResumen documento, CancellationToken cancellationToken = default)
    {
        using var respuesta = await EnviarAutorizadoAsync(() =>
        {
            var solicitud = new HttpRequestMessage(HttpMethod.Get, $"api/v1/documents/{documento.Id}/pdf");
            solicitud.Headers.TryAddWithoutValidation("If-Match", $"\"{documento.Version}\"");
            return solicitud;
        }, cancellationToken);
        var bytes = await respuesta.Content.ReadAsByteArrayAsync(cancellationToken);
        var hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        if (!hash.Equals(documento.Hash, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"El PDF del documento {documento.Id} no coincide con el SHA-256 informado por la API.");
        return bytes;
    }

    public async Task EnviarPdfFirmadoAsync(DocumentoResumen documento, byte[] pdfFirmado, Guid claveIdempotencia, CancellationToken cancellationToken = default)
    {
        var hashFirmado = Convert.ToHexString(SHA256.HashData(pdfFirmado)).ToLowerInvariant();
        using var respuesta = await EnviarAutorizadoAsync(() =>
        {
            var solicitud = new HttpRequestMessage(HttpMethod.Post, $"api/v1/documents/{documento.Id}/signed-pdf");
            solicitud.Headers.TryAddWithoutValidation("Idempotency-Key", claveIdempotencia.ToString());
            var contenido = new MultipartFormDataContent();
            var archivo = new ByteArrayContent(pdfFirmado);
            archivo.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            contenido.Add(archivo, "signedFile", $"documento-{documento.Id}-firmado.pdf");
            contenido.Add(new StringContent(documento.Version), "sourceDocumentVersion");
            contenido.Add(new StringContent(documento.Hash), "sourceDocumentSha256");
            contenido.Add(new StringContent(hashFirmado), "signedFileSha256");
            solicitud.Content = contenido;
            return solicitud;
        }, cancellationToken);
        var resultado = await respuesta.Content.ReadFromJsonAsync<UploadResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidDataException("La API devolvió una confirmación vacía.");
        if (resultado.DocumentId != documento.Id || resultado.Status != "received" ||
            !hashFirmado.Equals(resultado.SignedFileSha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("La confirmación de recepción no coincide con el PDF firmado.");
    }

    public async Task CerrarSesionAsync(CancellationToken cancellationToken = default)
    {
        var refresh = _refreshToken;
        try
        {
            if (refresh is not null && _accessToken is not null)
            {
                using var solicitud = new HttpRequestMessage(HttpMethod.Post, "api/v1/sessions/logout")
                { Content = JsonContent.Create(new { refreshToken = refresh }) };
                solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                using var respuesta = await _http.SendAsync(solicitud, cancellationToken);
            }
        }
        finally { _accessToken = null; _refreshToken = null; }
    }

    private async Task<HttpResponseMessage> EnviarAutorizadoAsync(Func<HttpRequestMessage> crearSolicitud, CancellationToken cancellationToken)
    {
        await RenovarSiNecesarioAsync(false, cancellationToken);
        var tokenUsado = _accessToken;
        using var solicitud = crearSolicitud();
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenUsado);
        var respuesta = await _http.SendAsync(solicitud, cancellationToken);
        if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
        {
            respuesta.Dispose();
            await RenovarSiNecesarioAsync(true, cancellationToken, tokenUsado);
            using var reintento = crearSolicitud();
            reintento.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            respuesta = await _http.SendAsync(reintento, cancellationToken);
        }
        try { await VerificarRespuestaAsync(respuesta, cancellationToken); return respuesta; }
        catch { respuesta.Dispose(); throw; }
    }

    private async Task RenovarSiNecesarioAsync(bool forzar, CancellationToken cancellationToken, string? tokenAnterior = null)
    {
        await _renovacion.WaitAsync(cancellationToken);
        try
        {
            if (_refreshToken is null) throw new SesionExpiradaException();
            if (forzar && tokenAnterior != _accessToken) return;
            if (!forzar && DateTimeOffset.UtcNow < _accessExpiresAt.AddSeconds(-30)) return;
            using var respuesta = await _http.PostAsJsonAsync("api/v1/sessions/refresh", new { refreshToken = _refreshToken }, cancellationToken);
            try
            {
                await VerificarRespuestaAsync(respuesta, cancellationToken);
                var sesion = await respuesta.Content.ReadFromJsonAsync<SessionResponse>(JsonOptions, cancellationToken)
                    ?? throw new InvalidDataException("La API devolvió una renovación vacía.");
                GuardarTokens(sesion);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                _accessToken = null;
                _refreshToken = null;
                throw new SesionExpiradaException(ex);
            }
            catch { _accessToken = null; _refreshToken = null; throw; }
        }
        finally { _renovacion.Release(); }
    }

    private void GuardarTokens(SessionResponse sesion)
    {
        if (string.IsNullOrWhiteSpace(sesion.AccessToken) || string.IsNullOrWhiteSpace(sesion.RefreshToken))
            throw new InvalidDataException("La API no devolvió los tokens de sesión.");
        _accessToken = sesion.AccessToken;
        _refreshToken = sesion.RefreshToken;
        _accessExpiresAt = DateTimeOffset.UtcNow.AddSeconds(sesion.ExpiresIn);
    }

    private static async Task VerificarRespuestaAsync(HttpResponseMessage respuesta, CancellationToken cancellationToken)
    {
        if (respuesta.IsSuccessStatusCode) return;
        string? detalle = null;
        try
        {
            using var json = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync(cancellationToken));
            if (json.RootElement.TryGetProperty("detail", out var campo)) detalle = campo.GetString();
        }
        catch (JsonException) { }
        throw new HttpRequestException($"API HTTP {(int)respuesta.StatusCode}: {detalle ?? respuesta.ReasonPhrase}", null, respuesta.StatusCode);
    }

    public void Dispose() { _http.Dispose(); _renovacion.Dispose(); }

    private sealed record SessionResponse(string AccessToken, string RefreshToken, int ExpiresIn, UserResponse? User);
    private sealed record UserResponse(string DisplayName);
    private sealed record DocumentPage(List<DocumentItem>? Items, int PageNumber, int PageSize, int TotalCount);
    private sealed record DocumentItem(int Id, string Type, string Title, string Version, string Sha256);
    private sealed record UploadResponse(int DocumentId, string Status, string SignedFileSha256);
}
