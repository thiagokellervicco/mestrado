using AlgoritmosOrdenacao.Algoritmos;
using AlgoritmosOrdenacao.AlgoritmosOtimizados;
using AlgoritmosOrdenacao.Core;
using AlgoritmosOrdenacao.Relatorio;

var quickMode = args.Contains("--rapido");
var ultraMode = args.Contains("--ultra");
var optimizedMode = args.Contains("--optimized");
var demoMode = args.Contains("--demo");
var demoReportMode = args.Contains("--demo-report");
var pagesMode = args.Contains("--pages");
var indexOnly = args.Contains("--index-only");
var filteredArgs = args
    .Where(a => a != "--rapido" && a != "--completo" && a != "--ultra" && a != "--optimized" && a != "--demo" && a != "--demo-report" && a != "--pages" && a != "--index-only")
    .Where(a => !a.StartsWith("--"))
    .ToArray();

if (demoReportMode)
{
    RunDemoReport(optimizedMode);
    return;
}

if (demoMode)
{
    RunDemo(optimizedMode);
    return;
}

var modo = quickMode ? "Rapido" : ultraMode ? "Ultra" : optimizedMode ? "Otimizado" : "Completo";
var dataHora = DateTime.Now.ToString("yyyyMMdd_HHmmss");

Console.WriteLine("=== Algoritmos de Ordenação ===");
Console.WriteLine();
Console.WriteLine(quickMode ? "Modo RÁPIDO: apenas 1.000 e 10.000 elementos (para validação)"
    : ultraMode ? "Modo ULTRA: 1.000, 10.000, 100.000 e 10.000.000 elementos (apenas O(n log n) em 10M)"
    : "Tamanhos: 1.000, 10.000, 100.000 elementos");
Console.WriteLine("Tipos: Aleatório, Crescente, Decrescente");
Console.WriteLine(optimizedMode
    ? "Algoritmos: padrões + Bubble (Optimized), Insertion (Sentinel), Selection (Double), Merge (Single Aux), Quick (Hoare+Median+Insertion)"
    : "Algoritmos: Bubble, Selection, Insertion, Merge, Quick, Quick (pivô aleatório), Array.Sort (C# nativo)");
Console.WriteLine();
if (!quickMode && !ultraMode)
    Console.WriteLine("AVISO: Vetores de 100.000 elementos com algoritmos O(n²) podem demorar vários minutos.");
if (ultraMode)
    Console.WriteLine("AVISO: Modo ULTRA com 10M elementos pode levar 15-30 minutos.");
Console.WriteLine("Cada execução utiliza novo espaço de memória (cópia do vetor).");
Console.WriteLine();

if (indexOnly)
{
    var docsDir = Path.Combine(Directory.GetCurrentDirectory(), "docs");
    if (Directory.Exists(docsDir))
    {
        UpdateDocsIndex(docsDir);
        Console.WriteLine("Índice de relatórios atualizado.");
    }
    else
        Console.WriteLine("Pasta docs/ não encontrada.");
    return;
}

var reportPath = filteredArgs.Length > 0
    ? Path.GetFullPath(filteredArgs[0].EndsWith(".html", StringComparison.OrdinalIgnoreCase) ? filteredArgs[0] : $"{filteredArgs[0]}.html")
    : Path.Combine(Directory.GetCurrentDirectory(), "docs", $"Relatorio-{modo}-{dataHora}.html");

var reportDir = Path.GetDirectoryName(reportPath);
if (!string.IsNullOrEmpty(reportDir))
    Directory.CreateDirectory(reportDir);

if (pagesMode)
    Console.WriteLine("Modo GitHub Pages: índice será atualizado após salvar.");
Console.WriteLine($"Relatório será salvo em: {Path.GetFullPath(reportPath)}");
Console.WriteLine();

var runner = new BenchmarkRunner(ultraMode, optimizedMode);
Console.WriteLine(quickMode ? "Executando testes... (modo rápido)"
    : ultraMode ? "Executando testes... (modo ultra com 10M elementos)"
    : optimizedMode ? "Executando testes... (padrão + otimizados)"
    : "Executando testes...");
Console.WriteLine();

var sw = System.Diagnostics.Stopwatch.StartNew();
var results = runner.Run();
sw.Stop();
var totalElapsed = sw.Elapsed;

var generator = new ReportGenerator(reportPath);
generator.Generate(results, totalElapsed);

if (reportDir != null && reportDir.EndsWith("docs"))
    UpdateDocsIndex(reportDir!);

Console.WriteLine($"Tempo total de execução: {FormatTotalTime(totalElapsed)}");
Console.WriteLine($"Relatório salvo em: {Path.GetFullPath(reportPath)}");
Console.WriteLine();
Console.WriteLine("Concluído. Abra o arquivo HTML no navegador para visualizar os resultados.");

static string FormatTotalTime(TimeSpan t)
{
    if (t.TotalSeconds < 60)
        return $"{t.TotalSeconds:F2} s";
    var min = (int)t.TotalMinutes;
    var sec = t.TotalSeconds - 60 * min;
    return $"{min} min {sec:F1} s";
}

static void UpdateDocsIndex(string docsDir)
{

    var allHtml = Directory.GetFiles(docsDir, "Relatorio*.html")
        .Select(Path.GetFileName)
        .Where(n => n != null && System.Text.RegularExpressions.Regex.IsMatch(n, @"Relatorio-[^-]+-\d{8}_\d{6}\.html$"))
        .OrderDescending()
        .ToList();

    var listItems = allHtml.Select(f => {
        var name = f!.Replace("Relatorio-", "").Replace(".html", "");
        var parts = name.Split('-');
        string label = name.Replace("-", " ");
        if (parts.Length >= 2)
        {
            var last = parts[^1];
            if (last.Length >= 8 && last.Contains('_')) // yyyyMMdd_HHmmss
            {
                var modoNome = string.Join(" ", parts[..^1]);
                var d = last[6..8];
                var m = last[4..6];
                var y = last[0..4];
                var h = last.Length > 9 ? last[9..11] : "";
                var min = last.Length > 11 ? last[11..13] : "";
                label = string.IsNullOrEmpty(h) ? $"{modoNome} ({d}/{m}/{y})" : $"{modoNome} ({d}/{m}/{y} {h}:{min})";
            }
        }
        return $"    <li><a href=\"{f}\">{label}</a></li>";
    });
    var listHtml = string.Join("\n", listItems);

    var machineLines = MachineInfo.CollectStaticDescription().Split('\n', StringSplitOptions.RemoveEmptyEntries);
    var machineInfoHtml = string.Join("", machineLines.Select(l => $"<li>{l.TrimStart('•', ' ')}</li>"));

    var indexPath = Path.Combine(docsDir, "index.html");
    var indexContent = @"<!DOCTYPE html>
<html lang=""pt-BR"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Algoritmos de Ordenação</title>
  <style>
* { box-sizing: border-box; }
body { font-family: 'Segoe UI', system-ui, sans-serif; max-width: 1200px; margin: 0 auto; padding: 2rem; background: #f5f5f5; }
h1 { color: #1a1a2e; border-bottom: 3px solid #16213e; padding-bottom: 0.5rem; }
h2 { color: #16213e; margin-top: 2rem; }
h3 { color: #0f3460; }
.metodologia { background: white; padding: 1.5rem; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); line-height: 1.7; }
a { color: #1565c0; text-decoration: none; }
a:hover { text-decoration: underline; }
.report-list { list-style: none; padding: 0; margin: 1rem 0; }
.report-list li { display: block; margin: 0.5rem 0; padding: 0.75rem 1rem; background: #f8f9fa; border-left: 4px solid #16213e; border-radius: 4px; }
.report-list a { display: block; color: #1565c0; font-weight: 600; }
.report-list a:hover { color: #0d47a1; }
p { color: #444; margin: 0.5rem 0; }
code { background: #f0f0f0; padding: 0.1em 0.3em; border-radius: 3px; font-size: 0.9em; }
.github-link { display: inline-flex; align-items: center; gap: 0.5rem; margin-top: 1rem; padding: 0.5rem 1rem; background: #24292e; color: white !important; border-radius: 6px; font-weight: 600; }
.github-link:hover { background: #333; text-decoration: none; color: white !important; }
.info-maquina { font-size: 0.95rem; margin: 0.5rem 0; }
  </style>
</head>
<body>
<section class=""metodologia"">
  <h1>Algoritmos de Ordenação</h1>
  <p><strong>Aluno:</strong> Thiago Keller | <strong>RA:</strong> 10779365 | <strong>Curso:</strong> Mestrado de Computação Aplicada</p>
  <p><strong>Instituição:</strong> Universidade Presbiteriana Mackenzie | <strong>Professora:</strong> Dra Valeria Farinazzo Martins | <strong>Ano:</strong> 2026 (1º semestre)</p>
  <p>Projeto de Mestrado – Comparação de desempenho entre algoritmos clássicos de ordenação.</p>

  <h2>Informações da Máquina</h2>
  <p>Configuração fixa do hardware/software (comum a todos os relatórios). Dados como fonte de energia, memória GC e heap são específicos de cada execução e aparecem nos relatórios individuais.</p>
  <ul class=""info-maquina"">
" + machineInfoHtml + @"
  </ul>

  <h2>Algoritmos Otimizados</h2>
  <p>Além dos algoritmos clássicos, o projeto inclui versões otimizadas (use <code>--optimized</code> nos benchmarks). Fontes das otimizações:</p>
  <ul>
    <li><strong>Bubble Sort (Optimized)</strong> — Early exit: interrompe quando nenhuma troca ocorrer na passada. O(n²) pior caso, O(n) melhor. <em>Otimização clássica da literatura de algoritmos.</em></li>
    <li><strong>Insertion Sort (Sentinel)</strong> — Menor elemento na posição 0 elimina a verificação <code>j &gt;= 0</code> no loop interno. <em>Cormen et al., Introduction to Algorithms, Cap. 2.</em></li>
    <li><strong>Selection Sort (Double)</strong> — Busca menor e maior por passagem, posicionando ambos nas extremidades. Reduz iterações à metade. <em>Variação conhecida na literatura de ordenação.</em></li>
    <li><strong>Merge Sort (Single Aux Array)</strong> — Um único array auxiliar para todo o merge, alocado uma vez. <em>Cormen et al., Introduction to Algorithms, Cap. 2.</em></li>
    <li><strong>Quick Sort (Hoare + Median + Insertion)</strong> — Particionamento de Hoare (<em>Cormen, Cap. 7</em>), pivô mediana-de-três (<em>Sedgewick</em>), Insertion Sort para subarrays pequenos e eliminação de recursão de cauda (<em>padrão em implementações híbridas</em>).</li>
  </ul>

  <h2>Relatórios</h2>
  <p>Clique para visualizar cada relatório com metodologia, resultados e gráficos.</p>
  <ul class=""report-list"">
" + listHtml + """

                 </ul>

                 <a href="https://github.com/thiagokellervicco/ProjetoAlgoritmoMackenzie" target="_blank" rel="noopener" class="github-link">Ver código no GitHub</a>
               </section>

               <p style="font-size: 0.9rem; color: #666; margin-top: 1rem;">
                 Execute <code>dotnet run --project src/AlgoritmosOrdenacao -- --pages</code> para gerar um novo relatório.
               </p>
               </body>
               </html>
               """;
    File.WriteAllText(indexPath, indexContent);
    Console.WriteLine($"Índice atualizado: {indexPath}");
}

static void RunDemoReport(bool includeOptimized)
{
    var docsDir = Path.Combine(Directory.GetCurrentDirectory(), "docs");
    Directory.CreateDirectory(docsDir);
    var reportPath = Path.Combine(docsDir, "Relatorio-Demo.html");

    Console.WriteLine("=== Relatório Demo (10 elementos) ===");
    Console.WriteLine($"Relatório será salvo em: {Path.GetFullPath(reportPath)}");
    Console.WriteLine();

    var runner = new BenchmarkRunner(ultraMode: false, optimizedMode: includeOptimized, demoMode: true);
    var sw = System.Diagnostics.Stopwatch.StartNew();
    var results = runner.Run();
    sw.Stop();

    var generator = new ReportGenerator(reportPath);
    generator.Generate(results, sw.Elapsed);

    Console.WriteLine($"Relatório salvo em: {Path.GetFullPath(reportPath)}");
    Console.WriteLine("Concluído. Abra o arquivo HTML no navegador.");
}

static void RunDemo(bool includeOptimized)
{
    const int Size = 10;
    var entrada = DataGenerator.Generate(Size, VectorType.Random);

    Console.WriteLine("=== Demonstração rápida (10 registros) ===");
    Console.WriteLine();
    Console.WriteLine($"Entrada: [{string.Join(", ", entrada)}]");
    Console.WriteLine();

    var algorithms = new List<ISortAlgorithm>
    {
        new BubbleSort(),
        new SelectionSort(),
        new InsertionSort(),
        new MergeSort(),
        new QuickSort(),
        new ArraySortNative()
    };
    if (includeOptimized)
    {
        algorithms.AddRange(
        [
            new BubbleSortOptimized(),
            new InsertionSortOptimized(),
            new SelectionSortOptimized(),
            new MergeSortOptimized(),
            new QuickSortFinal()
        ]);
    }

    foreach (var alg in algorithms)
    {
        var copia = (int[])entrada.Clone();
        alg.Sort(copia);
        Console.WriteLine($"{alg.Name,-40} Saída: [{string.Join(", ", copia)}]");
    }

    Console.WriteLine();
    Console.WriteLine("Todos os algoritmos produziram a mesma ordenação.");
}
