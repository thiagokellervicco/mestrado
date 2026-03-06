# Algoritmos de Ordenação

Projeto de Mestrado – Comparação de desempenho entre algoritmos clássicos de ordenação.

**Aluno:** Thiago Keller | **RA:** 10779365 | **Curso:** Mestrado de Computação Aplicada | **Instituição:** Universidade Presbiteriana Mackenzie | **Professora:** Dra Valeria Farinazzo Martins | **Ano:** 2026 (1º semestre)

## Sobre o Projeto

Este projeto implementa uma comparação de desempenho entre diversos algoritmos de ordenação em C#, gerando relatórios HTML com resultados, gráficos e análise de complexidade assintótica.

### Algoritmos Implementados

| Algoritmo | Complexidade | Descrição |
|-----------|--------------|-----------|
| Bubble Sort (Original) | O(n²) | Implementação clássica |
| Selection Sort | O(n²) | Seleção do menor elemento |
| Insertion Sort | O(n²) | Inserção ordenada |
| Merge Sort | O(n log n) | Divisão e conquista |
| Quick Sort | O(n log n) | Pivô fixo |
| Quick Sort (Pivô Aleatório) | O(n log n) | Pivô aleatório para evitar pior caso |
| Array.Sort (C# Nativo) | O(n log n) | IntroSort da biblioteca .NET |

### Algoritmos Otimizados

Versões otimizadas disponíveis com `--optimized`. Fontes:

| Algoritmo | Otimização | Fonte |
|-----------|------------|-------|
| Bubble Sort (Optimized) | Early exit quando não há trocas | Literatura de algoritmos |
| Insertion Sort (Sentinel) | Sentinela em A[0] elimina checagem `j ≥ 0` | Cormen et al., Cap. 2 |
| Selection Sort (Double) | Busca menor e maior por passagem | Variação clássica |
| Merge Sort (Single Aux Array) | Um único array auxiliar | Cormen et al., Cap. 2 |
| Quick Sort (Hoare + Median + Insertion) | Hoare + mediana-de-três + Insertion (n≤15) | Cormen Cap. 7, Sedgewick (mediana) |

### Tipos de Entrada

- **Aleatório** – Vetores com elementos pseudoaleatórios
- **Crescente** – Dados já ordenados
- **Decrescente** – Dados em ordem inversa

## Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download) ou superior

## Como Executar

```bash
# Navegar até o projeto
cd "Projeto 1 - Algoritmo"

# Demonstração rápida (10 registros, entrada e saída)
dotnet run --project src/AlgoritmosOrdenacao -- --demo

# Relatório Demo: executa benchmark com 10 elementos e salva em docs/Relatorio-Demo.html
dotnet run --project src/AlgoritmosOrdenacao -- --demo-report

# Teste completo (1.000, 10.000, 100.000 elementos)
dotnet run --project src/AlgoritmosOrdenacao

# Com algoritmos otimizados (padrão + otimizados)
dotnet run --project src/AlgoritmosOrdenacao -- --optimized

# Modo rápido (1.000 e 10.000 elementos, para validação)
dotnet run --project src/AlgoritmosOrdenacao -- --rapido

# Modo ultra (inclui 10M elementos, apenas O(n log n))
dotnet run --project src/AlgoritmosOrdenacao -- --ultra
```

### Por que o modo ultra usa apenas O(n log n) em 10M elementos?

Algoritmos O(n²) — Bubble Sort, Selection Sort, Insertion Sort — tornam-se impraticáveis com vetores muito grandes:

| Tamanho    | O(n log n) | O(n²)     |
|------------|------------|-----------|
| 100.000    | segundos   | minutos   |
| 10.000.000 | poucos min | horas/dias|

Com 10 milhões de elementos, O(n²) significa ~100 trilhões de operações, o que levaria horas ou dias. Já O(n log n) é ~230 milhões de operações — viável em poucos minutos. Por isso, para tamanhos ≥ 1M, o modo ultra executa apenas Merge Sort, Quick Sort e Array.Sort.

## Estrutura do Projeto

```
├── docs/                    # Relatórios HTML (GitHub Pages)
│   ├── index.html           # Página inicial (info da máquina, algoritmos otimizados)
│   └── Relatorio-*.html     # Relatórios gerados
├── src/AlgoritmosOrdenacao/
│   ├── Algoritmos/          # Algoritmos clássicos
│   ├── AlgoritmosOtimizados/  # Versões otimizadas (Bubble, Insertion, Selection, Merge, Quick)
│   ├── Core/                # Runner e Result
│   ├── Relatorio/           # ReportGenerator, MachineInfo
│   └── Program.cs
├── Relatorio/               # Cópias locais dos relatórios
└── README.md
```

## Relatórios

A página inicial (`docs/index.html`) concentra informações da máquina e a descrição dos algoritmos otimizados. Os relatórios incluem:

- Estado da execução (fonte de energia, memória GC)
- Tabelas com tempos (mediana, mínimo, máximo)
- Gráficos interativos (Chart.js)
- Análise de eficiência e complexidade assintótica

## Referências

- Cormen, T. H., et al. *Introduction to Algorithms* (3ª edição), MIT Press.  
  Algoritmos base e otimizações: Insertion Sort com sentinela (Cap. 2), Merge Sort com array auxiliar único (Cap. 2), particionamento de Hoare no Quick Sort (Cap. 7).
- Sedgewick, R. *Algorithms* (4ª edição). Mediana de três como pivô no Quick Sort.
- Documentação .NET – [Array.Sort](https://learn.microsoft.com/dotnet/api/system.array.sort) (IntroSort).
