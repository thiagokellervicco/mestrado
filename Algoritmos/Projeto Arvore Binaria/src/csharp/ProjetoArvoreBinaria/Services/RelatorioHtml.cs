using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ProjetoArvoreBinaria.Services;

/// <summary>Relatório em HTML (paridade com <c>relatorio.py</c>).</summary>
public static class RelatorioHtml
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    private static string H(string? s) => WebUtility.HtmlEncode(s ?? "");

    private static string HtmlTable(IReadOnlyList<string> headers, IEnumerable<IEnumerable<string>> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<table class=\"data\">");
        sb.Append("  <thead><tr>");
        foreach (var h in headers) sb.Append("<th>").Append(H(h)).Append("</th>");
        sb.AppendLine("</tr></thead>");
        sb.AppendLine("  <tbody>");
        foreach (var row in rows)
        {
            sb.Append("    <tr>");
            foreach (var c in row) sb.Append("<td>").Append(H(c)).Append("</td>");
            sb.AppendLine("</tr>");
        }
        sb.AppendLine("  </tbody>");
        sb.AppendLine("</table>");
        return sb.ToString();
    }

    private static string Str(object? v)
    {
        if (v is null) return "";
        if (v is double d) return d.ToString(Inv);
        if (v is float f) return f.ToString(Inv);
        if (v is IFormattable fmt) return fmt.ToString(null, Inv);
        return v.ToString() ?? "";
    }

    private static string FormatarDeltaMs(double delta)
    {
        var s = Math.Abs(delta).ToString("F3", Inv);
        return (delta >= 0 ? "+" : "-") + s;
    }

    private static string RamTresValores(IReadOnlyDictionary<string, object?> sistema)
    {
        var t = sistema.GetValueOrDefault("ram_total_gb");
        var d = sistema.GetValueOrDefault("ram_disponivel_gb");
        var p = sistema.GetValueOrDefault("ram_percent");
        if (t is double tg && d is double dg && p is double pc)
            return $"{tg.ToString("F2", Inv)} GB / {dg.ToString("F2", Inv)} GB / {pc.ToString("F1", Inv)}%";
        if (t is double tg2)
            return $"{tg2.ToString("F2", Inv)} GB / n/d / n/d";
        return "n/d / n/d / n/d";
    }

    public static string MontarDocumento(
        string arquivoDados,
        int nLinhas,
        IReadOnlyDictionary<string, object?> sistema,
        BenchmarkResult bench,
        string linguagem)
    {
        var agora = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", Inv);
        var nLinhasFmt = nLinhas.ToString("N0", PtBr);

        var clockAtual = sistema.GetValueOrDefault("clock_mhz_atual");
        string clockTxt;
        if (clockAtual is double ca && sistema.GetValueOrDefault("clock_mhz_min") is double cmin
            && sistema.GetValueOrDefault("clock_mhz_max") is double cmax)
            clockTxt = $"{ca:F0} / {cmin:F0} / {cmax:F0} MHz";
        else
            clockTxt = "n/d";

        var secAmbiente = HtmlTable(
            ["Campo", "Valor"],
            new[]
            {
                new[] { "Sistema operacional", Str(sistema.GetValueOrDefault("os")) },
                new[] { "Arquitetura", Str(sistema.GetValueOrDefault("arquitetura")) },
                new[] { "CPU", Str(sistema.GetValueOrDefault("cpu_modelo")) },
                new[]
                {
                    "Núcleos físicos / lógicos",
                    $"{Str(sistema.GetValueOrDefault("nucleos_fisicos"))} / {Str(sistema.GetValueOrDefault("nucleos_logicos"))}"
                },
                new[] { "Clock atual / mín / máx", clockTxt },
                new[] { "RAM total / disponível / uso %", RamTresValores(sistema) }
            });

        var secEstrutura = HtmlTable(
            ["Métrica", "ABB", "AVL"],
            new[]
            {
                new[] { "Total de nós", bench.TotalAbb.ToString(Inv), bench.TotalAvl.ToString(Inv) },
                new[] { "Altura", bench.AlturaAbb.ToString(Inv), bench.AlturaAvl.ToString(Inv) },
                new[] { "ID mínimo", bench.IdMinAbb.ToString(Inv), bench.IdMinAvl.ToString(Inv) },
                new[] { "ID máximo", bench.IdMaxAbb.ToString(Inv), bench.IdMaxAvl.ToString(Inv) }
            });

        var vIns = bench.TInsAbbS < bench.TInsAvlS ? "ABB" : (bench.TInsAvlS < bench.TInsAbbS ? "AVL" : "Empate");
        var secInsercao = HtmlTable(
            ["", "ABB (ms)", "AVL (ms)", "Δ (ms)", "Vencedor"],
            new[]
            {
                new[]
                {
                    "Inserção (todos os registros)",
                    (bench.TInsAbbS * 1000).ToString("F3", Inv),
                    (bench.TInsAvlS * 1000).ToString("F3", Inv),
                    FormatarDeltaMs((bench.TInsAvlS - bench.TInsAbbS) * 1000),
                    vIns
                }
            });

        var secOps = HtmlTable(
            ["Operação", "ABB (ms)", "AVL (ms)", "Vencedor"],
            bench.Operacoes.Select(o => new[] { o.Nome, o.MsAbb.ToString("F4", Inv), o.MsAvl.ToString("F4", Inv), o.Vencedor }));

        var secRem = HtmlTable(
            ["Operação", "ABB (ms)", "AVL (ms)", "Vencedor"],
            new[]
            {
                new[]
                {
                    "Remover 5 nós (+ reconstrução)",
                    bench.RemocaoMsAbb.ToString("F3", Inv),
                    bench.RemocaoMsAvl.ToString("F3", Inv),
                    bench.RemocaoVencedor
                }
            });

        const string css = """
    :root { --bg:#0f1219; --card:#161b26; --text:#e6e9ef; --muted:#8b96a8; --accent:#5eead4; --border:#2a3344; }
    * { box-sizing: border-box; }
    body { font-family: system-ui, Segoe UI, sans-serif; background: var(--bg); color: var(--text); margin: 0; padding: 2rem; line-height: 1.5; }
    .wrap { max-width: 960px; margin: 0 auto; }
    h1 { font-size: 1.5rem; font-weight: 700; margin-bottom: 0.5rem; border-bottom: 1px solid var(--border); padding-bottom: 0.75rem; }
    .meta { color: var(--muted); font-size: 0.95rem; margin: 0.35rem 0; }
    .meta strong { color: var(--accent); }
    code { font-size: 0.88em; background: rgba(255,255,255,0.06); padding: 0.15rem 0.4rem; border-radius: 4px; }
    h2 { font-size: 1.1rem; margin-top: 2rem; margin-bottom: 0.75rem; color: var(--accent); }
    table.data { width: 100%; border-collapse: collapse; font-size: 0.9rem; background: var(--card); border: 1px solid var(--border); border-radius: 8px; overflow: hidden; }
    table.data th, table.data td { text-align: left; padding: 0.55rem 0.75rem; border-bottom: 1px solid var(--border); }
    table.data th { background: rgba(94,234,212,0.08); color: var(--accent); font-weight: 600; }
    table.data tr:last-child td { border-bottom: none; }
    table.data tbody tr:hover td { background: rgba(255,255,255,0.03); }
    footer { margin-top: 2.5rem; padding-top: 1rem; border-top: 1px solid var(--border); color: var(--muted); font-size: 0.85rem; }
    """;

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"pt-BR\">");
        sb.AppendLine("<head>");
        sb.AppendLine("  <meta charset=\"utf-8\">");
        sb.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
        sb.Append("  <title>Relatório ABB vs AVL — ").Append(H(linguagem)).AppendLine("</title>");
        sb.Append("  <style>").Append(css).AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("  <div class=\"wrap\">");
        sb.AppendLine("    <h1>Relatório — ABB vs AVL (desempenho de estudantes)</h1>");
        sb.AppendLine("    <p class=\"meta\"><strong>Autores:</strong> Thiago Keller e Ricardo Diniz (trabalho em dupla)</p>");
        sb.Append("    <p class=\"meta\"><strong>Linguagem:</strong> ").Append(H(linguagem)).AppendLine("</p>");
        sb.Append("    <p class=\"meta\"><strong>Data/hora da execução:</strong> ").Append(H(agora)).AppendLine("</p>");
        sb.Append("    <p class=\"meta\"><strong>Arquivo de dados:</strong> <code>").Append(H(arquivoDados)).AppendLine("</code></p>");
        sb.Append("    <p class=\"meta\"><strong>Linhas no dataset:</strong> ").Append(H(nLinhasFmt)).AppendLine("</p>");
        sb.AppendLine("    <h2>Ambiente</h2>");
        sb.AppendLine(secAmbiente);
        sb.AppendLine("    <h2>Estrutura das árvores</h2>");
        sb.AppendLine(secEstrutura);
        sb.AppendLine("    <h2>Tempos de inserção (carga completa)</h2>");
        sb.AppendLine(secInsercao);
        sb.AppendLine("    <h2>Tempo médio das operações (ms, média de 5 execuções)</h2>");
        sb.AppendLine(secOps);
        sb.AppendLine("    <h2>Remoção (5 nós + reconstrução, média de 5)</h2>");
        sb.AppendLine(secRem);
        sb.AppendLine("    <footer>Relatório gerado automaticamente pelo programa. · Thiago Keller e Ricardo Diniz</footer>");
        sb.AppendLine("  </div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");
        return sb.ToString();
    }

    private static readonly Regex RxLinguagem = new(
        @"<strong>Linguagem:</strong>\s*([^<]+)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(200));

    private static readonly Regex RxNomeRelatorio = new(
        @"^relatorio_(\d{8})_(\d{6})\.html$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(200));

    private static string RotuloDoArquivo(string nome)
    {
        var m = RxNomeRelatorio.Match(nome);
        if (!m.Success) return nome;
        var d = m.Groups[1].Value;
        var t = m.Groups[2].Value;
        return $"{d[..4]}-{d.Substring(4, 2)}-{d.Substring(6, 2)} {t[..2]}:{t.Substring(2, 2)}:{t.Substring(4, 2)}";
    }

    private static string LinguagemDoRelatorio(string caminho)
    {
        try
        {
            var bytes = File.ReadAllBytes(caminho);
            var n = Math.Min(16384, bytes.Length);
            var s = Encoding.UTF8.GetString(bytes.AsSpan(0, n));
            var m = RxLinguagem.Match(s);
            return m.Success ? m.Groups[1].Value.Trim() : "?";
        }
        catch
        {
            return "?";
        }
    }

    /// <summary>Atualiza <c>reports_data.js</c> para a página de listagem de relatórios.</summary>
    public static void RegenerarIndiceRelatorios(string pastaReports)
    {
        Directory.CreateDirectory(pastaReports);
        var nomes = Directory.GetFiles(pastaReports, "relatorio_*.html")
            .Select(Path.GetFileName)
            .Where(f => f is not null
                        && !string.Equals(f, "relatorio_latest.html", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(f => f, StringComparer.Ordinal)
            .ToList();
        var itens = new List<Dictionary<string, string>>();
        foreach (var nome in nomes)
        {
            var path = Path.Combine(pastaReports, nome!);
            itens.Add(new Dictionary<string, string>
            {
                ["file"] = nome!,
                ["language"] = LinguagemDoRelatorio(path),
                ["label"] = RotuloDoArquivo(nome!),
            });
        }
        var json = JsonSerializer.Serialize(itens, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        });
        var js = "// Auto-gerado ao executar Python ou C#. Não editar.\nwindow.PROJETO_ARVORE_RELATORIOS = " + json + ";\n";
        File.WriteAllText(Path.Combine(pastaReports, "reports_data.js"), js, new UTF8Encoding(false));
    }

    public static void EscreverExecucao(string raizRepo, string arquivoDados, int nLinhas,
        IReadOnlyDictionary<string, object?> sistema, BenchmarkResult bench, string linguagem = "C#")
    {
        var pasta = Path.Combine(raizRepo, "reports");
        Directory.CreateDirectory(pasta);
        var html = MontarDocumento(arquivoDados, nLinhas, sistema, bench, linguagem);
        var ts = DateTime.Now.ToString("yyyyMMdd_HHmmss", Inv);
        var p1 = Path.Combine(pasta, $"relatorio_{ts}.html");
        var p2 = Path.Combine(pasta, "relatorio_latest.html");
        File.WriteAllText(p1, html, new UTF8Encoding(false));
        File.WriteAllText(p2, html, new UTF8Encoding(false));
        RegenerarIndiceRelatorios(pasta);
        Console.WriteLine();
        Console.WriteLine(new string('=', 70));
        Console.WriteLine("  Relatório HTML:");
        Console.WriteLine($"    • {p1}");
        Console.WriteLine($"    • {p2}");
        Console.WriteLine(new string('=', 70));
    }
}
