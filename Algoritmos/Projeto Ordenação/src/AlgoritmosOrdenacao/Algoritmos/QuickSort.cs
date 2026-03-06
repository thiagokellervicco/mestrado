namespace AlgoritmosOrdenacao.Algoritmos;

/// <summary>
/// Ordenação rápida (Quick Sort).
/// Complexidade: O(n log n) médio, O(n²) no pior caso.
/// </summary>
public sealed class QuickSort : ISortAlgorithm
{
    public string Name => "Quick Sort";

    public void Sort(int[] array)
    {
        ArgumentNullException.ThrowIfNull(array);
        if (array.Length <= 1)
            return;
        SortRecursive(array, 0, array.Length - 1);
    }

    private static void SortRecursive(int[] array, int low, int high)
    {
        if (low < high)
        {
            var pivot = Partition(array, low, high);
            SortRecursive(array, low, pivot - 1);
            SortRecursive(array, pivot + 1, high);
        }
    }

    private static int Partition(int[] array, int low, int high)
    {
        var pivot = array[high];
        var i = low - 1;

        for (var j = low; j < high; j++)
        {
            if (array[j] <= pivot)
            {
                i++;
                (array[i], array[j]) = (array[j], array[i]);
            }
        }
        (array[i + 1], array[high]) = (array[high], array[i + 1]);
        return i + 1;
    }
}
