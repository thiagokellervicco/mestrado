namespace AlgoritmosOrdenacao.Algoritmos;

/// <summary>
/// Ordenação rápida (Quick Sort) com pivô aleatório.
/// Reduz a probabilidade de O(n²) em dados ordenados ou decrescentes.
/// Complexidade: O(n log n) esperado em todos os casos.
/// </summary>
public sealed class QuickSortRandomPivot : ISortAlgorithm
{
    private static readonly Random Rng = Random.Shared;

    public string Name => "Quick Sort (Pivô Aleatório)";

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
            // Escolhe pivô aleatório e troca com o último elemento
            var pivotIndex = Rng.Next(low, high + 1);

            (array[pivotIndex], array[high]) = (array[high], array[pivotIndex]);

            var pivot = Partition(array, low, high);
            
            SortRecursive(array, low, pivot - 1);
            
            SortRecursive(array, pivot + 1, high);
        }
    }

    private static int Partition(int[] array, int low, int high)
    {
        var pivotValue = array[high];
        var i = low - 1;

        for (var j = low; j < high; j++)
        {
            if (array[j] <= pivotValue)
            {
                i++;
                (array[i], array[j]) = (array[j], array[i]);
            }
        }
        (array[i + 1], array[high]) = (array[high], array[i + 1]);
        return i + 1;
    }
}
