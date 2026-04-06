"""Geração de relatório HTML a cada execução."""

from __future__ import annotations

import html
import json
import re
from datetime import datetime
from pathlib import Path
from typing import Any, Dict, List, Sequence, Tuple

from .benchmark import BenchmarkResult

_REL_LANG = re.compile(r"<strong>Linguagem:</strong>\s*([^<]+)", re.IGNORECASE)


def _rotulo_relatorio_html(nome: str) -> str:
    m = re.match(r"relatorio_(\d{8})_(\d{6})\.html$", nome, re.IGNORECASE)
    if not m:
        return nome
    d, t = m.group(1), m.group(2)
    return f"{d[:4]}-{d[4:6]}-{d[6:8]} {t[:2]}:{t[2:4]}:{t[4:6]}"


def _linguagem_lida_do_html(caminho: Path) -> str:
    try:
        trecho = caminho.read_text(encoding="utf-8")[:16_384]
    except OSError:
        return "?"
    found = _REL_LANG.search(trecho)
    return found.group(1).strip() if found else "?"


def regenerar_indice_relatorios(pasta_reports: Path) -> None:
    """
    Varre `relatorio_*.html` (exceto `relatorio_latest.html`), infere a linguagem
    do conteúdo e grava `reports_data.js` para a página `index.html` da pasta.
    """
    pasta_reports.mkdir(parents=True, exist_ok=True)
    nomes: List[str] = []
    for p in sorted(pasta_reports.glob("relatorio_*.html"), reverse=True):
        if p.name.lower() == "relatorio_latest.html":
            continue
        nomes.append(p.name)
    itens: List[Dict[str, str]] = []
    for nome in nomes:
        caminho = pasta_reports / nome
        itens.append(
            {
                "file": nome,
                "language": _linguagem_lida_do_html(caminho),
                "label": _rotulo_relatorio_html(nome),
            }
        )
    corpo = json.dumps(itens, ensure_ascii=False, indent=2)
    js = f"// Auto-gerado ao executar Python ou C#. Não editar.\nwindow.PROJETO_ARVORE_RELATORIOS = {corpo};\n"
    (pasta_reports / "reports_data.js").write_text(js, encoding="utf-8")


def _html_table(headers: Sequence[str], rows: Sequence[Sequence[str]]) -> str:
    th = "".join(f"<th>{html.escape(h)}</th>" for h in headers)
    trs = []
    for row in rows:
        cells = "".join(f"<td>{html.escape(str(c))}</td>" for c in row)
        trs.append(f"<tr>{cells}</tr>")
    return (
        '<table class="data">\n'
        f"  <thead><tr>{th}</tr></thead>\n"
        "  <tbody>\n"
        + "\n".join(f"    {t}" for t in trs)
        + "\n  </tbody>\n</table>"
    )


def _ram_linha(s: Dict[str, Any]) -> str:
    t, d, p = s.get("ram_total_gb"), s.get("ram_disponivel_gb"), s.get("ram_percent")
    if t is not None and d is not None and p is not None:
        return f"{t} GB / {d} GB / {p}%"
    if t is not None:
        return f"{t} GB / n/d / n/d"
    return "n/d / n/d / n/d"


def _conteudo_relatorio_html(
    arquivo_dados: Path,
    n_linhas: int,
    sistema: Dict[str, Any],
    bench: BenchmarkResult,
    linguagem: str = "Python",
) -> str:
    agora = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    clock = sistema.get("clock_mhz_atual")
    clock_txt = (
        f"{clock:.0f} / {sistema.get('clock_mhz_min', 0):.0f} / {sistema.get('clock_mhz_max', 0):.0f} MHz"
        if clock is not None
        else "n/d"
    )
    n_linhas_fmt = f"{n_linhas:,}".replace(",", ".")
    arquivo_str = html.escape(str(arquivo_dados))

    sec_ambiente = _html_table(
        ["Campo", "Valor"],
        [
            ["Sistema operacional", str(sistema.get("os", ""))],
            ["Arquitetura", str(sistema.get("arquitetura", ""))],
            ["CPU", str(sistema.get("cpu_modelo", ""))],
            [
                "Núcleos físicos / lógicos",
                f"{sistema.get('nucleos_fisicos')} / {sistema.get('nucleos_logicos')}",
            ],
            ["Clock atual / mín / máx", clock_txt],
            ["RAM total / disponível / uso %", _ram_linha(sistema)],
        ],
    )
    sec_estrutura = _html_table(
        ["Métrica", "ABB", "AVL"],
        [
            ["Total de nós", str(bench.total_abb), str(bench.total_avl)],
            ["Altura", str(bench.altura_abb), str(bench.altura_avl)],
            ["ID mínimo", str(bench.id_min_abb), str(bench.id_min_avl)],
            ["ID máximo", str(bench.id_max_abb), str(bench.id_max_avl)],
        ],
    )
    v_ins = (
        "ABB"
        if bench.t_ins_abb_s < bench.t_ins_avl_s
        else ("AVL" if bench.t_ins_avl_s < bench.t_ins_abb_s else "Empate")
    )
    sec_insercao = _html_table(
        ["", "ABB (ms)", "AVL (ms)", "Δ (ms)", "Vencedor"],
        [
            [
                "Inserção (todos os registros)",
                f"{bench.t_ins_abb_s * 1000:.3f}",
                f"{bench.t_ins_avl_s * 1000:.3f}",
                f"{(bench.t_ins_avl_s - bench.t_ins_abb_s) * 1000:+.3f}",
                v_ins,
            ],
        ],
    )
    ops_rows: List[List[str]] = [
        [nome, f"{a:.4f}", f"{v:.4f}", w] for nome, a, v, w in bench.operacoes
    ]
    sec_ops = _html_table(["Operação", "ABB (ms)", "AVL (ms)", "Vencedor"], ops_rows)
    sec_rem = _html_table(
        ["Operação", "ABB (ms)", "AVL (ms)", "Vencedor"],
        [
            [
                "Remover 5 nós (+ reconstrução)",
                f"{bench.remocao_ms_abb:.3f}",
                f"{bench.remocao_ms_avl:.3f}",
                bench.remocao_vencedor,
            ],
        ],
    )

    css = """
    :root { --bg:#0f1219; --card:#161b26; --text:#e6e9ef; --muted:#8b96a8; --accent:#5eead4; --border:#2a3344; }
    * { box-sizing: border-box; }
    body { font-family: system-ui, Segoe UI, sans-serif; background: var(--bg); color: var(--text); margin: 0; padding: 2rem; line-height: 1.5; }
    .wrap { max-width: 960px; margin: 0 auto; }
    h1 { font-size: 1.5rem; font-weight: 700; margin-bottom: 0.5rem; border-bottom: 1px solid var(--border); padding-bottom: 0.75rem; }
    .meta { color: var(--muted); font-size: 0.95rem; margin: 0.35rem 0; }
    .meta strong { color: var(--accent); }
    h2 { font-size: 1.1rem; margin-top: 2rem; margin-bottom: 0.75rem; color: var(--accent); }
    table.data { width: 100%; border-collapse: collapse; font-size: 0.9rem; background: var(--card); border: 1px solid var(--border); border-radius: 8px; overflow: hidden; }
    table.data th, table.data td { text-align: left; padding: 0.55rem 0.75rem; border-bottom: 1px solid var(--border); }
    table.data th { background: rgba(94,234,212,0.08); color: var(--accent); font-weight: 600; }
    table.data tr:last-child td { border-bottom: none; }
    table.data tbody tr:hover td { background: rgba(255,255,255,0.03); }
    footer { margin-top: 2.5rem; padding-top: 1rem; border-top: 1px solid var(--border); color: var(--muted); font-size: 0.85rem; }
    """

    return f"""<!DOCTYPE html>
<html lang="pt-BR">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>Relatório ABB vs AVL — {html.escape(linguagem)}</title>
  <style>{css}</style>
</head>
<body>
  <div class="wrap">
    <h1>Relatório — ABB vs AVL (desempenho de estudantes)</h1>
    <p class="meta"><strong>Autores:</strong> Thiago Keller e Ricardo Diniz</p>
    <p class="meta"><strong>Linguagem:</strong> {html.escape(linguagem)}</p>
    <p class="meta"><strong>Data/hora da execução:</strong> {html.escape(agora)}</p>
    <p class="meta"><strong>Arquivo de dados:</strong> <code>{arquivo_str}</code></p>
    <p class="meta"><strong>Linhas no dataset:</strong> {html.escape(n_linhas_fmt)}</p>

    <h2>Ambiente</h2>
    {sec_ambiente}

    <h2>Estrutura das árvores</h2>
    {sec_estrutura}

    <h2>Tempos de inserção (carga completa)</h2>
    {sec_insercao}

    <h2>Tempo médio das operações (ms, média de 5 execuções)</h2>
    {sec_ops}

    <h2>Remoção (5 nós + reconstrução, média de 5)</h2>
    {sec_rem}

    <footer>Relatório gerado automaticamente pelo programa. · Thiago Keller e Ricardo Diniz</footer>
  </div>
</body>
</html>
"""


def gerar_relatorio_html(
    caminho_saida: Path,
    arquivo_dados: Path,
    n_linhas: int,
    sistema: Dict[str, Any],
    bench: BenchmarkResult,
    linguagem: str = "Python",
) -> Path:
    """Escreve um único ficheiro HTML."""
    caminho_saida.parent.mkdir(parents=True, exist_ok=True)
    texto = _conteudo_relatorio_html(arquivo_dados, n_linhas, sistema, bench, linguagem)
    caminho_saida.write_text(texto, encoding="utf-8")
    return caminho_saida


def escrever_relatorios_execucao(
    raiz_projeto: Path,
    arquivo_dados: Path,
    n_linhas: int,
    sistema: Dict[str, Any],
    bench: BenchmarkResult,
    linguagem: str = "Python",
) -> Tuple[Path, Path]:
    """
    Gera `reports/relatorio_YYYYMMDD_HHMMSS.html` e sobrescreve `reports/relatorio_latest.html`.
    """
    pasta = raiz_projeto / "reports"
    pasta.mkdir(parents=True, exist_ok=True)
    texto = _conteudo_relatorio_html(arquivo_dados, n_linhas, sistema, bench, linguagem)
    ts = datetime.now().strftime("%Y%m%d_%H%M%S")
    p_timestamp = pasta / f"relatorio_{ts}.html"
    p_latest = pasta / "relatorio_latest.html"
    p_timestamp.write_text(texto, encoding="utf-8")
    p_latest.write_text(texto, encoding="utf-8")
    regenerar_indice_relatorios(pasta)
    return p_timestamp, p_latest
