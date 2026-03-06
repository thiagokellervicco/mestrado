namespace AlgoritmosOrdenacao.Core;

/// <summary>
/// Resultado de uma execução de benchmark.
/// </summary>
public record BenchmarkResult(
    string Algorithm,
    VectorType VectorType,
    int Size,
    TimeSpan AverageTime,
    TimeSpan MinTime,
    TimeSpan MaxTime,
    int Iterations,
    long MemoryBytesMedian = 0,
    int[]? SampleInput = null,
    int[]? SampleOutput = null
);
