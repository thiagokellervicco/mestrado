using AlgoritmosOrdenacao.Algoritmos;

namespace AlgoritmosOrdenacao.AlgoritmosOtimizados;

/// <summary>
/// Merge Sort com array auxiliar único: aloca memória uma só vez para todo o processo.
/// </summary>
public sealed class MergeSortOptimized : ISortAlgorithm
{
    public string Name => "Merge Sort (Single Aux Array)";

    public void Sort(int[] array)
    {
        ArgumentNullException.ThrowIfNull(array);
        int[] aux = new int[array.Length];
        SortRecursive(array, aux, 0, array.Length - 1);
    }

    private void SortRecursive(int[] array, int[] aux, int low, int high)
    {
        if (low >= high) return;

        int mid = low + (high - low) / 2;
        SortRecursive(array, aux, low, mid);
        SortRecursive(array, aux, mid + 1, high);

        Merge(array, aux, low, mid, high);
    }

    private void Merge(int[] array, int[] aux, int low, int mid, int high)
    {
        Array.Copy(array, low, aux, low, high - low + 1);

        int i = low;
        int j = mid + 1;

        for (int k = low; k <= high; k++)
        {
            if (i > mid) array[k] = aux[j++];
            else if (j > high) array[k] = aux[i++];
            else if (aux[j] < aux[i]) array[k] = aux[j++];
            else array[k] = aux[i++];
        }
    }
}
