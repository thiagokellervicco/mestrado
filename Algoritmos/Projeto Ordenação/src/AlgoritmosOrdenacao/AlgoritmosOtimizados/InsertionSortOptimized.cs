using AlgoritmosOrdenacao.Algoritmos;

namespace AlgoritmosOrdenacao.AlgoritmosOtimizados;

/// <summary>
/// Insertion Sort com sentinela: menor elemento na posição 0 elimina checagem j >= 0.
/// </summary>
public sealed class InsertionSortOptimized : ISortAlgorithm
{
    public string Name => "Insertion Sort (Sentinel)";

    public void Sort(int[] array)
    {
        ArgumentNullException.ThrowIfNull(array);
        int n = array.Length;
        if (n <= 1) return;

        // Passo 1: Coloca o menor elemento na primeira posição (Sentinela)
        int minIndex = 0;
        for (int i = 1; i < n; i++)
            if (array[i] < array[minIndex]) minIndex = i;

        (array[0], array[minIndex]) = (array[minIndex], array[0]);

        // Passo 2: O loop interno agora não precisa checar 'j >= 0'
        for (int i = 2; i < n; i++)
        {
            int key = array[i];
            int j = i - 1;
            while (array[j] > key)
            {
                array[j + 1] = array[j];
                j--;
            }
            array[j + 1] = key;
        }
    }
}
