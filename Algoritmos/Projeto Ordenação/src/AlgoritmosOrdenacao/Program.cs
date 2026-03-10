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
    var timestamped = Directory.GetFiles(docsDir, "Relatorio*.html")
        .Select(Path.GetFileName)
        .Where(n => n != null && System.Text.RegularExpressions.Regex.IsMatch(n, @"Relatorio-[^-]+-\d{8}_\d{6}\.html$"))
        .OrderDescending()
        .ToList();

    var demoPath = Path.Combine(docsDir, "Relatorio-Demo.html");
    var allHtml = File.Exists(demoPath)
        ? ["Relatorio-Demo.html", .. timestamped]
        : timestamped;

    var cardItems = allHtml.Select(f => {
        string label;
        if (f == "Relatorio-Demo.html")
            label = "Demo (10 elementos)";
        else
        {
            var name = f!.Replace("Relatorio-", "").Replace(".html", "");
            var parts = name.Split('-');
            label = name.Replace("-", " ");
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
        }
        return $"    <a href=\"{f}\" class=\"report-card\">{label}</a>";
    });
    var cardsHtml = string.Join("\n", cardItems);

    var machineLines = MachineInfo.CollectStaticDescription().Split('\n', StringSplitOptions.RemoveEmptyEntries);
    var machineInfoHtml = string.Join("", machineLines.Select(l => $"<li>{l.TrimStart('•', ' ')}</li>"));

    var indexPath = Path.Combine(docsDir, "index.html");
    var indexContent = @"<!DOCTYPE html>
<html lang=""pt-BR"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <meta name=""description"" content=""Projeto de Mestrado — Comparação de desempenho entre algoritmos clássicos de ordenação em C# (Bubble, Selection, Insertion, Merge, Quick Sort e otimizados)."">
  <link rel=""icon"" href=""https://upload.wikimedia.org/wikipedia/commons/2/2e/Mackenzie_M.png"" type=""image/png"">
  <title>Algoritmos de Ordenação — Mestrado Mackenzie</title>
  <style>
* { box-sizing: border-box; }
body { font-family: 'Segoe UI', system-ui, sans-serif; max-width: 1200px; margin: 0 auto; padding: 2rem; background: #1a1a1a; color: #e0e0e0; }
.back-link { display: inline-block; font-size: 0.9rem; color: #9aa8c2; text-decoration: none; margin-bottom: 1.5rem; }
.back-link:hover { color: #6ba3e8; }
h1 { color: #e8e8e8; border-bottom: 3px solid #4a6fa5; padding-bottom: 0.5rem; }
h2 { color: #c8d4e6; margin-top: 2rem; }
h3 { color: #9aa8c2; }
.metodologia { background: #252525; padding: 1.5rem; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.3); line-height: 1.7; }
a { color: #6ba3e8; text-decoration: none; }
a:hover { text-decoration: underline; }
.report-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 1rem; margin: 1rem 0; }
@media (max-width: 900px) { .report-grid { grid-template-columns: repeat(2, 1fr); } }
@media (max-width: 480px) { .report-grid { grid-template-columns: 1fr; } }
.report-card { display: block; padding: 1rem 1.25rem; background: #2d2d2d; border: 1px solid #333; border-radius: 8px; color: #6ba3e8; font-weight: 600; text-decoration: none; transition: background 0.2s, border-color 0.2s; }
.report-card:hover { background: #353535; border-color: #4a6fa5; color: #8bb8f0; }
p { color: #b0b0b0; margin: 0.5rem 0; }
code { background: #3a3a3a; color: #c9a959; padding: 0.1em 0.3em; border-radius: 3px; font-size: 0.9em; }
.github-link { display: inline-flex; align-items: center; gap: 0.5rem; margin-top: 1rem; padding: 0.5rem 1rem; background: #24292e; color: #e0e0e0 !important; border-radius: 6px; font-weight: 600; }
.github-link:hover { background: #333; text-decoration: none; color: #e0e0e0 !important; }
.info-maquina { font-size: 0.95rem; margin: 0.5rem 0; color: #b0b0b0; }
.algoritmos-table { width: 100%; border-collapse: collapse; margin: 1rem 0; background: #252525; border-radius: 8px; overflow: hidden; }
.algoritmos-table th, .algoritmos-table td { padding: 0.75rem 1rem; text-align: left; border-bottom: 1px solid #333; }
.algoritmos-table th { background: #2d2d2d; color: #9aa8c2; font-weight: 600; }
.algoritmos-table td { color: #e0e0e0; }
.algoritmos-table tr:last-child td { border-bottom: none; }
.algoritmos-table code { background: #3a3a3a; color: #c9a959; }
pre { background: #2d2d2d; padding: 1rem; border-radius: 6px; overflow-x: auto; border: 1px solid #333; margin: 0.75rem 0; }
pre code { padding: 0; background: transparent; }
  </style>
</head>
<body>
  <a href=""../../../index.html"" class=""back-link"">← Voltar ao Mestrado</a>

<section class=""metodologia"">
  <h1>Algoritmos de Ordenação</h1>
  <p><strong>Aluno:</strong> Thiago Keller | <strong>RA:</strong> 10779365 | <strong>Curso:</strong> Mestrado de Computação Aplicada</p>
  <p><strong>Instituição:</strong> Universidade Presbiteriana Mackenzie | <strong>Professora:</strong> Dra Valeria Farinazzo Martins | <strong>Ano:</strong> 2026 (1º semestre)</p>
  <p>Projeto de Mestrado – Comparação de desempenho entre algoritmos clássicos de ordenação.</p>

  <h2>Algoritmos</h2>
  <p>Complexidade assintótica dos algoritmos implementados:</p>
  <table class=""algoritmos-table"">
    <thead>
      <tr><th>Algoritmo</th><th>Melhor Caso</th><th>Caso Médio</th><th>Pior Caso</th></tr>
    </thead>
    <tbody>
      <tr><td>Bubble Sort</td><td><code>O(n²)</code></td><td><code>O(n²)</code></td><td><code>O(n²)</code></td></tr>
      <tr><td>Bubble Sort (Optimized)</td><td><code>O(n)</code></td><td><code>O(n²)</code></td><td><code>O(n²)</code></td></tr>
      <tr><td>Selection Sort</td><td><code>O(n²)</code></td><td><code>O(n²)</code></td><td><code>O(n²)</code></td></tr>
      <tr><td>Selection Sort (Double)</td><td><code>O(n²)</code></td><td><code>O(n²)</code></td><td><code>O(n²)</code></td></tr>
      <tr><td>Insertion Sort</td><td><code>O(n)</code></td><td><code>O(n²)</code></td><td><code>O(n²)</code></td></tr>
      <tr><td>Insertion Sort (Sentinel)</td><td><code>O(n)</code></td><td><code>O(n²)</code></td><td><code>O(n²)</code></td></tr>
      <tr><td>Merge Sort</td><td><code>O(n log n)</code></td><td><code>O(n log n)</code></td><td><code>O(n log n)</code></td></tr>
      <tr><td>Merge Sort (Single Aux)</td><td><code>O(n log n)</code></td><td><code>O(n log n)</code></td><td><code>O(n log n)</code></td></tr>
      <tr><td>Quick Sort</td><td><code>O(n log n)</code></td><td><code>O(n log n)</code></td><td><code>O(n²)</code></td></tr>
      <tr><td>Quick Sort (Pivô Aleatório)</td><td><code>O(n log n)</code></td><td><code>O(n log n)</code></td><td><code>O(n²)</code></td></tr>
      <tr><td>Quick Sort (Hoare + Median + Insertion)</td><td><code>O(n log n)</code></td><td><code>O(n log n)</code></td><td><code>O(n²)</code></td></tr>
      <tr><td>Array.Sort (C# Nativo)</td><td><code>O(n log n)</code></td><td><code>O(n log n)</code></td><td><code>O(n log n)</code></td></tr>
    </tbody>
  </table>

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

  <h2>Benchmark em Modo Release</h2>
  <p>Para rodar o benchmark em modo Release e garantir que o JIT (Just-In-Time compiler) aplique todas as otimizações de performance (como inlining de métodos e eliminação de bounds checking), o comando correto é:</p>
  <pre><code>dotnet run -c Release --project src/AlgoritmosOrdenacao -- --optimized</code></pre>
  <p><strong>Por que o <code>-c Release</code> é fundamental?</strong></p>
  <ul>
    <li><strong>Otimização de Código:</strong> No modo Debug, o compilador mantém o código ""literal"" para facilitar o rastreio de erros. No modo Release, ele reescreve partes do seu algoritmo para rodar mais rápido na CPU.</li>
    <li><strong>Performance do GC:</strong> O Garbage Collector se comporta de forma mais agressiva e eficiente em Release, o que pode impactar os tempos de memória que você vê no Merge Sort.</li>
    <li><strong>Remoção de Instruções NOP:</strong> O modo Debug insere instruções ""vazias"" para permitir que os breakpoints funcionem corretamente. Isso adiciona uma latência que, em 100 mil elementos, se torna muito visível.</li>
  </ul>

  <h2>Relatórios</h2>
  <p>Clique para visualizar cada relatório com metodologia, resultados e gráficos.</p>
  <div class=""report-grid"">
" + cardsHtml + @"
  </div>

  <a href=""https://github.com/thiagokellervicco/MestradoMackenzie/tree/master/Algoritmos/Projeto%20Ordena%C3%A7%C3%A3o"" target=""_blank"" rel=""noopener"" class=""github-link"">Ver código no GitHub</a>
</section>
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
