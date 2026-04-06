namespace Firmador.Cliente.ViewModels;

public sealed class DocumentoGridItem
{
    public bool Seleccionado { get; set; }
    public int Id { get; init; }
    public string TipoDocumento { get; init; } = string.Empty;
    public string Titulo { get; init; } = string.Empty;
    public string Hash { get; init; } = string.Empty;
}
