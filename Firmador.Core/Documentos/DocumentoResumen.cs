namespace Firmador.Core.Documentos;

public sealed class DocumentoResumen
{
    public required int Id { get; init; }
    public required string TipoDocumento { get; init; }
    public required string Titulo { get; init; }
    public required string Hash { get; init; }
}
