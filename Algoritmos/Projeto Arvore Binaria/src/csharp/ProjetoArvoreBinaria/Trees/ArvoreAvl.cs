using ProjetoArvoreBinaria.Models;

namespace ProjetoArvoreBinaria.Trees;

/// <summary>Árvore AVL indexada por student_id.</summary>
public sealed class ArvoreAvl
{
    private NoAvl? _raiz;
    private int _totalNos;

    public int Total => _totalNos;

    private static int AlturaNo(NoAvl? no) => no?.Altura ?? 0;

    private static void AtualizarAltura(NoAvl no) =>
        no.Altura = 1 + Math.Max(AlturaNo(no.Esquerda), AlturaNo(no.Direita));

    private static int FatorBalanceamento(NoAvl no) => AlturaNo(no.Esquerda) - AlturaNo(no.Direita);

    private NoAvl RotacaoDireita(NoAvl y)
    {
        var x = y.Esquerda!;
        var t2 = x.Direita;
        x.Direita = y;
        y.Esquerda = t2;
        AtualizarAltura(y);
        AtualizarAltura(x);
        return x;
    }

    private NoAvl RotacaoEsquerda(NoAvl x)
    {
        var y = x.Direita!;
        var t2 = y.Esquerda;
        y.Esquerda = x;
        x.Direita = t2;
        AtualizarAltura(x);
        AtualizarAltura(y);
        return y;
    }

    private NoAvl Balancear(NoAvl no)
    {
        AtualizarAltura(no);
        var fb = FatorBalanceamento(no);
        if (fb > 1 && FatorBalanceamento(no.Esquerda!) >= 0) return RotacaoDireita(no);
        if (fb > 1 && FatorBalanceamento(no.Esquerda!) < 0)
        {
            no.Esquerda = RotacaoEsquerda(no.Esquerda!);
            return RotacaoDireita(no);
        }
        if (fb < -1 && FatorBalanceamento(no.Direita!) <= 0) return RotacaoEsquerda(no);
        if (fb < -1 && FatorBalanceamento(no.Direita!) > 0)
        {
            no.Direita = RotacaoDireita(no.Direita!);
            return RotacaoEsquerda(no);
        }
        return no;
    }

    public void Inserir(Estudante estudante)
    {
        _raiz = InserirRecursivo(_raiz, estudante);
        _totalNos++;
    }

    private NoAvl? InserirRecursivo(NoAvl? no, Estudante estudante)
    {
        if (no is null) return new NoAvl { Estudante = estudante };
        if (estudante.StudentId < no.Chave)
            no.Esquerda = InserirRecursivo(no.Esquerda, estudante);
        else if (estudante.StudentId > no.Chave)
            no.Direita = InserirRecursivo(no.Direita, estudante);
        else
        {
            _totalNos--;
            return no;
        }
        return Balancear(no);
    }

    public Estudante? Buscar(int studentId) => BuscarRecursivo(_raiz, studentId);

    private static Estudante? BuscarRecursivo(NoAvl? atual, int chave)
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

    private (NoAvl? no, bool removido) RemoverRecursivo(NoAvl? no, int chave)
    {
        if (no is null) return (null, false);
        if (chave < no.Chave)
        {
            var (e, r) = RemoverRecursivo(no.Esquerda, chave);
            no.Esquerda = e;
            return (Balancear(no), r);
        }
        if (chave > no.Chave)
        {
            var (d, r) = RemoverRecursivo(no.Direita, chave);
            no.Direita = d;
            return (Balancear(no), r);
        }

        if (no.Esquerda is null) return (no.Direita, true);
        if (no.Direita is null) return (no.Esquerda, true);
        var sucessor = Minimo(no.Direita);
        no.Estudante = sucessor.Estudante;
        var (dir, _) = RemoverRecursivo(no.Direita, sucessor.Chave);
        no.Direita = dir;
        return (Balancear(no), true);
    }

    private static NoAvl Minimo(NoAvl no)
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

    public int Altura() => AlturaNo(_raiz);

    public List<Estudante> EmOrdem()
    {
        var r = new List<Estudante>();
        EmOrdemRecursivo(_raiz, r);
        return r;
    }

    private static void EmOrdemRecursivo(NoAvl? no, List<Estudante> lista)
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

    private static void PreOrdemRecursivo(NoAvl? no, List<Estudante> lista)
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

    private static void PosOrdemRecursivo(NoAvl? no, List<Estudante> lista)
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

    private static void BuscaFaixaRecursiva(NoAvl? no, int idMin, int idMax, List<Estudante> lista)
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

    private static void BuscaNotaRecursiva(NoAvl? no, double min, double max, List<Estudante> lista)
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

    private static void BuscarPorCampoRecursivo(NoAvl? no, Func<Estudante, bool> pred, List<Estudante> lista)
    {
        if (no is null) return;
        BuscarPorCampoRecursivo(no.Esquerda, pred, lista);
        if (pred(no.Estudante)) lista.Add(no.Estudante);
        BuscarPorCampoRecursivo(no.Direita, pred, lista);
    }

    public bool Contains(int studentId) => Buscar(studentId) is not null;

    public override string ToString() =>
        $"ArvoreAVL(total={_totalNos}, altura={Altura()}, raiz_id={(_raiz?.Chave.ToString() ?? "null")})";
}
