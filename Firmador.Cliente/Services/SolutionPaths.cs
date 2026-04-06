namespace Firmador.Cliente.Services;

public sealed class SolutionPaths
{
    private const string SolutionFileName = "Firmador.slnx";

    private readonly string _solutionRoot;

    public SolutionPaths()
    {
        _solutionRoot = ResolverSolutionRoot();
    }

    public string ObtenerRutaDocumentoPdf(int documentoId)
    {
        return Path.Combine(_solutionRoot, "docs", $"{documentoId}.pdf");
    }

    public string ObtenerDirectorioFirmados()
    {
        return Path.Combine(_solutionRoot, "docs", "firmados");
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
