namespace Firmador.Core.Firma;

public sealed class ServidorTsaNoDisponibleException : Exception
{
    public ServidorTsaNoDisponibleException(Exception innerException)
        : base("No hay acceso al servidor de sello de tiempo (TSA)", innerException)
    {
    }
}
