"""Medição e tabelas comparativas ABB vs AVL."""

import time
from dataclasses import dataclass
from typing import Callable, List, Tuple

from .arvore_avl import ArvoreAVL
from .arvore_binaria import ArvoreBinariaBusca


def medir(fn: Callable[[], None], repeticoes: int = 5) -> float:
    """Executa fn() N vezes e retorna a média em milissegundos."""
    total = 0.0
    for _ in range(repeticoes):
        t0 = time.perf_counter()
        fn()
        total += time.perf_counter() - t0
    return (total / repeticoes) * 1000.0


@dataclass(frozen=True)
class BenchmarkResult:
    """Resultados numéricos de uma execução do benchmark (para console e relatório)."""

    t_ins_abb_s: float
    t_ins_avl_s: float
    total_abb: int
    total_avl: int
    altura_abb: int
    altura_avl: int
    id_min_abb: int
    id_max_abb: int
    id_min_avl: int
    id_max_avl: int
    operacoes: List[Tuple[str, float, float, str]]
    remocao_ms_abb: float
    remocao_ms_avl: float
    remocao_vencedor: str


def executar_benchmark_comparacao(
    abb: ArvoreBinariaBusca,
    avl: ArvoreAVL,
    t_ins_abb: float,
    t_ins_avl: float,
    repeticoes: int = 5,
) -> BenchmarkResult:
    """Calcula métricas ABB vs AVL (não imprime)."""
    sid_busca = abb.minimo().student_id
    id_min, id_max = 100000, 200000

    operacoes_raw: List[Tuple[str, Callable[[], None], Callable[[], None]]] = [
        ("Busca pontual (1 ID)", lambda: abb.buscar(sid_busca), lambda: avl.buscar(sid_busca)),
        ("Em-Ordem (traversal completo)", lambda: abb.em_ordem(), lambda: avl.em_ordem()),
        ("Pré-Ordem (traversal completo)", lambda: abb.pre_ordem(), lambda: avl.pre_ordem()),
        ("Pós-Ordem (traversal completo)", lambda: abb.pos_ordem(), lambda: avl.pos_ordem()),
        (
            "Busca por faixa de IDs",
            lambda: abb.buscar_por_faixa(id_min, id_max),
            lambda: avl.buscar_por_faixa(id_min, id_max),
        ),
        (
            "Busca por nota (8.0–10.0)",
            lambda: abb.buscar_por_nota(8.0, 10.0),
            lambda: avl.buscar_por_nota(8.0, 10.0),
        ),
        ("Buscar aprovados", lambda: abb.buscar_aprovados(), lambda: avl.buscar_aprovados()),
        ("Buscar reprovados", lambda: abb.buscar_reprovados(), lambda: avl.buscar_reprovados()),
        ("Busca por idade (20 anos)", lambda: abb.buscar_por_idade(20), lambda: avl.buscar_por_idade(20)),
        (
            "Busca por gênero (Female)",
            lambda: abb.buscar_por_genero("Female"),
            lambda: avl.buscar_por_genero("Female"),
        ),
    ]

    operacoes: List[Tuple[str, float, float, str]] = []
    for nome, fn_abb, fn_avl in operacoes_raw:
        t_a = medir(fn_abb, repeticoes)
        t_v = medir(fn_avl, repeticoes)
        v = "ABB" if t_a < t_v else ("AVL" if t_v < t_a else "Empate")
        operacoes.append((nome, t_a, t_v, v))

    ids_remover = [e.student_id for e in abb.em_ordem()[:5]]

    def remocao_abb():
        tmp = ArvoreBinariaBusca()
        for e in abb.em_ordem():
            tmp.inserir(e)
        for sid in ids_remover:
            tmp.remover(sid)

    def remocao_avl():
        tmp = ArvoreAVL()
        for e in avl.em_ordem():
            tmp.inserir(e)
        for sid in ids_remover:
            tmp.remover(sid)

    t_r_abb = medir(remocao_abb, repeticoes)
    t_r_avl = medir(remocao_avl, repeticoes)
    v_rem = "ABB" if t_r_abb < t_r_avl else ("AVL" if t_r_avl < t_r_abb else "Empate")

    return BenchmarkResult(
        t_ins_abb_s=t_ins_abb,
        t_ins_avl_s=t_ins_avl,
        total_abb=abb.total,
        total_avl=avl.total,
        altura_abb=abb.altura(),
        altura_avl=avl.altura(),
        id_min_abb=abb.minimo().student_id,
        id_max_abb=abb.maximo().student_id,
        id_min_avl=avl.minimo().student_id,
        id_max_avl=avl.maximo().student_id,
        operacoes=operacoes,
        remocao_ms_abb=t_r_abb,
        remocao_ms_avl=t_r_avl,
        remocao_vencedor=v_rem,
    )


def imprimir_resultado_benchmark(r: BenchmarkResult) -> None:
    """Imprime tabelas comparativas no console."""
    print("\n" + "=" * 70)
    print("  TABELA 1 — Estrutura das Árvores")
    print("=" * 70)
    print(f"  {'Métrica':<30} {'ABB':>15} {'AVL':>15}")
    print(f"  {'-'*30} {'-'*15} {'-'*15}")
    print(f"  {'Total de nós':<30} {r.total_abb:>15,} {r.total_avl:>15,}")
    print(f"  {'Altura da árvore':<30} {r.altura_abb:>15} {r.altura_avl:>15}")
    print(f"  {'ID mínimo':<30} {r.id_min_abb:>15} {r.id_min_avl:>15}")
    print(f"  {'ID máximo':<30} {r.id_max_abb:>15} {r.id_max_avl:>15}")

    print("\n" + "=" * 70)
    print("  TABELA 2 — Tempo de Inserção (carga completa do dataset)")
    print("=" * 70)
    print(f"  {'Operação':<30} {'ABB (ms)':>12} {'AVL (ms)':>12} {'Δ (ms)':>10} {'Vencedor':>10}")
    print(f"  {'-'*30} {'-'*12} {'-'*12} {'-'*10} {'-'*10}")
    t_abb_ms = r.t_ins_abb_s * 1000
    t_avl_ms = r.t_ins_avl_s * 1000
    delta = t_avl_ms - t_abb_ms
    v_ins = "ABB" if t_abb_ms < t_avl_ms else ("AVL" if t_avl_ms < t_abb_ms else "Empate")
    print(
        f"  {'Inserção (todos os registros)':<30} {t_abb_ms:>12.3f} {t_avl_ms:>12.3f} {delta:>+10.3f} {v_ins:>10}"
    )

    print("\n" + "=" * 70)
    print("  TABELA 3 — Tempo Médio das Operações (média de 5 execuções, em ms)")
    print("=" * 70)
    print(f"  {'Operação':<35} {'ABB (ms)':>12} {'AVL (ms)':>12} {'Vencedor':>10}")
    print(f"  {'-'*35} {'-'*12} {'-'*12} {'-'*10}")
    for nome, t_a, t_v, v in r.operacoes:
        print(f"  {nome:<35} {t_a:>12.4f} {t_v:>12.4f} {v:>10}")

    print("\n" + "=" * 70)
    print("  TABELA 4 — Tempo de Remoção (ID existente, média de 5 execuções)")
    print("=" * 70)
    print(f"  {'Operação':<35} {'ABB (ms)':>12} {'AVL (ms)':>12} {'Vencedor':>10}")
    print(f"  {'-'*35} {'-'*12} {'-'*12} {'-'*10}")
    print(
        f"  {'Remover 5 nós (+ reconstrução)':<35} {r.remocao_ms_abb:>12.3f} {r.remocao_ms_avl:>12.3f} {r.remocao_vencedor:>10}"
    )

    print("\n" + "=" * 70)
    print("  Legenda: Δ positivo = AVL foi mais lenta que ABB na inserção")
    print("           Menor tempo = melhor desempenho")
    print("=" * 70)


def imprimir_tabela_comparacao(
    abb: ArvoreBinariaBusca,
    avl: ArvoreAVL,
    t_ins_abb: float,
    t_ins_avl: float,
) -> BenchmarkResult:
    """Executa o benchmark e imprime (retorna o resultado para relatório)."""
    resultado = executar_benchmark_comparacao(abb, avl, t_ins_abb, t_ins_avl)
    imprimir_resultado_benchmark(resultado)
    return resultado
