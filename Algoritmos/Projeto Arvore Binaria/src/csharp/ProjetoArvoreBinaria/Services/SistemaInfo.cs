using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
namespace ProjetoArvoreBinaria.Services;

/// <summary>Mesmos campos que <c>coletar_info_sistema()</c> no Python (para relatório idêntico).</summary>
public static class SistemaInfo
{
    public static Dictionary<string, object?> ColetarParaRelatorio()
    {
        var os = RuntimeInformation.OSDescription;
        var arch = RuntimeInformation.OSArchitecture.ToString();
        var logicos = Environment.ProcessorCount;
        int? fisicos = null;
        string cpuModelo = "—";
        double? ramTotalGb = null;
        double? ramDispGb = null;
        double? ramUsadaGb = null;
        double? ramPercent = null;
        double? clockAtual = null, clockMin = null, clockMax = null;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            fisicos = TrySysctlInt("hw.physicalcpu");
            var brand = TrySysctl("machdep.cpu.brand_string");
            if (!string.IsNullOrEmpty(brand)) cpuModelo = brand;
            var memBytes = TrySysctlLong("hw.memsize");
            if (memBytes.HasValue) ramTotalGb = Math.Round(memBytes.Value / 1e9, 2);
            TryVmStatMac(out ramDispGb, out ramUsadaGb, out ramPercent, memBytes);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            TryLinuxMemInfo(out ramTotalGb, out ramDispGb, out ramUsadaGb, out ramPercent);
            TryLinuxCpuModel(out cpuModelo);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            cpuModelo = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? "—";
            try
            {
                var gc = GC.GetGCMemoryInfo();
                ramTotalGb = Math.Round(gc.TotalAvailableMemoryBytes / 1e9, 2);
            }
            catch { /* ignore */ }
        }

        if (ramTotalGb is null)
        {
            try
            {
                var gc = GC.GetGCMemoryInfo();
                ramTotalGb = Math.Round(gc.TotalAvailableMemoryBytes / 1e9, 2);
            }
            catch { ramTotalGb = null; }
        }

        fisicos ??= logicos;

        return new Dictionary<string, object?>
        {
            ["os"] = os,
            ["arquitetura"] = arch,
            ["cpu_modelo"] = cpuModelo,
            ["nucleos_fisicos"] = fisicos,
            ["nucleos_logicos"] = logicos,
            ["clock_mhz_atual"] = clockAtual,
            ["clock_mhz_min"] = clockMin,
            ["clock_mhz_max"] = clockMax,
            ["ram_total_gb"] = ramTotalGb,
            ["ram_disponivel_gb"] = ramDispGb,
            ["ram_usada_gb"] = ramUsadaGb,
            ["ram_percent"] = ramPercent
        };
    }

    private static string? TrySysctl(string name)
    {
        try
        {
            using var p = Process.Start(new ProcessStartInfo
            {
                FileName = "/usr/sbin/sysctl",
                Arguments = $"-n {name}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            if (p is null) return null;
            var o = p.StandardOutput.ReadToEnd().Trim();
            p.WaitForExit(2000);
            return string.IsNullOrEmpty(o) ? null : o;
        }
        catch { return null; }
    }

    private static int? TrySysctlInt(string name)
    {
        var s = TrySysctl(name);
        return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : null;
    }

    private static long? TrySysctlLong(string name)
    {
        var s = TrySysctl(name);
        return long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : null;
    }

    /// <summary>vm_stat no macOS (aproximação de páginas livres).</summary>
    private static void TryVmStatMac(out double? dispGb, out double? usadaGb, out double? pct, long? totalBytes)
    {
        dispGb = usadaGb = pct = null;
        try
        {
            using var p = Process.Start(new ProcessStartInfo
            {
                FileName = "/usr/bin/vm_stat",
                Arguments = "",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            if (p is null) return;
            var text = p.StandardOutput.ReadToEnd();
            p.WaitForExit(3000);
            var pageSize = 4096L;
            foreach (var line in text.Split('\n'))
            {
                if (line.Contains("page size of", StringComparison.Ordinal))
                {
                    var digits = new string(line.Where(char.IsDigit).ToArray());
                    if (long.TryParse(digits, out var ps)) pageSize = ps;
                    break;
                }
            }
            long freePages = 0, activePages = 0, inactivePages = 0, speculativePages = 0, wiredPages = 0;
            foreach (var line in text.Split('\n'))
            {
                var t = line.Trim();
                if (t.StartsWith("Pages free:", StringComparison.Ordinal))
                    freePages = ParseVmStatNumber(t);
                else if (t.StartsWith("Pages active:", StringComparison.Ordinal))
                    activePages = ParseVmStatNumber(t);
                else if (t.StartsWith("Pages inactive:", StringComparison.Ordinal))
                    inactivePages = ParseVmStatNumber(t);
                else if (t.StartsWith("Pages speculative:", StringComparison.Ordinal))
                    speculativePages = ParseVmStatNumber(t);
                else if (t.StartsWith("Pages wired down:", StringComparison.Ordinal))
                    wiredPages = ParseVmStatNumber(t);
            }
            var freeBytes = freePages * pageSize;
            if (totalBytes is > 0)
            {
                dispGb = Math.Round(freeBytes / 1e9, 2);
                var used = totalBytes.Value - freeBytes;
                if (used >= 0)
                {
                    usadaGb = Math.Round(used / 1e9, 2);
                    pct = Math.Round(100.0 * used / totalBytes.Value, 1);
                }
            }
        }
        catch { /* ignore */ }
    }

    private static long ParseVmStatNumber(string line)
    {
        var i = line.LastIndexOf(':');
        if (i < 0) return 0;
        var tail = line[(i + 1)..].Trim().TrimEnd('.');
        return long.TryParse(tail, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : 0;
    }

    private static void TryLinuxMemInfo(out double? totalGb, out double? dispGb, out double? usadaGb, out double? pct)
    {
        totalGb = dispGb = usadaGb = pct = null;
        try
        {
            foreach (var line in File.ReadLines("/proc/meminfo"))
            {
                if (line.StartsWith("MemTotal:", StringComparison.Ordinal))
                {
                    var kb = ParseMeminfoKb(line);
                    if (kb.HasValue) totalGb = Math.Round(kb.Value / 1e6, 2);
                }
                else if (line.StartsWith("MemAvailable:", StringComparison.Ordinal))
                {
                    var kb = ParseMeminfoKb(line);
                    if (kb.HasValue) dispGb = Math.Round(kb.Value / 1e6, 2);
                }
            }
            if (totalGb is not null && dispGb is not null)
            {
                usadaGb = Math.Round(totalGb.Value - dispGb.Value, 2);
                pct = Math.Round(100.0 * usadaGb.Value / totalGb.Value, 1);
            }
        }
        catch { /* ignore */ }
    }

    private static long? ParseMeminfoKb(string line)
    {
        var parts = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2 && long.TryParse(parts[1], out var kb) ? kb : null;
    }

    private static void TryLinuxCpuModel(out string modelo)
    {
        modelo = "—";
        try
        {
            foreach (var line in File.ReadLines("/proc/cpuinfo"))
            {
                if (line.StartsWith("model name", StringComparison.Ordinal))
                {
                    var i = line.IndexOf(':');
                    if (i >= 0) modelo = line[(i + 1)..].Trim();
                    return;
                }
            }
        }
        catch { /* ignore */ }
    }

    public static Dictionary<string, object?> Imprimir()
    {
        var s = ColetarParaRelatorio();
        Console.WriteLine("SISTEMA");
        Console.WriteLine(new string('=', 40));
        Console.WriteLine($"OS:           {s["os"]}");
        Console.WriteLine($"Arquitetura:  {s["arquitetura"]}");
        Console.WriteLine();
        Console.WriteLine(new string('=', 40));
        Console.WriteLine("PROCESSADOR");
        Console.WriteLine(new string('=', 40));
        Console.WriteLine($"Modelo:       {s["cpu_modelo"]}");
        Console.WriteLine($"Núcleos físicos:  {s["nucleos_fisicos"]}");
        Console.WriteLine($"Núcleos lógicos:  {s["nucleos_logicos"]}");
        if (s["clock_mhz_atual"] is double)
        {
            Console.WriteLine($"Clock atual:  {Convert.ToDouble(s["clock_mhz_atual"]):F0} MHz");
            Console.WriteLine($"Clock mín:    {Convert.ToDouble(s["clock_mhz_min"]):F0} MHz");
            Console.WriteLine($"Clock máx:    {Convert.ToDouble(s["clock_mhz_max"]):F0} MHz");
        }
        else
            Console.WriteLine("Frequência CPU: (não disponível neste sistema)");
        Console.WriteLine();
        Console.WriteLine(new string('=', 40));
        Console.WriteLine("MEMÓRIA RAM");
        Console.WriteLine(new string('=', 40));
        FmtRam(s);
        return s;
    }

    private static void FmtRam(Dictionary<string, object?> s)
    {
        var inv = CultureInfo.InvariantCulture;
        if (s["ram_total_gb"] is double rt)
            Console.WriteLine($"Total:        {rt.ToString("F2", inv)} GB");
        else
            Console.WriteLine("Total:        n/d");
        if (s["ram_disponivel_gb"] is double rd)
            Console.WriteLine($"Disponível:   {rd.ToString("F2", inv)} GB");
        else
            Console.WriteLine("Disponível:   n/d");
        if (s["ram_usada_gb"] is double ru)
            Console.WriteLine($"Usada:        {ru.ToString("F2", inv)} GB");
        else
            Console.WriteLine("Usada:        n/d");
        if (s["ram_percent"] is double rp)
            Console.WriteLine($"Uso (%):      {rp.ToString("F1", inv)}%");
        else
            Console.WriteLine("Uso (%):      n/d");
    }
}
