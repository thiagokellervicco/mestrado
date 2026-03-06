namespace AlgoritmosOrdenacao.Core;

/// <summary>
/// Gera vetores para benchmark com diferentes configurações.
/// </summary>
public static class DataGenerator
{
    private static readonly Random Rng = Random.Shared;

    /// <summary>
    /// Gera um novo array (novo espaço de memória) conforme o tipo especificado.
    /// </summary>
    public static int[] Generate(int size, VectorType type)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size);
        var array = new int[size];

        switch (type)
        {
            case VectorType.Random:
                for (int i = 0; i < size; i++)
                    array[i] = Rng.Next(int.MinValue, int.MaxValue);
                break;
            case VectorType.Ascending:
                for (int i = 0; i < size; i++)
                    array[i] = i;
                break;
            case VectorType.Descending:
                for (int i = 0; i < size; i++)
                    array[i] = size - 1 - i;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type));
        }

        return array;
    }

    public static string GetDescription(VectorType type) => type switch
    {
        VectorType.Random => "Aleatório",
        VectorType.Ascending => "Crescente",
        VectorType.Descending => "Decrescente",
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
}
