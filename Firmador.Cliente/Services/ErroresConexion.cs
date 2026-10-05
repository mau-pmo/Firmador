using System.Net;
using System.Net.Sockets;

namespace Firmador.Cliente.Services;

internal static class ErroresConexion
{
    public const string Mensaje = "No hay acceso al servidor EDA";

    public static bool EsFallaDeConexion(Exception exception)
    {
        for (var error = exception; error is not null; error = error.InnerException)
        {
            if (error is HttpRequestException { StatusCode: null } or TaskCanceledException or SocketException)
                return true;

            if (error is WebException webException && webException.Status is
                WebExceptionStatus.ConnectFailure or
                WebExceptionStatus.ConnectionClosed or
                WebExceptionStatus.KeepAliveFailure or
                WebExceptionStatus.NameResolutionFailure or
                WebExceptionStatus.ProxyNameResolutionFailure or
                WebExceptionStatus.ReceiveFailure or
                WebExceptionStatus.SendFailure or
                WebExceptionStatus.Timeout or
                WebExceptionStatus.TrustFailure)
                return true;
        }

        return false;
    }
}
