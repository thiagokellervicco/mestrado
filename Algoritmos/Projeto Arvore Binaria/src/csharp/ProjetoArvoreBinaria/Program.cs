using ProjetoArvoreBinaria.Services;
using ProjetoArvoreBinaria.Trees;

var datasetPath = PathResolver.EncontrarDataset();
if (datasetPath is null)
{
    Console.Error.WriteLine("Nenhum dataset encontrado. Coloque na raiz do projeto:");
    Console.Error.WriteLine("  student_performance_prediction_dataset-2.csv  ou  .xlsx");
    return 1;
}

var raizRepo = Path.GetDirectoryName(Path.GetFullPath(datasetPath))!;

var sistema = SistemaInfo.Imprimir();

Console.WriteLine();
Console.WriteLine(new string('=', 70));
Console.WriteLine("  ABB & AVL — Desempenho de Estudantes (C#)");
Console.WriteLine(new string('=', 70));

Console.WriteLine("\n[1] Carregando dados e construindo árvores...");
var (abb, avl, tAbb, tAvl, nLinhas) = DatasetLoader.Carregar(datasetPath);
Console.WriteLine($"    Linhas lidas: {nLinhas:N0}");
Console.WriteLine($"    ✔ {abb}  |  {tAbb * 1000:F3} ms");
Console.WriteLine($"    ✔ {avl}  |  {tAvl * 1000:F3} ms");

Console.WriteLine("\n[2] Informações das árvores");
Console.WriteLine($"    {"Total de nós",-25} {abb.Total,10} {avl.Total,10}");
Console.WriteLine($"    {"Altura",-25} {abb.Altura(),10} {avl.Altura(),10}");

var idsTeste = new[] { 238418, 91535, 999999 };
Console.WriteLine("\n[3] Busca por student_id (ABB)");
foreach (var sid in idsTeste)
{
    var r = abb.Buscar(sid);
    Console.WriteLine(r is null ? $"    ✘ {sid} não encontrado" : $"    ✔ {r}");
}
Console.WriteLine("\n    Busca por student_id (AVL)");
foreach (var sid in idsTeste)
{
    var r = avl.Buscar(sid);
    Console.WriteLine(r is null ? $"    ✘ {sid} não encontrado" : $"    ✔ {r}");
}

Console.WriteLine("\n[4] Em-ordem — 5 primeiros (ABB)");
foreach (var est in abb.EmOrdem().Take(5))
    Console.WriteLine($"    • {est}");

const int idMin = 100_000, idMax = 110_000;
var faixaAbb = abb.BuscarPorFaixa(idMin, idMax);
var faixaAvl = avl.BuscarPorFaixa(idMin, idMax);
Console.WriteLine($"\n[5] Faixa [{idMin} — {idMax}]  ABB: {faixaAbb.Count} | AVL: {faixaAvl.Count}");

const int idRemover = 238418;
Console.WriteLine($"\n[6] Removendo student_id = {idRemover}");
Console.WriteLine($"    ABB: {(abb.Remover(idRemover) ? "✔" : "✘")} | Total: {abb.Total}");
Console.WriteLine($"    AVL: {(avl.Remover(idRemover) ? "✔" : "✘")} | Total: {avl.Total}");

Console.WriteLine("\n[7] Operador Contains");
Console.WriteLine($"    91535 in abb → {abb.Contains(91535)} | in avl → {avl.Contains(91535)}");

Console.WriteLine("\n[8] Nota 8.0–10.0");
Console.WriteLine($"    ABB: {abb.BuscarPorNota(8, 10).Count} | AVL: {avl.BuscarPorNota(8, 10).Count}");

Console.WriteLine("\n[9] Aprovados / reprovados");
Console.WriteLine($"    ABB: {abb.BuscarAprovados().Count} aprov. | {abb.BuscarReprovados().Count} reprov.");
Console.WriteLine($"    AVL: {avl.BuscarAprovados().Count} aprov. | {avl.BuscarReprovados().Count} reprov.");

Console.WriteLine("\n[10] Idade 20 anos");
Console.WriteLine($"    ABB: {abb.BuscarPorIdade(20).Count} | AVL: {avl.BuscarPorIdade(20).Count}");

Console.WriteLine("\n[11] Gênero Female");
Console.WriteLine($"    ABB: {abb.BuscarPorGenero("Female").Count} | AVL: {avl.BuscarPorGenero("Female").Count}");

var bench = BenchmarkService.Executar(abb, avl, tAbb, tAvl);
BenchmarkService.Imprimir(bench);
RelatorioHtml.EscreverExecucao(raizRepo, datasetPath, nLinhas, sistema, bench);

Console.WriteLine("\n" + new string('=', 70));
Console.WriteLine("  Concluído com sucesso!");
Console.WriteLine(new string('=', 70));
return 0;
