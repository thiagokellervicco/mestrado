using AlgoritmosOrdenacao.Algoritmos;

namespace AlgoritmosOrdenacao.AlgoritmosOtimizados;

/// <summary>
/// Bubble Sort com early exit: para assim que o array estiver ordenado.
/// Complexidade: O(n²) pior caso, O(n) melhor caso (já ordenado).
/// </summary>
public sealed class BubbleSortOptimized : ISortAlgorithm
{
    public string Name => "Bubble Sort (Optimized)";

    public void Sort(int[] array)
    {
        ArgumentNullException.ThrowIfNull(array);
        int n = array.Length;
        for (int i = 0; i < n - 1; i++)
        {
            bool swapped = false;
            for (int j = 0; j < n - i - 1; j++)
            {
                if (array[j] > array[j + 1])
                {
                    (array[j], array[j + 1]) = (array[j + 1], array[j]);
                    swapped = true;
                }
            }
            if (!swapped) break;
        }
    }
}
