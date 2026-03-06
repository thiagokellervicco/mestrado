namespace AlgoritmosOrdenacao.Algoritmos;

/// <summary>
/// Interface para algoritmos de ordenação.
/// </summary>
public interface ISortAlgorithm
{
    string Name { get; }
    void Sort(int[] array);
}
