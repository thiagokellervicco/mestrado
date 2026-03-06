namespace AlgoritmosOrdenacao.Algoritmos;

/// <summary>
/// Ordenação por seleção.
/// Complexidade: O(n²) em todos os casos.
/// </summary>
public sealed class SelectionSort : ISortAlgorithm
{
    public string Name => "Selection Sort";

    public void Sort(int[] array)
    {
        ArgumentNullException.ThrowIfNull(array);
        var n = array.Length;

        for (var i = 0; i < n - 1; i++)
        {
            var minIdx = i;
            for (var j = i + 1; j < n; j++)
            {
                if (array[j] < array[minIdx])
                    minIdx = j;
            }
            if (minIdx != i)
                (array[i], array[minIdx]) = (array[minIdx], array[i]);
        }
    }
}
