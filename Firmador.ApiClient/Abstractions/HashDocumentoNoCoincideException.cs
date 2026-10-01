namespace Firmador.ApiClient.Abstractions;

public sealed class HashDocumentoNoCoincideException : IOException
{
    public HashDocumentoNoCoincideException(int documentoId)
        : base($"El PDF del documento {documentoId} no coincide con el SHA-256 informado por la API.")
    {
    }
}
