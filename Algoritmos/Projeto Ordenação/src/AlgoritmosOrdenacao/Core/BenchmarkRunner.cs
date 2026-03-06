using System.Diagnostics;
using AlgoritmosOrdenacao.Algoritmos;
using AlgoritmosOrdenacao.AlgoritmosOtimizados;

namespace AlgoritmosOrdenacao.Core;

/// <summary>
/// Executa benchmarks de algoritmos de ordenação.
/// Sempre utiliza novo espaço de memória para cada execução (cópia do array).
/// </summary>
public sealed class BenchmarkRunner(bool ultraMode = false, bool optimizedMode = false, bool demoMode = false)
{
    private const int IterationsPerCase = 5;

    private static readonly int[] FullSizes = [1_000, 10_000, 100_000];

    private static readonly int[] UltraSizes = [1_000, 10_000, 100_000, 10_000_000];

    private static readonly int[] DemoSizes = [10];

    private static readonly VectorType[] VectorTypes = [VectorType.Random, VectorType.Ascending, VectorType.Descending];

    private static readonly IReadOnlyList<ISortAlgorithm> StandardAlgorithms =
    [
        new BubbleSort(),
        new SelectionSort(),
        new InsertionSort(),
        new MergeSort(),
        new QuickSort(),
        new QuickSortRandomPivot(),
        new ArraySortNative()
    ];

    private static readonly IReadOnlyList<ISortAlgorithm> OptimizedAlgorithms =
    [
        new BubbleSortOptimized(),
        new InsertionSortOptimized(),
        new SelectionSortOptimized(),
        new MergeSortOptimized(),
        new QuickSortFinal()
    ];

    private readonly IReadOnlyList<ISortAlgorithm> _algorithms = optimizedMode
        ? [.. StandardAlgorithms, .. OptimizedAlgorithms]
        : StandardAlgorithms;

    /// <summary>
    /// Executa todos os benchmarks. Cada algoritmo recebe uma cópia do array em memória.
    /// </summary>
    public IReadOnlyList<BenchmarkResult> Run()
    {
        var results = new List<BenchmarkResult>();
        
        var sizes = demoMode ? DemoSizes : (ultraMode ? UltraSizes : FullSizes);

        foreach (var size in sizes)
        {
            foreach (var type in VectorTypes)
            {
                var originalArray = DataGenerator.Generate(size, type);
                var isFirstInGroup = true;

                foreach (var algorithm in _algorithms)
                {
                    int iterations = IterationsPerCase;
                    
                    var times = new List<TimeSpan>();

                    var memoryBytes = new List<long>();

                    // Executa uma vez fora do cronômetro para o JIT compilar o método
                    algorithm.Sort((int[])originalArray.Clone());

                    int[]? sampleOutput = null;

                    for (int i = 0; i < iterations; i++)
                    {
                        // Sempre aloca novo espaço de memória - cópia do array original
                        var copy = new int[size];
                        Array.Copy(originalArray, copy, size);

                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        GC.Collect();

                        //Pega a memoria que esta alocada para ao inicio da execução
                        var memBefore = GC.GetTotalAllocatedBytes(true);
                        
                        // Executa propriamente a ordenação, o start do tempo é somente nessa parte
                        var sw = Stopwatch.StartNew();
                        algorithm.Sort(copy);
                        sw.Stop();
                        
                        //Pega a memoria que esta alocada ao final da execução
                        var memAfter = GC.GetTotalAllocatedBytes(true);

                        // Captura saída no modo demo (após primeira ordenação)
                        if (demoMode && sampleOutput == null)
                            sampleOutput = (int[])copy.Clone();

                        //adiciona os valores para posteriormente utilizar
                        times.Add(sw.Elapsed);
                        memoryBytes.Add(Math.Max(0, memAfter - memBefore));
                    }

                    //Ordena para pegar a mediana de tempo e memoria
                    times.Sort();
                    memoryBytes.Sort();
                    
                    var median = times[iterations / 2];
                    
                    var memMedian = memoryBytes[iterations / 2];
                    
                    var sampleInput = demoMode && isFirstInGroup ? (int[])originalArray.Clone() : null;
                    isFirstInGroup = false;

                    results.Add(new BenchmarkResult(
                        algorithm.Name,
                        type,
                        size,
                        median,
                        times.Min(),
                        times.Max(),
                        iterations,
                        memMedian,
                        sampleInput,
                        sampleOutput
                    ));
                }
            }
        }

        return results;
    }
}
