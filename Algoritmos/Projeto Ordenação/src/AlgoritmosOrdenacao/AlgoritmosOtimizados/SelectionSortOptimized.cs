using AlgoritmosOrdenacao.Algoritmos;

namespace AlgoritmosOrdenacao.AlgoritmosOtimizados;

/// <summary>
/// Selection Sort com dupla seleção: busca menor e maior em cada passagem,
/// reduzindo iterações do loop externo pela metade.
/// </summary>
public sealed class SelectionSortOptimized : ISortAlgorithm
{
    public string Name => "Selection Sort (Double)";

    public void Sort(int[] array)
    {
        ArgumentNullException.ThrowIfNull(array);
        int left = 0;
        int right = array.Length - 1;

        while (left < right)
        {
            int min = left;
            int max = left;

            for (int i = left + 1; i <= right; i++)
            {
                if (array[i] < array[min]) min = i;
                else if (array[i] > array[max]) max = i;
            }

            (array[left], array[min]) = (array[min], array[left]);

            if (max == left) max = min;

            (array[right], array[max]) = (array[max], array[right]);

            left++;
            right--;
        }
    }
}
