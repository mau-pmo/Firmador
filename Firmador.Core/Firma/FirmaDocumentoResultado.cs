namespace Firmador.Core.Firma;

public sealed class FirmaDocumentoResultado
{
    public required int DocumentoId { get; init; }
    public required string ArchivoOriginal { get; init; }
    public required string ArchivoFirmado { get; init; }
}
