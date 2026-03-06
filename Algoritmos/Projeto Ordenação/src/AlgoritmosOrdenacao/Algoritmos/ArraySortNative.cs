namespace AlgoritmosOrdenacao.Algoritmos;

/// <summary>
/// Ordenação nativa do C# (Array.Sort).
/// Utiliza IntroSort: híbrido de QuickSort, HeapSort e InsertionSort.
/// Complexidade: O(n log n) garantido no pior caso.
/// Implementação .NET; não está no Cormen.
/// </summary>
public sealed class ArraySortNative : ISortAlgorithm
{
    public string Name => "Array.Sort (C# Nativo)";

    public void Sort(int[] array)
    {
        ArgumentNullException.ThrowIfNull(array);
        Array.Sort(array);
    }
}
