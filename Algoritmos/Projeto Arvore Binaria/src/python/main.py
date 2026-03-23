"""
Árvore Binária de Busca (ABB) e Árvore AVL — Dataset de Desempenho de Estudantes.
Chave de ordenação: student_id
"""

import sys
from pathlib import Path
from typing import Optional

from .benchmark import imprimir_tabela_comparacao
from .dataset import carregar_dataset
from .relatorio import escrever_relatorios_execucao
from .sistema import imprimir_info_sistema


def _raiz_projeto() -> Path:
    return Path(__file__).resolve().parent.parent.parent


def _resolver_arquivo_dados(raiz: Path) -> Optional[Path]:
    """Prefere CSV (enunciado); senão usa o Excel com o mesmo nome base."""
    candidatos = (
        "student_performance_prediction_dataset-2.csv",
        "student_performance_prediction_dataset-2.xlsx",
    )
    for nome in candidatos:
        p = raiz / nome
        if p.is_file():
            return p
    return None


def main() -> None:
    sys.setrecursionlimit(50000)

    raiz = _raiz_projeto()
    arquivo = _resolver_arquivo_dados(raiz)
    if arquivo is None:
        print("\nErro: nenhum dataset encontrado na pasta do projeto.")
        print("Esperado um destes ficheiros na raiz do projeto:")
        print("  • student_performance_prediction_dataset-2.csv")
        print("  • student_performance_prediction_dataset-2.xlsx")
        sys.exit(1)

    info_sistema = imprimir_info_sistema()

    print("\n" + "=" * 70)
    print("  ABB & AVL — Desempenho de Estudantes")
    print("=" * 70)

    # 1. Carregamento (ambas as árvores)
    print("\n[1] Carregando dados e construindo ABB e AVL...")
    abb, avl, t_ins_abb, t_ins_avl, n_linhas = carregar_dataset(arquivo)
    print(f"    Linhas lidas: {n_linhas:,}")
    print(f"    ✔ {abb}  |  Tempo de carga: {t_ins_abb * 1000:.3f} ms")
    print(f"    ✔ {avl}  |  Tempo de carga: {t_ins_avl * 1000:.3f} ms")

    # 2. Informações gerais
    print("\n[2] Informações das Árvores")
    print(f"    {'':25} {'ABB':>10} {'AVL':>10}")
    print(f"    {'Total de nós':<25} {abb.total:>10} {avl.total:>10}")
    print(f"    {'Altura':<25} {abb.altura():>10} {avl.altura():>10}")
    print(f"    {'ID mínimo':<25} {abb.minimo().student_id:>10} {avl.minimo().student_id:>10}")
    print(f"    {'ID máximo':<25} {abb.maximo().student_id:>10} {avl.maximo().student_id:>10}")

    # 3. Busca pontual
    ids_teste = [238418, 91535, 999999]
    print("\n[3] Busca por student_id (ABB)")
    for sid in ids_teste:
        resultado = abb.buscar(sid)
        if resultado:
            print(f"    ✔ Encontrado → {resultado}")
        else:
            print(f"    ✘ student_id {sid} não encontrado")

    print("\n    Busca por student_id (AVL)")
    for sid in ids_teste:
        resultado = avl.buscar(sid)
        if resultado:
            print(f"    ✔ Encontrado → {resultado}")
        else:
            print(f"    ✘ student_id {sid} não encontrado")

    # 4. Traversal em-ordem (primeiros 5)
    print("\n[4] Traversal Em-Ordem — 5 primeiros (ABB)")
    for est in abb.em_ordem()[:5]:
        print(f"    • {est}")

    # 5. Busca por faixa de IDs
    id_min, id_max = 100000, 110000
    faixa_abb = abb.buscar_por_faixa(id_min, id_max)
    faixa_avl = avl.buscar_por_faixa(id_min, id_max)
    print(f"\n[5] Busca por faixa [{id_min} — {id_max}]")
    print(f"    ABB: {len(faixa_abb)} estudante(s) | AVL: {len(faixa_avl)} estudante(s)")

    # 6. Remoção
    id_remover = 238418
    print(f"\n[6] Removendo student_id = {id_remover}")
    ok_abb = abb.remover(id_remover)
    ok_avl = avl.remover(id_remover)
    print(f"    ABB: {'✔ Removido' if ok_abb else '✘ Não encontrado'} | Total: {abb.total}")
    print(f"    AVL: {'✔ Removido' if ok_avl else '✘ Não encontrado'} | Total: {avl.total}")

    # 7. Operador 'in'
    print("\n[7] Operador 'in'")
    print(f"    91535 in abb → {91535 in abb} | 91535 in avl → {91535 in avl}")
    print(f"    {id_remover} in abb → {id_remover in abb} | {id_remover} in avl → {id_remover in avl}")

    # 8. Busca por nota final
    print("\n[8] Busca por nota final entre 8.0 e 10.0")
    top_abb = abb.buscar_por_nota(8.0, 10.0)
    top_avl = avl.buscar_por_nota(8.0, 10.0)
    print(f"    ABB: {len(top_abb)} estudante(s) | AVL: {len(top_avl)} estudante(s)")

    # 9. Aprovados e reprovados
    apr_abb, rep_abb = abb.buscar_aprovados(), abb.buscar_reprovados()
    apr_avl, rep_avl = avl.buscar_aprovados(), avl.buscar_reprovados()
    print(f"\n[9] Aprovados — ABB: {len(apr_abb)} | AVL: {len(apr_avl)}")
    print(f"    Reprovados — ABB: {len(rep_abb)} | AVL: {len(rep_avl)}")

    # 10. Busca por idade
    com_20_abb = abb.buscar_por_idade(20)
    com_20_avl = avl.buscar_por_idade(20)
    print(f"\n[10] Estudantes com 20 anos — ABB: {len(com_20_abb)} | AVL: {len(com_20_avl)}")

    # 11. Busca por gênero
    fem_abb = abb.buscar_por_genero("Female")
    fem_avl = avl.buscar_por_genero("Female")
    print(f"\n[11] Gênero 'Female' — ABB: {len(fem_abb)} | AVL: {len(fem_avl)}")

    resultado_bench = imprimir_tabela_comparacao(abb, avl, t_ins_abb, t_ins_avl)

    p_ts, p_latest = escrever_relatorios_execucao(
        raiz, arquivo.resolve(), n_linhas, info_sistema, resultado_bench
    )
    print("\n" + "=" * 70)
    print("  Relatório HTML gerado:")
    print(f"    • {p_ts}")
    print(f"    • {p_latest} (sempre a última execução)")
    print("=" * 70)

    print("\n" + "=" * 70)
    print("  Concluído com sucesso!")
    print("=" * 70)


if __name__ == "__main__":
    main()
