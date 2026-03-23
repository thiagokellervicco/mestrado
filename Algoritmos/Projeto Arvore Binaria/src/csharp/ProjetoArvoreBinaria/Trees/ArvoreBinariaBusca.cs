using ProjetoArvoreBinaria.Models;

namespace ProjetoArvoreBinaria.Trees;

/// <summary>Árvore binária de busca indexada por student_id.</summary>
public sealed class ArvoreBinariaBusca
{
    private No? _raiz;
    
    private int _totalNos;

    public int Total => _totalNos;

    public void Inserir(Estudante estudante)
    {
        var novo = new No { Estudante = estudante };
        if (_raiz is null)
        {
            _raiz = novo;
            _totalNos++;
            return;
        }
        if (InserirRecursivo(_raiz, novo))
            _totalNos++;
    }

    private static bool InserirRecursivo(No atual, No novo)
    {
        if (novo.Chave < atual.Chave)
        {
            if (atual.Esquerda is null)
            {
                atual.Esquerda = novo;
                return true;
            }
            return InserirRecursivo(atual.Esquerda, novo);
        }
        if (novo.Chave > atual.Chave)
        {
            if (atual.Direita is null)
            {
                atual.Direita = novo;
                return true;
            }
            return InserirRecursivo(atual.Direita, novo);
        }
        return false;
    }

    public Estudante? Buscar(int studentId) => BuscarRecursivo(_raiz, studentId);

    private static Estudante? BuscarRecursivo(No? atual, int chave)
    {
        if (atual is null) return null;
        if (atual.Chave == chave) return atual.Estudante;
        return chave < atual.Chave
            ? BuscarRecursivo(atual.Esquerda, chave)
            : BuscarRecursivo(atual.Direita, chave);
    }

    public bool Remover(int studentId)
    {
        var (nova, removido) = RemoverRecursivo(_raiz, studentId);
        _raiz = nova;
        if (removido) _totalNos--;
        return removido;
    }

    private static (No? no, bool removido) RemoverRecursivo(No? atual, int chave)
    {
        if (atual is null) return (null, false);
        if (chave < atual.Chave)
        {
            var (e, r) = RemoverRecursivo(atual.Esquerda, chave);
            atual.Esquerda = e;
            return (atual, r);
        }
        if (chave > atual.Chave)
        {
            var (d, r) = RemoverRecursivo(atual.Direita, chave);
            atual.Direita = d;
            return (atual, r);
        }

        if (atual.Esquerda is null) return (atual.Direita, true);
        if (atual.Direita is null) return (atual.Esquerda, true);
        var sucessor = Minimo(atual.Direita);
        atual.Estudante = sucessor.Estudante;
        var (dir, _) = RemoverRecursivo(atual.Direita, sucessor.Chave);
        atual.Direita = dir;
        return (atual, true);
    }

    private static No Minimo(No no)
    {
        while (no.Esquerda is not null) no = no.Esquerda;
        return no;
    }

    public Estudante? Minimo() => _raiz is null ? null : Minimo(_raiz).Estudante;

    public Estudante? Maximo()
    {
        if (_raiz is null) return null;
        var no = _raiz;
        while (no.Direita is not null) no = no.Direita;
        return no.Estudante;
    }

    public int Altura() => AlturaRecursivo(_raiz);

    private static int AlturaRecursivo(No? no) =>
        no is null ? 0 : 1 + Math.Max(AlturaRecursivo(no.Esquerda), AlturaRecursivo(no.Direita));

    public List<Estudante> EmOrdem()
    {
        var r = new List<Estudante>();
        EmOrdemRecursivo(_raiz, r);
        return r;
    }

    private static void EmOrdemRecursivo(No? no, List<Estudante> lista)
    {
        if (no is null) return;
        EmOrdemRecursivo(no.Esquerda, lista);
        lista.Add(no.Estudante);
        EmOrdemRecursivo(no.Direita, lista);
    }

    public List<Estudante> PreOrdem()
    {
        var r = new List<Estudante>();
        PreOrdemRecursivo(_raiz, r);
        return r;
    }

    private static void PreOrdemRecursivo(No? no, List<Estudante> lista)
    {
        if (no is null) return;
        lista.Add(no.Estudante);
        PreOrdemRecursivo(no.Esquerda, lista);
        PreOrdemRecursivo(no.Direita, lista);
    }

    public List<Estudante> PosOrdem()
    {
        var r = new List<Estudante>();
        PosOrdemRecursivo(_raiz, r);
        return r;
    }

    private static void PosOrdemRecursivo(No? no, List<Estudante> lista)
    {
        if (no is null) return;
        PosOrdemRecursivo(no.Esquerda, lista);
        PosOrdemRecursivo(no.Direita, lista);
        lista.Add(no.Estudante);
    }

    public List<Estudante> BuscarPorFaixa(int idMin, int idMax)
    {
        var r = new List<Estudante>();
        BuscaFaixaRecursiva(_raiz, idMin, idMax, r);
        return r;
    }

    private static void BuscaFaixaRecursiva(No? no, int idMin, int idMax, List<Estudante> lista)
    {
        if (no is null) return;
        if (no.Chave > idMin) BuscaFaixaRecursiva(no.Esquerda, idMin, idMax, lista);
        if (idMin <= no.Chave && no.Chave <= idMax) lista.Add(no.Estudante);
        if (no.Chave < idMax) BuscaFaixaRecursiva(no.Direita, idMin, idMax, lista);
    }

    public List<Estudante> BuscarPorNota(double min, double max)
    {
        var r = new List<Estudante>();
        BuscaNotaRecursiva(_raiz, min, max, r);
        return r;
    }

    private static void BuscaNotaRecursiva(No? no, double min, double max, List<Estudante> lista)
    {
        if (no is null) return;
        BuscaNotaRecursiva(no.Esquerda, min, max, lista);
        if (min <= no.Estudante.FinalGrade && no.Estudante.FinalGrade <= max) lista.Add(no.Estudante);
        BuscaNotaRecursiva(no.Direita, min, max, lista);
    }

    public List<Estudante> BuscarAprovados() => BuscarPorCampo(e => e.PassFail == "Pass");
    public List<Estudante> BuscarReprovados() => BuscarPorCampo(e => e.PassFail == "Fail");
    public List<Estudante> BuscarPorIdade(int idade) => BuscarPorCampo(e => e.Age == idade);
    public List<Estudante> BuscarPorGenero(string genero) => BuscarPorCampo(e => e.Gender == genero);

    private List<Estudante> BuscarPorCampo(Func<Estudante, bool> pred)
    {
        var r = new List<Estudante>();
        BuscarPorCampoRecursivo(_raiz, pred, r);
        return r;
    }

    private static void BuscarPorCampoRecursivo(No? no, Func<Estudante, bool> pred, List<Estudante> lista)
    {
        if (no is null) return;
        BuscarPorCampoRecursivo(no.Esquerda, pred, lista);
        if (pred(no.Estudante)) lista.Add(no.Estudante);
        BuscarPorCampoRecursivo(no.Direita, pred, lista);
    }

    public bool Contains(int studentId) => Buscar(studentId) is not null;

    public override string ToString() =>
        $"ArvoreBinariaBusca(total={_totalNos}, altura={Altura()}, raiz_id={(_raiz?.Chave.ToString() ?? "null")})";
}
