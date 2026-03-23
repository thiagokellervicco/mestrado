using System.Diagnostics;
using ProjetoArvoreBinaria.Trees;

namespace ProjetoArvoreBinaria.Services;

public sealed record BenchmarkResult(
    double TInsAbbS,
    double TInsAvlS,
    int TotalAbb,
    int TotalAvl,
    int AlturaAbb,
    int AlturaAvl,
    int IdMinAbb,
    int IdMaxAbb,
    int IdMinAvl,
    int IdMaxAvl,
    IReadOnlyList<(string Nome, double MsAbb, double MsAvl, string Vencedor)> Operacoes,
    double RemocaoMsAbb,
    double RemocaoMsAvl,
    string RemocaoVencedor);

public static class BenchmarkService
{
    private static double Medir(Action acao, int repeticoes = 5)
    {
        double total = 0;
        for (var i = 0; i < repeticoes; i++)
        {
            var sw = Stopwatch.StartNew();
            acao();
            sw.Stop();
            total += sw.Elapsed.TotalMilliseconds;
        }
        return total / repeticoes;
    }

    public static BenchmarkResult Executar(ArvoreBinariaBusca abb, ArvoreAvl avl, double tInsAbb, double tInsAvl)
    {
        var sidBusca = abb.Minimo()!.StudentId;
        const int idMin = 100_000;
        const int idMax = 200_000;

        var ops = new List<(string, double, double, string)>
        {
            MedirPar("Busca pontual (1 ID)", () => abb.Buscar(sidBusca), () => avl.Buscar(sidBusca)),
            MedirPar("Em-Ordem (traversal completo)", () => abb.EmOrdem(), () => avl.EmOrdem()),
            MedirPar("Pré-Ordem (traversal completo)", () => abb.PreOrdem(), () => avl.PreOrdem()),
            MedirPar("Pós-Ordem (traversal completo)", () => abb.PosOrdem(), () => avl.PosOrdem()),
            MedirPar("Busca por faixa de IDs", () => abb.BuscarPorFaixa(idMin, idMax), () => avl.BuscarPorFaixa(idMin, idMax)),
            MedirPar("Busca por nota (8.0–10.0)", () => abb.BuscarPorNota(8, 10), () => avl.BuscarPorNota(8, 10)),
            MedirPar("Buscar aprovados", () => abb.BuscarAprovados(), () => avl.BuscarAprovados()),
            MedirPar("Buscar reprovados", () => abb.BuscarReprovados(), () => avl.BuscarReprovados()),
            MedirPar("Busca por idade (20 anos)", () => abb.BuscarPorIdade(20), () => avl.BuscarPorIdade(20)),
            MedirPar("Busca por gênero (Female)", () => abb.BuscarPorGenero("Female"), () => avl.BuscarPorGenero("Female")),
        };

        var idsRemover = abb.EmOrdem().Take(5).Select(e => e.StudentId).ToList();

        var tRemAbb = Medir(() =>
        {
            var tmp = new ArvoreBinariaBusca();
            foreach (var e in abb.EmOrdem()) tmp.Inserir(e);
            foreach (var sid in idsRemover) tmp.Remover(sid);
        });
        var tRemAvl = Medir(() =>
        {
            var tmp = new ArvoreAvl();
            foreach (var e in avl.EmOrdem()) tmp.Inserir(e);
            foreach (var sid in idsRemover) tmp.Remover(sid);
        });
        var vRem = tRemAbb < tRemAvl ? "ABB" : (tRemAvl < tRemAbb ? "AVL" : "Empate");

        return new BenchmarkResult(
            tInsAbb,
            tInsAvl,
            abb.Total,
            avl.Total,
            abb.Altura(),
            avl.Altura(),
            abb.Minimo()!.StudentId,
            abb.Maximo()!.StudentId,
            avl.Minimo()!.StudentId,
            avl.Maximo()!.StudentId,
            ops,
            tRemAbb,
            tRemAvl,
            vRem);
    }

    private static (string, double, double, string) MedirPar(string nome, Action aAbb, Action aAvl)
    {
        var ta = Medir(aAbb);
        var tv = Medir(aAvl);
        var v = ta < tv ? "ABB" : (tv < ta ? "AVL" : "Empate");
        return (nome, ta, tv, v);
    }

    public static void Imprimir(BenchmarkResult r)
    {
        Console.WriteLine();
        Console.WriteLine(new string('=', 70));
        Console.WriteLine("  TABELA 1 — Estrutura das Árvores");
        Console.WriteLine(new string('=', 70));
        Console.WriteLine($"  {"Métrica",-30} {"ABB",15} {"AVL",15}");
        Console.WriteLine($"  {new string('-', 30),-30} {new string('-', 15),15} {new string('-', 15),15}");
        Console.WriteLine($"  {"Total de nós",-30} {r.TotalAbb,15:N0} {r.TotalAvl,15:N0}");
        Console.WriteLine($"  {"Altura da árvore",-30} {r.AlturaAbb,15} {r.AlturaAvl,15}");
        Console.WriteLine($"  {"ID mínimo",-30} {r.IdMinAbb,15} {r.IdMinAvl,15}");
        Console.WriteLine($"  {"ID máximo",-30} {r.IdMaxAbb,15} {r.IdMaxAvl,15}");

        Console.WriteLine();
        Console.WriteLine(new string('=', 70));
        Console.WriteLine("  TABELA 2 — Tempo de Inserção (carga completa do dataset)");
        Console.WriteLine(new string('=', 70));
        var abbMs = r.TInsAbbS * 1000;
        var avlMs = r.TInsAvlS * 1000;
        var delta = avlMs - abbMs;
        var vIns = abbMs < avlMs ? "ABB" : (avlMs < abbMs ? "AVL" : "Empate");
        Console.WriteLine($"  {"Operação",-30} {"ABB (ms)",12} {"AVL (ms)",12} {"Δ (ms)",10} {"Vencedor",10}");
        Console.WriteLine($"  {"Inserção (todos os registros)",-30} {abbMs,12:F3} {avlMs,12:F3} {delta,+10:F3} {vIns,10}");

        Console.WriteLine();
        Console.WriteLine(new string('=', 70));
        Console.WriteLine("  TABELA 3 — Tempo Médio das Operações (média de 5 execuções, em ms)");
        Console.WriteLine(new string('=', 70));
        Console.WriteLine($"  {"Operação",-35} {"ABB (ms)",12} {"AVL (ms)",12} {"Vencedor",10}");
        foreach (var (nome, ta, tv, v) in r.Operacoes)
            Console.WriteLine($"  {nome,-35} {ta,12:F4} {tv,12:F4} {v,10}");

        Console.WriteLine();
        Console.WriteLine(new string('=', 70));
        Console.WriteLine("  TABELA 4 — Tempo de Remoção (média de 5 execuções)");
        Console.WriteLine(new string('=', 70));
        Console.WriteLine($"  {"Remover 5 nós (+ reconstrução)",-35} {r.RemocaoMsAbb,12:F3} {r.RemocaoMsAvl,12:F3} {r.RemocaoVencedor,10}");
        Console.WriteLine();
        Console.WriteLine(new string('=', 70));
    }
}
