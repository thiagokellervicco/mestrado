using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AlgoritmosOrdenacao.Relatorio;

/// <summary>
/// Coleta informações sobre a máquina de execução.
/// </summary>
public static class MachineInfo
{
    /// <summary>
    /// Informações estáticas da máquina (não variam entre execuções).
    /// Usado na página index.
    /// </summary>
    public static string CollectStaticDescription()
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine($"• Nome da máquina: {Environment.MachineName}");
        sb.AppendLine($"• Processadores (lógicos): {Environment.ProcessorCount}");
        sb.AppendLine($"• Sistema 64-bit: {Environment.Is64BitOperatingSystem}");
        sb.AppendLine($"• SO: {RuntimeInformation.OSDescription}");
        sb.AppendLine($"• Arquitetura do SO: {RuntimeInformation.OSArchitecture}");
        sb.AppendLine($"• Arquitetura do processo: {RuntimeInformation.ProcessArchitecture}");
        sb.AppendLine($"• .NET: {RuntimeInformation.FrameworkDescription}");
        sb.AppendLine($"• Versão: {Environment.Version}");

        var memory = GetTotalMemory();
        if (!string.IsNullOrEmpty(memory))
            sb.AppendLine($"• Memória RAM total: {memory}");

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Informações específicas da execução (variam a cada relatório).
    /// Fonte de energia, memória GC e heap no momento da execução.
    /// </summary>
    public static string CollectRuntimeDescription()
    {
        var sb = new System.Text.StringBuilder();

        var powerSource = GetPowerSource();
        if (!string.IsNullOrEmpty(powerSource))
            sb.AppendLine($"• Fonte de alimentação: {powerSource}");

        var gc = GC.GetGCMemoryInfo();
        var heapSizeMB = gc.HeapSizeBytes / (1024.0 * 1024.0);
        var totalAvailableMB = gc.TotalAvailableMemoryBytes / (1024.0 * 1024.0);
        sb.AppendLine($"• Memória disponível para GC: {totalAvailableMB:F1} MB");
        sb.AppendLine($"• Tamanho do heap GC: {heapSizeMB:F1} MB");

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Todas as informações (estáticas + runtime).
    /// </summary>
    public static string CollectDescription()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine(CollectStaticDescription());
        var runtime = CollectRuntimeDescription();
        if (!string.IsNullOrEmpty(runtime))
            sb.AppendLine().AppendLine(runtime);
        return sb.ToString().TrimEnd();
    }

    private static string? GetTotalMemory()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var bytes = ExecuteCommand("sysctl", "-n", "hw.memsize");
                if (long.TryParse(bytes?.Trim(), out var memBytes) && memBytes > 0)
                    return $"{memBytes / (1024.0 * 1024.0 * 1024.0):F1} GB";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var memInfo = File.ReadAllText("/proc/meminfo");
                var line = memInfo.Split('\n').FirstOrDefault(l => l.StartsWith("MemTotal:", StringComparison.Ordinal));
                if (line != null)
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2 && long.TryParse(parts[1], out var kb))
                        return $"{kb / 1024.0 / 1024.0:F1} GB";
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var result = ExecuteCommand("wmic", "computersystem", "get", "TotalPhysicalMemory");
                if (!string.IsNullOrEmpty(result))
                {
                    var value = result.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                        .Select(l => l.Trim())
                        .FirstOrDefault(l => long.TryParse(l, out _));
                    if (value != null && long.TryParse(value, out var bytes) && bytes > 0)
                        return $"{bytes / (1024.0 * 1024.0 * 1024.0):F1} GB";
                }
            }
        }
        catch
        {
            // Ignorar falhas ao obter memória
        }

        return null;
    }

    private static string? GetPowerSource()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var result = ExecuteCommand("pmset", "-g", "batt");
                if (!string.IsNullOrEmpty(result))
                {
                    if (result.Contains("AC Power", StringComparison.OrdinalIgnoreCase))
                        return "Conectado na tomada (AC)";
                    if (result.Contains("Battery Power", StringComparison.OrdinalIgnoreCase))
                        return "Bateria";
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var result = ExecuteCommand("powercfg", "/query", "SCHEME_CURRENT");
                if (!string.IsNullOrEmpty(result) && result.Contains("DC", StringComparison.Ordinal))
                    return "Bateria";
            }
        }
        catch
        {
            // Ignorar falhas
        }

        return null;
    }

    private static string? ExecuteCommand(string fileName, params string[] arguments)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            foreach (var arg in arguments)
                startInfo.ArgumentList.Add(arg);

            using var proc = new Process { StartInfo = startInfo };
            proc.Start();
            return proc.StandardOutput.ReadToEnd().Trim();
        }
        catch
        {
            return null;
        }
    }
}
