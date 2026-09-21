namespace Firmador.ApiClient.Abstractions;

public sealed class SesionExpiradaException : Exception
{
    public SesionExpiradaException()
        : base("La sesión venció. Inicie sesión nuevamente.")
    {
    }

    public SesionExpiradaException(Exception innerException)
        : base("La sesión venció. Inicie sesión nuevamente.", innerException)
    {
    }
}
