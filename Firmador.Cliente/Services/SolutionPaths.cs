namespace Firmador.Cliente.Services;

public sealed class SolutionPaths
{
    private const string SolutionFileName = "Firmador.slnx";
    private const string DocsDirectoryName = "docs";
    private const string SignedDirectoryName = "firmados";

    private readonly string _documentosRoot;
    private readonly string _directorioFirmados;

    public SolutionPaths()
    {
        _documentosRoot = ResolverDirectorioDocumentos();
        _directorioFirmados = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Firmador",
            DocsDirectoryName,
            SignedDirectoryName);
    }

    public string ObtenerRutaDocumentoPdf(int documentoId)
    {
        return Path.Combine(_documentosRoot, $"{documentoId}.pdf");
    }

    public string ObtenerDirectorioFirmados()
    {
        return _directorioFirmados;
    }

    private static string ResolverDirectorioDocumentos()
    {
        var directorioPublicacion = Path.Combine(AppContext.BaseDirectory, DocsDirectoryName);
        if (Directory.Exists(directorioPublicacion))
        {
            return directorioPublicacion;
        }

        var solutionRoot = ResolverSolutionRoot();
        var directorioDesarrollo = Path.Combine(solutionRoot, DocsDirectoryName);
        if (Directory.Exists(directorioDesarrollo))
        {
            return directorioDesarrollo;
        }

        throw new DirectoryNotFoundException($"No se pudo encontrar la carpeta {DocsDirectoryName}.");
    }

    private static string ResolverSolutionRoot()
    {
        var directorioActual = new DirectoryInfo(AppContext.BaseDirectory);

        while (directorioActual is not null)
        {
            var rutaSolucion = Path.Combine(directorioActual.FullName, SolutionFileName);
            if (File.Exists(rutaSolucion))
            {
                return directorioActual.FullName;
            }

            directorioActual = directorioActual.Parent;
        }

        throw new DirectoryNotFoundException($"No se pudo resolver la raiz de la solucion buscando {SolutionFileName}.");
    }
}
