namespace AlgoritmosOrdenacao.Algoritmos;

/// <summary>
/// Ordenação por bolha (versão original).
/// Complexidade: O(n²) no pior caso, O(n²) no melhor caso.
/// </summary>
public sealed class BubbleSort : ISortAlgorithm
{
    public string Name => "Bubble Sort (Original)";

    public void Sort(int[] array)
    {
        ArgumentNullException.ThrowIfNull(array);
        var n = array.Length;

        for (var i = 0; i < n - 1; i++)
        {
            for (var j = 0; j < n - 1 - i; j++)
            {
                if (array[j] > array[j + 1])
                {
                    (array[j], array[j + 1]) = (array[j + 1], array[j]);
                }
            }
        }
    }
}
