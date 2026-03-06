namespace AlgoritmosOrdenacao.Algoritmos;

/// <summary>
/// Ordenação por intercalação (Merge Sort).
/// Complexidade: O(n log n) em todos os casos.
/// </summary>
public sealed class MergeSort : ISortAlgorithm
{
    public string Name => "Merge Sort";

    public void Sort(int[] array)
    {
        ArgumentNullException.ThrowIfNull(array);
        if (array.Length <= 1)
            return;
        SortRecursive(array, 0, array.Length - 1);
    }

    private static void SortRecursive(int[] array, int start, int end)
    {
        if (start >= end)
            return;

        var mid = start + (end - start) / 2;
        SortRecursive(array, start, mid);
        SortRecursive(array, mid + 1, end);
        Merge(array, start, mid, end);
    }

    private static void Merge(int[] array, int start, int mid, int end)
    {
        var n1 = mid - start + 1;
        var n2 = end - mid;

        var left = new int[n1];
        var right = new int[n2];

        Array.Copy(array, start, left, 0, n1);
        Array.Copy(array, mid + 1, right, 0, n2);

        int i = 0, j = 0, k = start;
        while (i < n1 && j < n2)
        {
            if (left[i] <= right[j])
            {
                array[k] = left[i];
                i++;
            }
            else
            {
                array[k] = right[j];
                j++;
            }
            k++;
        }

        while (i < n1)
        {
            array[k] = left[i];
            i++;
            k++;
        }

        while (j < n2)
        {
            array[k] = right[j];
            j++;
            k++;
        }
    }
}
