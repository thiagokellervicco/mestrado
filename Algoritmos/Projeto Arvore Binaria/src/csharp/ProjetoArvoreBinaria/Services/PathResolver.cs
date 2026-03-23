namespace ProjetoArvoreBinaria.Services;

/// <summary>Localiza o dataset na raiz do repositório (como no projeto Python).</summary>
public static class PathResolver
{
    private static readonly string[] NomesArquivo =
    {
        "student_performance_prediction_dataset-2.csv",
        "student_performance_prediction_dataset-2.xlsx"
    };

    public static string? EncontrarDataset()
    {
        foreach (var start in PontosPartida())
        {
            var dir = new DirectoryInfo(start);
            for (var i = 0; i < 14 && dir is not null; i++)
            {
                foreach (var nome in NomesArquivo)
                {
                    var p = Path.Combine(dir.FullName, nome);
                    if (File.Exists(p)) return p;
                }
                dir = dir.Parent;
            }
        }
        return null;
    }

    private static IEnumerable<string> PontosPartida()
    {
        yield return Environment.CurrentDirectory;
        yield return AppContext.BaseDirectory;
    }
}
