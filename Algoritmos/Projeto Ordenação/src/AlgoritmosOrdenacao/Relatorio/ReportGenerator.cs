using System.Globalization;
using System.Text;

namespace AlgoritmosOrdenacao.Relatorio;

/// <summary>
/// Gera relatório HTML completo com metodologia, gráficos e análise crítica.
/// </summary>
public sealed class ReportGenerator(string outputPath)
{
    private readonly string _outputPath = outputPath ?? throw new ArgumentNullException(nameof(outputPath));

    public void Generate(IReadOnlyList<Core.BenchmarkResult> results, TimeSpan? totalElapsed = null)
    {
        var sb = new StringBuilder();
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"pt-BR\">");
        sb.AppendLine("<head>");
        sb.AppendLine("  <meta charset=\"UTF-8\">");
        sb.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        sb.AppendLine("  <title>Relatório - Algoritmos de Ordenação</title>");
        sb.AppendLine("  <script src=\"https://cdn.jsdelivr.net/npm/chart.js\"></script>");
        sb.AppendLine("  <style>");
        sb.AppendLine(GetCssStyles());
        sb.AppendLine("  </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("  <nav class=\"breadcrumb\" aria-label=\"Navegação\">");
        sb.AppendLine("    <a href=\"../../../index.html\">Mestrado</a>");
        sb.AppendLine("    <span class=\"sep\">›</span>");
        sb.AppendLine("    <a href=\"index.html\">Algoritmos de Ordenação</a>");
        sb.AppendLine("    <span class=\"sep\">›</span>");
        sb.AppendLine("    <span aria-current=\"page\">Relatório</span>");
        sb.AppendLine("  </nav>");

        sb.AppendLine(GetMethodologySection(results, totalElapsed));
        sb.AppendLine(GetSampleArraysSection(results));
        sb.AppendLine(GetTablesSection(results));
        sb.AppendLine(GetChartsSection(results));
        sb.AppendLine(GetEfficiencyAnalysisSection());
        sb.AppendLine(GetArraySortDetailsSection());
        sb.AppendLine(GetAssymptoticAnalysisSection());

        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        var dir = Path.GetDirectoryName(_outputPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllText(_outputPath, sb.ToString(), Encoding.UTF8);
    }

    private static string GetCssStyles() => """
        * { box-sizing: border-box; }
        body { font-family: 'Segoe UI', system-ui, sans-serif; max-width: 1200px; margin: 0 auto; padding: 2rem; background: #1a1a1a; color: #e0e0e0; }
        .breadcrumb { font-size: 0.9rem; color: #9aa8c2; margin-bottom: 1.5rem; }
        .breadcrumb a { color: #6ba3e8; text-decoration: none; }
        .breadcrumb a:hover { color: #8bb8f0; text-decoration: underline; }
        .breadcrumb .sep { margin: 0 0.4rem; opacity: 0.7; }
        h1 { color: #e8e8e8; border-bottom: 3px solid #4a6fa5; padding-bottom: 0.5rem; }
        h2 { color: #c8d4e6; margin-top: 2rem; }
        h3 { color: #9aa8c2; }
        table { width: 100%; border-collapse: collapse; background: #252525; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.3); margin: 1rem 0; }
        th, td { padding: 0.75rem 1rem; text-align: left; border-bottom: 1px solid #333; color: #e0e0e0; }
        th { background: #2d3a4f; color: #e0e0e0; }
        tr:hover { background: #2d2d2d; }
        .metodologia { background: #252525; border: 1px solid #333; padding: 1.5rem; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.3); line-height: 1.7; color: #b0b0b0; }
        .metodologia p { color: #b0b0b0; }
        .metodologia li { color: #b0b0b0; }
        .analise { background: #252525; border: 1px solid #333; padding: 1.5rem; border-radius: 8px; margin: 1rem 0; line-height: 1.8; color: #b0b0b0; }
        .analise p { color: #b0b0b0; }
        .chart-container { position: relative; min-height: 400px; margin: 2rem 0; background: #252525; border: 1px solid #333; padding: 1rem; border-radius: 8px; }
        .chart-container canvas { height: 400px !important; }
        .badge { display: inline-block; padding: 0.25rem 0.5rem; border-radius: 4px; font-size: 0.85rem; }
        .badge-aleatorio { background: #2d3a4f; color: #6ba3e8; }
        .badge-crescente { background: #2d3a2d; color: #7cb87c; }
        .badge-decrescente { background: #4a3a2d; color: #e89b6b; }
        a { color: #6ba3e8; text-decoration: none; }
        a:hover { color: #8bb8f0; text-decoration: underline; }
        code { background: #3a3a3a; color: #c9a959; padding: 0.15em 0.4em; border-radius: 3px; font-size: 0.9em; }
        .info-maquina { font-size: 0.95rem; color: #b0b0b0; }
        .chart-melhor { margin-top: 0.5rem; padding: 0.5rem 1rem; background: #2d3a2d; border-left: 4px solid #4a9e4a; font-weight: 600; color: #b0e0b0; }
        .chart-legend-note { margin-bottom: 0.5rem; font-size: 0.95rem; color: #9aa8c2; }
        .observacao { background: #4a4520; padding: 0.75rem 1rem; border-left: 4px solid #c9a959; font-size: 0.95rem; margin: 1rem 0; color: #e0d8b0; }
        .io-table { margin-top: 0.5rem; }
        .io-table code { font-size: 0.8em; word-break: break-all; }
        """;

    private string GetMethodologySection(IReadOnlyList<Core.BenchmarkResult> results, TimeSpan? totalElapsed)
    {
        var usedSizes = string.Join(", ", results.Select(r => r.Size).Distinct().OrderBy(x => x).Select(t => t.ToString("N0", CultureInfo.InvariantCulture)));
        var totalTimeLine = totalElapsed.HasValue
            ? $"<li><strong>Tempo total de execução:</strong> {FormatTotalTime(totalElapsed.Value)}</li>"
            : "";

        var runtimeLines = MachineInfo.CollectRuntimeDescription().Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var runtimeInfoHtml = runtimeLines.Length > 0
            ? $"<h3>Estado da execução</h3><ul class=\"info-maquina\">{string.Join("", runtimeLines.Select(l => $"<li>{l.TrimStart('•', ' ')}</li>"))}</ul>"
            : "";

        return $"""
            <section class="metodologia">
              <h1>Relatório - Algoritmos de Ordenação</h1>
              <p><strong>Aluno:</strong> Thiago Keller | <strong>RA:</strong> 10779365 | <strong>Curso:</strong> Mestrado de Computação Aplicada </p>
              <p><strong>Instituição:</strong> Universidade Presbiteriana Mackenzie | <strong>Professora:</strong> Dra Valeria Farinazzo Martins | <strong>Ano:</strong> 2026 (1º semestre)</p>
              <h2>Metodologia</h2>
              <p><em>Informações fixas da máquina (hardware, SO, .NET) estão na <a href="index.html">página inicial</a>.</em></p>
              {runtimeInfoHtml}
              <h3>Configuração</h3>
              <ul>
                <li><strong>Linguagem:</strong> C# (.NET {Environment.Version})</li>
                <li><strong>Massa de dados:</strong> Vetores de inteiros com tamanhos {usedSizes} elementos</li>
                <li><strong>Tipos de entrada:</strong> Aleatório, Crescente e Decrescente</li>
                <li><strong>Dados aleatórios:</strong> Pseudoaleatórios (System.Random / Random.Shared), determinísticos conforme o seed, adequados para os testes</li>
                <li><strong>Algoritmos:</strong> Bubble Sort (original), Bubble Sort (melhorado), Selection Sort, Insertion Sort, Merge Sort, Quick Sort, Quick Sort (pivô aleatório), Array.Sort (nativo C#)</li>
                <li><strong>Fontes dos algoritmos:</strong> Implementações clássicas baseadas em Cormen et al. (<a href="https://www.amazon.com.br/Introduction-Algorithms-Thomas-H-Cormen/dp/0262033844" target="_blank" rel="noopener">Introduction to Algorithms</a>, 3ª edição) e literatura padrão de estruturas de dados. Array.Sort utiliza IntroSort nativo da biblioteca .NET.</li>
                <li><strong>Memória:</strong> Cada execução utiliza uma cópia em novo espaço de memória (isolamento total)</li>
                <li><strong>Iterações:</strong> 5 execuções por combinação; mediana de tempo reportada</li>
                <li><strong>Medição:</strong> System.Diagnostics.Stopwatch (tempo); GC.GetTotalAllocatedBytes (memória alocada durante a execução)</li>
                {totalTimeLine}
              </ul>
            </section>
            """;
    }

    private static string GetSampleArraysSection(IReadOnlyList<Core.BenchmarkResult> results)
    {
        var withSamples = results
            .GroupBy(r => (r.VectorType, r.Size))
            .Where(g => g.Any(r => r.SampleInput is { Length: > 0 }))
            .Select(g => (
                Key: g.Key,
                Input: g.First(r => r.SampleInput != null).SampleInput!,
                Algorithms: g.Where(r => r.SampleOutput is { Length: > 0 }).ToList()
            ))
            .ToList();

        if (withSamples.Count == 0)
            return "";

        var sb = new StringBuilder();
        sb.AppendLine("<section class=\"metodologia\">");
        sb.AppendLine("  <h2>Vetores Utilizados e Verificação da Corretude</h2>");
        sb.AppendLine("  <p>Para cada tipo de entrada, abaixo estão o vetor utilizado e a saída de cada algoritmo. Todas as saídas devem ser idênticas (vetor ordenado) para garantir que os algoritmos estão corretos.</p>");

        foreach (var item in withSamples.OrderBy(x => x.Key.VectorType).ThenBy(x => x.Key.Size))
        {
            var (vectorType, size) = item.Key;
            var inputArray = item.Input;
            var algorithms = item.Algorithms;
            var desc = Core.DataGenerator.GetDescription(vectorType);
            var inputStr = "[" + string.Join(", ", inputArray) + "]";

            sb.AppendLine($"  <h3>{desc} ({size} elementos)</h3>");
            sb.AppendLine($"  <p><strong>Entrada:</strong> <code>{inputStr}</code></p>");

            if (algorithms.Count > 0)
            {
                var outputs = algorithms.Select(r => r.SampleOutput!).ToList();
                var firstOutput = outputs[0];
                var allEqual = outputs.All(o => o.SequenceEqual(firstOutput));

                sb.AppendLine("  <table class=\"io-table\">");
                sb.AppendLine("    <thead><tr><th>Algoritmo</th><th>Entrada</th><th>Saída</th></tr></thead>");
                sb.AppendLine("    <tbody>");

                foreach (var r in algorithms.OrderBy(x => x.Algorithm))
                {
                    var outputStr = "[" + string.Join(", ", r.SampleOutput!) + "]";
                    var okMark = r.SampleOutput!.SequenceEqual(firstOutput) ? " ✓" : " ✗";
                    sb.AppendLine($"      <tr><td>{r.Algorithm}{okMark}</td><td><code>{inputStr}</code></td><td><code>{outputStr}</code></td></tr>");
                }

                sb.AppendLine("    </tbody>");
                sb.AppendLine("  </table>");
                sb.AppendLine($"  <p class=\"chart-melhor\">{(allEqual ? "✓ Todas as saídas são idênticas — os algoritmos produziram o mesmo resultado ordenado." : "✗ Atenção: há divergência nas saídas. Verifique a implementação.")}</p>");
            }
        }

        sb.AppendLine("</section>");
        return sb.ToString();
    }

    private static string GetTablesSection(IReadOnlyList<Core.BenchmarkResult> results)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<section>");
        sb.AppendLine("  <h2>Resultados Detalhados</h2>");

        var grouped = results
            .GroupBy(r => (r.VectorType, r.Size))
            .OrderBy(g => g.Key.VectorType)
            .ThenBy(g => g.Key.Size);

        foreach (var group in grouped)
        {
            var description = Core.DataGenerator.GetDescription(group.Key.VectorType);
            sb.AppendLine($"  <h3>{description} - {group.Key.Size:N0} elementos</h3>");
            sb.AppendLine("  <table>");
            sb.AppendLine("    <thead><tr><th>Algoritmo</th><th>Tempo Médio (mediana)</th><th>Memória (mediana)</th><th>Tempo Mín</th><th>Tempo Máx</th></tr></thead>");
            sb.AppendLine("    <tbody>");
            var sortedTable = group.OrderBy(x => x.AverageTime).ToList();
            var bestInTable = sortedTable.First();
            foreach (var r in sortedTable)
            {
                var bestMarker = r == bestInTable ? " ★" : "";
                sb.AppendLine($"      <tr>");
                sb.AppendLine($"        <td>{r.Algorithm}{bestMarker}</td>");
                sb.AppendLine($"        <td>{FormatTime(r.AverageTime)}</td>");
                sb.AppendLine($"        <td>{FormatMemory(r.MemoryBytesMedian)}</td>");
                sb.AppendLine($"        <td>{FormatTime(r.MinTime)}</td>");
                sb.AppendLine($"        <td>{FormatTime(r.MaxTime)}</td>");
                sb.AppendLine($"      </tr>");
            }
            sb.AppendLine("    </tbody>");
            sb.AppendLine("  </table>");
        }

        sb.AppendLine("</section>");
        return sb.ToString();
    }

    private static string FormatTime(TimeSpan t)
    {
        return $"{t.TotalSeconds:F6} s";
    }

    private static string FormatTotalTime(TimeSpan t)
    {
        if (t.TotalSeconds < 60)
            return $"{t.TotalSeconds:F2} s";
        var min = (int)t.TotalMinutes;
        var sec = t.TotalSeconds - 60 * min;
        return $"{min} min {sec:F1} s";
    }

    private static string FormatMemory(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F2} KB";
        return $"{bytes / (1024.0 * 1024.0):F2} MB";
    }

    private static string GetChartsSection(IReadOnlyList<Core.BenchmarkResult> results)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<section>");
        sb.AppendLine("  <h2> Gráficos Comparativos</h2>");
        sb.AppendLine("  <p class=\"chart-legend-note\">Eixo do tempo em segundos (escala linear).</p>");

        var colors = new[]
        {
            "rgb(22, 33, 62)",
            "rgb(15, 52, 96)",
            "rgb(233, 69, 96)",
            "rgb(242, 183, 5)",
            "rgb(46, 213, 115)",
            "rgb(0, 210, 211)",
            "rgb(155, 89, 182)",
            "rgb(255, 138, 101)"
        };

        int chartId = 0;
        var grouped = results
            .GroupBy(r => (r.VectorType, r.Size))
            .OrderBy(g => g.Key.VectorType)
            .ThenBy(g => g.Key.Size);

        foreach (var group in grouped)
        {
            chartId++;
            var description = Core.DataGenerator.GetDescription(group.Key.VectorType);
            var sorted = group.OrderBy(x => x.Algorithm).ToList();
            var best = sorted.MinBy(r => r.AverageTime)!;
            var bestIndex = sorted.IndexOf(best);

            var labels = string.Join(", ", sorted.Select(r => $"\"{r.Algorithm}\""));
            var values = string.Join(", ", sorted.Select(r => Math.Max(r.AverageTime.TotalSeconds, 1e-9).ToString(CultureInfo.InvariantCulture)));
            var colorsWithBest = GetJsonColorsWithBest(colors, sorted.Count, bestIndex);

            sb.AppendLine($"  <h3>{description} - {group.Key.Size:N0} elementos</h3>");
            sb.AppendLine($"  <div class=\"chart-container\">");
            sb.AppendLine($"    <canvas id=\"chart{chartId}\"></canvas>");
            sb.AppendLine($"    <p class=\"chart-melhor\">★ Melhor: {best.Algorithm} ({FormatTime(best.AverageTime)})</p>");
            sb.AppendLine($"  </div>");
            sb.AppendLine($"  <script>");
            sb.AppendLine($"  new Chart(document.getElementById('chart{chartId}'), {{");
            sb.AppendLine($"    type: 'bar',");
            sb.AppendLine($"    data: {{");
            sb.AppendLine($"      labels: [{labels}],");
            sb.AppendLine($"      datasets: [{{");
            sb.AppendLine($"        label: 'Tempo',");
            sb.AppendLine($"        data: [{values}],");
            sb.AppendLine($"        backgroundColor: {colorsWithBest}");
            sb.AppendLine($"      }}]");
            sb.AppendLine($"    }},");
            sb.AppendLine($"    options: {{");
            sb.AppendLine($"      responsive: true,");
            sb.AppendLine($"      maintainAspectRatio: false,");
            sb.AppendLine($"      plugins: {{");
            sb.AppendLine($"        tooltip: {{");
            sb.AppendLine($"          callbacks: {{");
            sb.AppendLine($"            label: function(ctx) {{");
            sb.AppendLine($"              var v = (ctx.parsed && ctx.parsed.y !== undefined) ? ctx.parsed.y : (ctx.raw != null ? ctx.raw : ctx.parsed);");
            sb.AppendLine($"              return (v != null ? v : 0).toFixed(6) + ' s';");
            sb.AppendLine($"            }}");
            sb.AppendLine($"          }}");
            sb.AppendLine($"        }},");
            sb.AppendLine($"        legend: {{");
            sb.AppendLine($"          display: true,");
            sb.AppendLine($"          position: 'bottom',");
            sb.AppendLine($"          labels: {{ color: '#9aa8c2', usePointStyle: true }},");
            sb.AppendLine($"          generateLabels: function(chart) {{");
            sb.AppendLine($"            var ds = chart.data.datasets[0];");
            sb.AppendLine($"            return chart.data.labels.map(function(label, i) {{");
            sb.AppendLine($"              return {{ text: label, fillStyle: ds.backgroundColor[i], hidden: false, index: i }};");
            sb.AppendLine($"            }});");
            sb.AppendLine($"          }}");
            sb.AppendLine($"        }}");
            sb.AppendLine($"      }},");
            sb.AppendLine($"      scales: {{");
            sb.AppendLine($"        x: {{ ticks: {{ color: '#9aa8c2', maxRotation: 45 }}, grid: {{ color: '#333' }} }},");
            sb.AppendLine($"        y: {{");
            sb.AppendLine($"          ticks: {{");
            sb.AppendLine($"            color: '#9aa8c2',");
            sb.AppendLine($"            callback: function(value) {{ return value.toFixed(6) + ' s'; }}");
            sb.AppendLine($"          }},");
            sb.AppendLine($"          grid: {{ color: '#333' }},");
            sb.AppendLine($"          title: {{ display: true, text: 'Tempo (s)', color: '#9aa8c2' }}");
            sb.AppendLine($"        }}");
            sb.AppendLine($"      }}");
            sb.AppendLine($"    }}");
            sb.AppendLine($"  }});");
            sb.AppendLine($"  </script>");
        }

        sb.AppendLine("</section>");
        return sb.ToString();
    }

    private static string GetJsonColors(string[] colors, int count)
    {
        var arr = Enumerable.Range(0, count).Select(i => $"\"{colors[i % colors.Length]}\"").ToList();
        return "[" + string.Join(", ", arr) + "]";
    }

    private static string GetJsonColorsWithBest(string[] colors, int count, int bestIndex)
    {
        const string bestColor = "rgb(46, 213, 115)"; // Destaque verde
        var arr = Enumerable.Range(0, count)
            .Select(i => i == bestIndex ? bestColor : colors[i % colors.Length])
            .Select(c => $"\"{c}\"");
        return "[" + string.Join(", ", arr) + "]";
    }

    private static string GetEfficiencyAnalysisSection() => """
        <section class="analise">
          <h2>Análise Crítica sobre a Eficiência dos Algoritmos</h2>
          <p><strong>Bubble Sort (original e melhorado):</strong> Ineficientes para grandes volumes (O(n²)). O melhorado interrompe se não houver trocas, beneficiando vetores quase ordenados. Em 100.000 elementos decrescentes, ambos são imprestáveis na prática.</p>
          <p><strong>Selection Sort:</strong> Também O(n²), não se beneficia de dados parcialmente ordenados. Consistente mas lento em todos os cenários.</p>
          <p><strong>Insertion Sort:</strong> O(n²) no pior caso, mas O(n) em vetores já ordenados. Excelente para vetores pequenos ou quase ordenados.</p>
          <p><strong>Merge Sort:</strong> O(n log n) em todos os casos. Previsível e eficiente. Desvantagem: requer O(n) de memória auxiliar.</p>
          <p><strong>Quick Sort:</strong> O(n log n) em média, O(n²) no pior caso (ex.: vetor ordenado com pivô no fim). Na prática, geralmente mais rápido que Merge Sort por constantes menores e melhor uso de cache. Em dados decrescentes pode degradar.</p>
          <p><strong>Quick Sort (Pivô Aleatório):</strong> Variante que escolhe o pivô aleatoriamente. Com dados Crescente ou Decrescente, evita o O(n²) garantido do pivô fixo; em esperança, mantém O(n log n). Compare com Array.Sort para ver se a discrepância nos casos Crescente/Decrescente diminui.</p>
          <p><strong>Array.Sort (C# Nativo):</strong> Utiliza IntroSort, um híbrido que combina QuickSort, HeapSort e Insertion Sort. O .NET usa HeapSort quando QuickSort degenera, garantindo O(n log n) no pior caso. Altamente otimizado (código nativo/IL JIT), costuma superar implementações manuais em todos os cenários. É a referência de performance em C#.</p>
          <p><strong>Conclusão:</strong> Para n pequeno (1.000), Insertion Sort e Bubble melhorado podem competir. Para n grande (100.000), apenas Merge Sort, Quick Sort e Array.Sort são viáveis. Array.Sort tende a ser o mais rápido por otimizações de baixo nível. </p>
        </section>
        """;

    private static string GetArraySortDetailsSection() => """
        <section class="analise">
          <h2>Array.Sort (C#) — Como Funciona e Por Que É Tão Rápido</h2>
          <p>O <code>Array.Sort</code> do .NET utiliza <strong>IntroSort</strong> (Introsort), um algoritmo híbrido que combina três estratégias para obter o melhor de cada uma. A implementação está no runtime do .NET e é altamente otimizada.</p>
          <h3>Estratégia do IntroSort</h3>
          <ol>
            <li><strong>QuickSort (caso geral):</strong> Ordena a maior parte dos dados. Excelente em média (O(n log n)) e com constantes baixas — bom uso de cache.</li>
            <li><strong>HeapSort (quando o QuickSort degrada):</strong> Monitora a profundidade de recursão. Se ultrapassar <em>2 × log₂(n)</em>, troca para HeapSort, garantindo O(n log n) no pior caso e evitando O(n²) em vetores patológicos (ordenados, quase ordenados ou decrescentes).</li>
            <li><strong>Insertion Sort (subarrays pequenos):</strong> Para partições com ≤ 16 elementos, usa Insertion Sort. Em n pequeno, ele é mais rápido que QuickSort por causa da localidade de cache e do menor custo de chamadas recursivas.</li>
          </ol>
          <h3>Por que é mais rápido que nossas implementações manuais</h3>
          <ul>
            <li><strong>Código otimizado:</strong> Escrito em C++/C no runtime, com anos de otimização e ajustes para diferentes cenários.</li>
            <li><strong>Pivô inteligente:</strong> Usa "median-of-three" ou variações para reduzir partições ruins.</li>
            <li><strong>Localidade de cache:</strong> Acesso em sequência ao array melhora o uso da memória cache do processador.</li>
            <li><strong>Sem overhead de chamadas:</strong> Menos chamadas de método em comparação com implementações em C# puro.</li>
            <li><strong>JIT e otimizações:</strong> O JIT do .NET pode inline, desenrolar e otimizar as partes chamadas em C#.</li>
          </ul>
          <p><strong>Referência:</strong> A implementação do IntroSort no .NET está em <a href="https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Collections/Generic/ArraySortHelper.cs" target="_blank" rel="noopener">GitHub - dotnet/runtime ArraySortHelper.cs</a>.</p>
        </section>
        """;

    private static string GetAssymptoticAnalysisSection() => """
        <section class="analise">
          <h2>Análise Crítica: Assintótica vs Tempos Obtidos</h2>
          <p>A análise assintótica (Big-O) descreve o crescimento quando n tende ao infinito, ignorando constantes e termos de menor ordem. Os tempos reais dependem de:</p>
          <ul>
            <li><strong>Constantes ocultas:</strong> Quick Sort costuma ter constantes menores que Merge Sort; Array.Sort, por ser código nativo/JIT otimizado, possui constantes ainda menores.</li>
            <li><strong>Cache e localidade:</strong> Algoritmos com melhor localidade de referência (ex.: Insertion Sort) podem superar outros em n pequeno.</li>
            <li><strong>Cenário de entrada:</strong> Bubble melhorado e Insertion Sort são O(n) em dados ordenados; Quick Sort pode ser O(n²) em dados já ordenados com pivô fixo; Array.Sort evita isso com IntroSort.</li>
          </ul>
          <p>Os resultados empíricos devem validar a teoria: espera-se que O(n²) cresça ~100x ao aumentar n 10x, e O(n log n) cresça ~13x (10 × log 10). A medição real captura o impacto de alocação de memória, garbage collection e características do hardware, que a análise assintótica não considera.</p>
        </section>
        """;

    public static string GetDefaultPath(string tipo = "Completo") =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "RelatorioOrdenacao",
            $"Relatorio-{tipo}-{DateTime.Now:yyyyMMdd_HHmmss}.html"
        );
}
