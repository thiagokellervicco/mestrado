using AlgoritmosOrdenacao.Algoritmos;

namespace AlgoritmosOrdenacao.AlgoritmosOtimizados;

/// <summary>
/// Quick Sort definitivo: Hoare + Mediana de três + Insertion Sort para subarrays pequenos + tail recursion.
/// </summary>
public sealed class QuickSortFinal : ISortAlgorithm
{
    public string Name => "Quick Sort (Hoare + Median + Insertion)";
    private const int Cutoff = 15;

    public void Sort(int[] array)
    {
        ArgumentNullException.ThrowIfNull(array);
        SortRecursive(array, 0, array.Length - 1);
    }

    private void SortRecursive(int[] array, int low, int high)
    {
        while (low < high)
        {
            if (high - low < Cutoff)
            {
                InsertionSort(array, low, high);
                break;
            }

            int p = Partition(array, low, high);

            if (p - low < high - p)
            {
                SortRecursive(array, low, p);
                low = p + 1;
            }
            else
            {
                SortRecursive(array, p + 1, high);
                high = p;
            }
        }
    }

    private int Partition(int[] array, int low, int high)
    {
        int mid = low + (high - low) / 2;
        int pivot = MedianOfThree(array[low], array[mid], array[high]);

        int i = low - 1;
        int j = high + 1;

        while (true)
        {
            do { i++; } while (array[i] < pivot);
            do { j--; } while (array[j] > pivot);
            if (i >= j) return j;
            (array[i], array[j]) = (array[j], array[i]);
        }
    }

    private static int MedianOfThree(int a, int b, int c) =>
        (a <= b && b <= c) || (c <= b && b <= a) ? b :
        (b <= a && a <= c) || (c <= a && a <= b) ? a : c;

    private static void InsertionSort(int[] array, int low, int high)
    {
        for (int i = low + 1; i <= high; i++)
        {
            int key = array[i], j = i - 1;
            while (j >= low && array[j] > key)
            {
                array[j + 1] = array[j];
                j--;
            }
            array[j + 1] = key;
        }
    }
}
