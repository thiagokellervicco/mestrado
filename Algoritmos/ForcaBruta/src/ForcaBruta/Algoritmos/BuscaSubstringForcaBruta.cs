namespace ForcaBruta.Algoritmos;

/// <summary>
/// Correspondência ingênua de padrão: compara o padrão com cada posição do texto.
/// Complexidade temporal O(n·m), espacial O(1).
/// </summary>
public static class BuscaSubstringForcaBruta
{
    /// <returns>Índice da primeira ocorrência de <paramref name="padrao"/> em <paramref name="texto"/>, ou -1.</returns>
    public static int PrimeiraOcorrencia(ReadOnlySpan<char> texto, ReadOnlySpan<char> padrao)
    {
        if (padrao.Length == 0)
            return 0;
        if (texto.Length < padrao.Length)
            return -1;

        var limite = texto.Length - padrao.Length;
        for (var i = 0; i <= limite; i++)
        {
            var j = 0;
            for (; j < padrao.Length; j++)
            {
                if (texto[i + j] != padrao[j])
                    break;
            }

            if (j == padrao.Length)
                return i;
        }

        return -1;
    }
}
