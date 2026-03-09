#!/usr/bin/env python3
"""Adiciona gráficos de escalabilidade aos relatórios HTML existentes."""

import re
import os
from pathlib import Path

DOCS = Path(__file__).parent.parent / "docs"
SIZES = [1000, 10000, 100000]  # Ordem dos charts 1,2,3 e 4,5,6 e 7,8,9
COLORS = [
    "rgb(46, 213, 115)",
    "rgb(255, 138, 101)",
    "rgb(0, 210, 211)",
    "rgb(242, 183, 5)",
    "rgb(233, 69, 96)",
    "rgb(155, 89, 182)",
    "rgb(22, 33, 62)",
    "rgb(15, 52, 96)",
]

# Chart indices: 0-based. charts 1,2,3 = Aleatório; 4,5,6 = Crescente; 7,8,9 = Decrescente
CHART_GROUPS = [
    ("Aleatório", [0, 1, 2]),
    ("Crescente", [3, 4, 5]),
    ("Decrescente", [6, 7, 8]),
]


def extract_chart_data(html: str) -> list[tuple[list[str], list[float]]]:
    """Extrai labels e data de cada chart (1-9)."""
    pattern = r"labels:\s*\[(.*?)\],\s*datasets:\s*\[\{\s*[^}]*label:\s*'[^']*',\s*data:\s*\[(.*?)\]"
    matches = re.findall(pattern, html, re.DOTALL)
    charts = []
    for labels_str, data_str in matches[:9]:  # max 9 charts
        labels = [s.strip().strip('"') for s in re.findall(r'"([^"]*)"', labels_str)]
        data = []
        for m in re.findall(r"[\d.Ee+-]+", data_str):
            try:
                data.append(float(m))
            except ValueError:
                pass
        if labels and len(data) == len(labels):
            charts.append((labels, data))
    return charts


def build_scalability_section(charts: list) -> str:
    """Constrói o HTML da seção de escalabilidade."""
    if len(charts) < 6:  # Precisa de pelo menos 2 tamanhos por tipo (ex: 1k e 10k)
        return ""

    sb = []
    sb.append('\n<section>')
    sb.append('  <h2>Gráficos de Escalabilidade</h2>')
    sb.append('  <p class="chart-legend-note">Eixos X (tamanho) e Y (tempo) em escala log₁₀. Os tamanhos 1.000, 10.000 e 100.000 (potências de 10) ficam equidistantes.</p>')

    chart_id = 1001
    x_min = 500

    for desc, indices in CHART_GROUPS:
        if any(i >= len(charts) for i in indices):
            continue
        # Usar labels do primeiro chart do grupo (devem ser consistentes)
        labels = charts[indices[0]][0]
        # Coletar valores por algoritmo para cada tamanho
        datasets_js = []
        for alg_i, label in enumerate(labels):
            points = []
            for ii, size in enumerate(SIZES):
                if ii < len(indices) and indices[ii] < len(charts):
                    _, data = charts[indices[ii]]
                    if alg_i < len(data):
                        v = max(data[alg_i], 1e-9)
                        points.append(f"{{ x: {size}, y: {v} }}")
            if not points:
                continue
            color = COLORS[len(datasets_js) % len(COLORS)]
            label_esc = label.replace('\\', '\\\\').replace('"', '\\"')
            datasets_js.append(
                f'{{ label: "{label_esc}", data: [{", ".join(points)}], '
                f'borderColor: "{color}", backgroundColor: "{color}", fill: false, tension: 0.1, pointRadius: 6 }}'
            )
        if not datasets_js:
            continue

        sb.append(f'  <h3>{desc}</h3>')
        sb.append(f'  <div class="chart-container">')
        sb.append(f'    <canvas id="scalabilityChart{chart_id}"></canvas>')
        sb.append('  </div>')
        sb.append('  <script>')
        sb.append(f'  new Chart(document.getElementById("scalabilityChart{chart_id}"), {{')
        sb.append("    type: 'line',")
        sb.append('    data: {')
        sb.append(f'      datasets: [{", ".join(datasets_js)}]')
        sb.append('    },')
        sb.append('    options: {')
        sb.append('      responsive: true,')
        sb.append('      maintainAspectRatio: false,')
        sb.append("      plugins: { legend: { display: true, position: 'bottom', labels: { color: '#9aa8c2', usePointStyle: true } } },")
        sb.append('      scales: {')
        sb.append('        x: {')
        sb.append("          type: 'logarithmic',")
        sb.append(f'          min: {x_min},')
        sb.append("          ticks: { color: '#9aa8c2', callback: function(v) { return v >= 1000 ? (v/1000) + 'k' : v; } },")
        sb.append("          grid: { color: '#333' },")
        sb.append("          title: { display: true, text: 'Tamanho (n) — escala log₁₀', color: '#9aa8c2' }")
        sb.append('        },')
        sb.append('        y: {')
        sb.append("          type: 'logarithmic',")
        sb.append('          min: 1e-7,')
        sb.append('          ticks: {')
        sb.append("            color: '#9aa8c2',")
        sb.append('            callback: function(value) {')
        sb.append("              if (value >= 1) return value + ' s';")
        sb.append("              if (value >= 0.001) return (value * 1000).toFixed(0) + ' ms';")
        sb.append("              if (value >= 0.0001) return (value * 1e6).toFixed(0) + ' µs';")
        sb.append("              if (value >= 0.00001) return (value * 1e6).toFixed(0) + ' µs';")
        sb.append("              return (value * 1e6).toFixed(1) + ' µs';")
        sb.append('            }')
        sb.append('          },')
        sb.append("          grid: { color: '#333' },")
        sb.append("          title: { display: true, text: 'Tempo (escala log₁₀)', color: '#9aa8c2' }")
        sb.append('        }')
        sb.append('      }')
        sb.append('    }')
        sb.append('  });')
        sb.append('  </script>')
        chart_id += 1

    sb.append('</section>')
    return '\n'.join(sb)


def process_file(filepath: Path) -> bool:
    """Processa um arquivo HTML e insere a seção de escalabilidade."""
    html = filepath.read_text(encoding='utf-8')

    if 'Gr\u00e1ficos de Escalabilidade' in html or 'Gráficos de Escalabilidade' in html:
        return False  # Já tem

    charts = extract_chart_data(html)
    if len(charts) < 6:
        return False  # Relatorio-Demo tem só 3 charts com 10 elementos

    section = build_scalability_section(charts)
    if not section:
        return False

    # Inserir antes de <section class="analise">
    marker = '<section class="analise">'
    if marker not in html:
        return False
    new_html = html.replace(
        '</section>\n\n<section class="analise">',
        f'</section>{section}\n\n<section class="analise">'
    )
    if new_html == html:
        # Tentar com apenas uma quebra
        new_html = html.replace(
            '</section>\n<section class="analise">',
            f'</section>{section}\n<section class="analise">'
        )
    if new_html == html:
        return False
    filepath.write_text(new_html, encoding='utf-8')
    return True


def main():
    os.chdir(DOCS.parent)
    for f in sorted(DOCS.glob('Relatorio*.html')):
        if 'index' in f.name:
            continue
        if process_file(f):
            print(f'Atualizado: {f.name}')
        else:
            print(f'Pulado: {f.name}')


if __name__ == '__main__':
    main()
