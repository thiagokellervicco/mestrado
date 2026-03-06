namespace AlgoritmosOrdenacao.Algoritmos;

/// <summary>
/// Ordenação por inserção.
/// Complexidade: O(n²) no pior caso, O(n) no melhor caso (já ordenado).
/// </summary>
public sealed class InsertionSort : ISortAlgorithm
{
    public string Name => "Insertion Sort";

    public void Sort(int[] array)
    {
        ArgumentNullException.ThrowIfNull(array);
        var n = array.Length;

        for (var i = 1; i < n; i++)
        {
            var key = array[i];
            var j = i - 1;
            while (j >= 0 && array[j] > key)
            {
                array[j + 1] = array[j];
                j--;
            }
            array[j + 1] = key;
        }
    }
}
