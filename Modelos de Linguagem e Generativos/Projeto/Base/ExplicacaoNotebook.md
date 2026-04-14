# Documentação Detalhada do Notebook `Percepcao_Racismo_LLM.ipynb`

## 1) Objetivo do notebook no projeto

Este notebook implementa a etapa de **simulação de respostas de opinião pública com LLM** para a pesquisa CESOP 04829, cujo tema é a percepção dos brasileiros sobre racismo no Brasil.

Na prática, ele:

- usa a base empírica real (`04829.SAV`, 2000 entrevistas, 144 variáveis);
- usa um dicionário estruturado (`survey_dictionary.json`) com blocos e códigos de resposta;
- gera respondentes sintéticos em formato de questionário completo;
- preserva a estrutura de colunas e códigos numéricos do dado original;
- registra logs de validação para auditoria metodológica.

Esse notebook representa, portanto, o **módulo de geração sintética** do trabalho.

Link do notebook no Google Colab:

- https://colab.research.google.com/drive/1GUEIalxhL5_DF2hvsKSIsAHUc00Q7-zG?usp=sharing

---

## 2) O que entra e o que sai

## Entradas obrigatórias

- `04829.SAV`
  - microdado original em formato SPSS.
- `survey_dictionary.json`
  - dicionário de variáveis, rótulos e blocos de aplicação no LLM.

Ambos são esperados em:

- `/content/drive/MyDrive/DadosModeloLinguagens`

## Saídas geradas

Em `/content/drive/MyDrive/DadosModeloLinguagens/saida_simulacao`, o notebook grava:

- `sintetico_04829_...csv`
  - tabela sintética final;
- `sintetico_04829_log_...json`
  - log detalhado por respondente, bloco e variável; em cada respondente há também **`empirical_seed_index`** (índice da linha do `04829.SAV` usada no bootstrap do perfil sócio), permitindo parear empírico × sintético para métricas no notebook;
- `llm_run_metadata_...json`
  - metadados de execução (modelo, temperatura, retries, seed etc.).

### 2.1) Tratamento das variáveis (documentar no relatório)

O grupo precisa de uma seção explícita sobre **como cada variável é tratada** do microdado até a saída sintética. Abaixo está o **fluxo real do notebook** (do arquivo ao CSV), para copiar/adaptar no relatório.

#### Pipeline por etapa

1. **Leitura empírica** — `pyreadstat.read_sav("04829.SAV")` → `DataFrame` com **144 colunas** na ordem original; `assert` garante esse número.
2. **Dicionário** — `survey_dictionary.json` fornece, por variável: nome, rótulo (`label`), `value_labels` (mapa código → texto) e participação em **blocos** (`suggested_llm_blocks`).
3. **Blocos LLM** — o questionário é enviado ao modelo em partes (blocos); variáveis do bloco `socio_demografia` são listadas em `SOCIO_VARS` quando há bootstrap sócio.
4. **Por respondente sintético**
   - sorteia-se uma **linha empírica** (`df_emp.sample(...)`) como semente;
   - o índice dessa linha é gravado no log como **`empirical_seed_index`** (pareamento empírico × sintético para avaliação);
   - se `USE_BOOTSTRAP_SOCIO=True`: o bloco `socio_demografia` **não** passa pelo LLM — os valores dessas colunas são **copiados** da linha sorteada;
   - demais blocos: o modelo (ou mock) devolve um **JSON** com códigos numéricos por variável.
5. **Validação** — `validate_and_merge` converte respostas para número e confere `value_labels` quando existem; códigos fora do domínio viram `NaN` e entram no log (`invalid_code`, `missing_key`).
6. **Saída** — uma linha por respondente, **mesmas 144 colunas** que o `.SAV`; faltantes como `NaN` onde aplicável.

#### O que registrar na tabela do relatório

| Tópico | O que registrar |
|--------|-----------------|
| **Origem** | Leitura do `04829.SAV` (SPSS) e mapeamento para DataFrame; nomes e ordem das 144 colunas. |
| **Metadados** | Uso do `survey_dictionary.json`: blocos, texto das perguntas, opções e **códigos numéricos** permitidos por variável. |
| **Tipos / papel no pipeline** | Escolha única com código fechado (maioria das variáveis com `value_labels`); variáveis **sem** `value_labels` no dicionário exigem cuidado extra na validação e na interpretação das métricas. |
| **Valores válidos** | Domínio de códigos (`value_labels`); resposta inválida → `NaN` + registro no log. |
| **Faltantes** | `NaN` no empírico e no sintético; comparar **taxa de missing** por variável antes/depois. |
| **Bootstrap sócio** | Lista de colunas em `socio_demografia` (copiadas da linha sorteada); restante gerado por bloco. |
| **Pós-processamento** | Coerção float/int no código; nenhuma recodificação “oculta” além da validação de domínio descrita acima. |

Isso atende ao pedido de **tratamento das variáveis** de forma auditável para o trabalho em grupo.

### 2.2) Métricas separadas (acurácia, precisão, recall, F1, etc.)

Pedido do grupo: **não** resumir tudo em um único número — **separar e nomear** cada métrica (colunas distintas na tabela ou subseções no texto).

#### Comparação adotada no notebook

Na seção **“Avaliação empírico × sintético (métricas separadas)”** do `Percepcao_Racismo_LLM.ipynb`, cada linha sintética é **pareada** com a linha empírica cujo índice está em `empirical_seed_index` no `log`. Para **cada variável** que tenha `value_labels` no dicionário:

- \(y_{\text{true}}\) = código na base empírica (linha pareada);
- \(y_{\text{pred}}\) = código na base sintética (mesma ordem de linhas).

Linhas em que `y_true` ou `y_pred` é `NaN` são **ignoradas** naquela variável (comparação por variável, não globais misturadas).

#### Glossário — uma métrica por linha (multiclasse)

Considere cada **código de resposta** como uma **classe**. Para a classe \(c\):

| Métrica | Nome em inglês | Ideia (uma frase) |
|--------|----------------|-------------------|
| **Acurácia** | _Accuracy_ | Fração de observações em que o código previsto é **igual** ao verdadeiro: \(\frac{1}{N}\sum_i \mathbf{1}[\hat{y}_i = y_i]\). **Uma linha por variável** no notebook (`acuracia`). |
| **Precisão (classe \(c\))** | _Precision_ | Entre tudo que foi previsto como \(c\), quanto de fato era \(c\): \( \text{TP}_c / (\text{TP}_c + \text{FP}_c) \). |
| **Revocação / sensibilidade (classe \(c\))** | _Recall_ | Entre todos os verdadeiros \(c\), quanto o modelo “acertou”: \( \text{TP}_c / (\text{TP}_c + \text{FN}_c) \). |
| **F1 (classe \(c\))** | _F1-score_ | Média harmônica de precisão e revocação na classe \(c\); equilibra os dois quando há desbalanceamento. |
| **Suporte (classe \(c\))** | _Support_ | Número de exemplos com rótulo verdadeiro \(c\) no conjunto usado naquela variável (útil para não overinterpretar F1 em classes raras). |

#### Agregação **macro** vs **micro** (como no código)

O notebook calcula, por variável, também:

| Coluna no `DataFrame` de métricas | Significado |
|-----------------------------------|-------------|
| `precisao_macro`, `recall_macro`, `f1_macro` | Média **não ponderada** das métricas por classe (cada classe pesa igual). |
| `precisao_micro`, `recall_micro`, `f1_micro` | Agrega **todos** os acertos/erros das classes e depois calcula uma única precisão/recall/F1 (equivale a dar peso proporcional à frequência da classe). |

**Não misturar** macro e micro no texto sem dizer qual foi usada; no relatório, vale apresentar **as duas** ou justificar uma delas.

#### O que não entra nessa tabela automática

- Variáveis **sem** `value_labels` no `survey_dictionary.json` são **puladas** pelo laço de métricas (ou podem ser avaliadas com outras ferramentas: distribuição marginal, teste \(\chi^2\), distância entre distribuições, etc.).
- Métricas de **classificação** pressupõem códigos comparáveis; não substituem análise de **distribuição** quando o objetivo for só “parecer com o empírico” sem pareamento por linha.

#### Implementação de referência

- Python: `sklearn.metrics` — `accuracy_score`, `precision_recall_fscore_support` com `average="macro"` e `average="micro"`, e `labels=` alinhados aos códigos observados (`labels_used` no notebook).
- Opcional no relatório: `classification_report` por variável-chave ou `confusion_matrix` para ilustrar erros entre códigos.

Biblioteca de referência em Python: `sklearn.metrics` (`accuracy_score`, `precision_recall_fscore_support`, `classification_report`, `confusion_matrix`), sempre com **rótulos** (`labels=`) alinhados aos códigos do questionário.

---

## 3) Estrutura de execução do notebook

O notebook foi organizado para ser executado de cima para baixo:

1. **Instalação de dependências**
   - `pyreadstat`, `pandas`, `tqdm`, `transformers`, `accelerate`, `safetensors`, `bitsandbytes`.

2. **Configuração geral**
   - mount do Google Drive;
   - definição de paths de entrada e saída;
   - definição de hiperparâmetros da simulação e do LLM.

3. **Carga e verificação dos dados**
   - leitura do `.SAV` e do JSON de dicionário;
   - verificação de existência de arquivos;
   - assert de consistência (144 colunas esperadas).

4. **Funções de prompt e validação**
   - montagem do prompt por bloco;
   - coerção e validação de códigos numéricos;
   - extração robusta de JSON da saída do modelo.

5. **Camada de inferência (mock ou LLM real)**
   - mock para teste de pipeline;
   - Qwen2.5-3B-Instruct 4-bit para geração real.

6. **Loop principal de simulação**
   - cria contexto inicial por respondente;
   - percorre blocos;
   - chama LLM;
   - valida, corrige e agrega resultados;
   - salva arquivos finais.

---

## 4) Parâmetros principais e impacto no experimento

## Tamanho da amostra sintética

- `N_RESPONDENTES = 200`
  - controla quantos questionários sintéticos serão gerados.

## Reprodutibilidade

- `RNG_SEED = 42`
  - fixa amostragem e escolhas pseudoaleatórias.

## Bootstrap de perfil sócio

- `USE_BOOTSTRAP_SOCIO = True`
  - bloco `socio_demografia` é herdado da base real;
  - demais blocos são preenchidos pelo LLM.

## Tipo de geração

- `USE_MOCK_LLM = True`
  - pipeline sem rede neural (teste de lógica).
- `USE_MOCK_LLM = False`
  - usa modelo real `Qwen/Qwen2.5-3B-Instruct` em 4-bit.

## Parâmetros de inferência

- `LLM_TEMPERATURE = 0.25`
- `LLM_MAX_NEW_TOKENS = 4096`
- `LLM_MAX_INPUT_TOKENS = 8192`
- `LLM_N_RETRIES = 3`
- `LLM_FALLBACK_MOCK_ON_FAILURE = True`

Esses parâmetros definem equilíbrio entre variabilidade de respostas, custo computacional e robustez contra saídas inválidas.

## Teste rápido

- `RUN_SMOKE_TEST = True`
  - executa 1 respondente e 1 bloco LLM (após sócio);
  - gera arquivos com sufixo `_SMOKE_`.

Recomendado para validar o ambiente antes da rodada completa.

---

## 5) Lógica metodológica de simulação

## 5.1) Contexto cumulativo por respondente

Para cada respondente sintético:

1. o notebook cria um contexto inicial (normalmente socio-demografico real, via bootstrap);
2. envia um bloco de perguntas por vez ao LLM;
3. adiciona respostas validadas ao contexto;
4. usa esse contexto expandido no próximo bloco.

Isso reforça **coerência interna** entre respostas ao longo do questionário.

## 5.2) Prompting por bloco

A função `build_block_prompt(...)` injeta:

- instrução comportamental do agente;
- exigência de saída em JSON puro;
- perfil/respostas prévias;
- enunciados e opções por variável.

Como o questionário é dividido em blocos, o notebook respeita limite de contexto sem perder continuidade semântica.

## 5.3) Parse robusto de saída

A função `extract_json_object(...)`:

- remove fences markdown, se existirem;
- localiza o primeiro objeto JSON válido;
- falha explicitamente se não achar JSON parseável.

## 5.4) Validação de domínio

A função `validate_and_merge(...)`:

- checa chave ausente (`missing_key`);
- checa código inválido (`invalid_code`);
- converte o que for válido para formato numérico;
- grava inconsistências no log;
- usa `NaN` para ausência/invalidade.

Esse ponto é central para manter compatibilidade analítica com o banco original.

---

## 6) Modo Mock vs Modo LLM Real

## Modo mock

Serve para depuração de pipeline:

- não carrega modelo;
- amostra códigos válidos com base no empírico;
- testa todo o fluxo de leitura, validação e exportação.

## Modo LLM real

Usa `Qwen/Qwen2.5-3B-Instruct` com quantização 4-bit:

- menor consumo de VRAM;
- viável em GPU T4 no Colab;
- retries automáticos quando a resposta não vem em JSON válido.

Se `LLM_FALLBACK_MOCK_ON_FAILURE=True`, blocos com falha recorrente podem ser preenchidos por mock, e isso deve ser registrado no relatório metodológico.

---

## 7) Principais artefatos para o relatório final

Para documentação acadêmica/reprodutibilidade, os artefatos mínimos são:

- notebook executado com configuração final;
- CSV sintético final;
- log JSON da rodada;
- metadata JSON com hiperparâmetros;
- descrição do ambiente (Colab GPU, versões de libs, data de execução).

---

## 8) Como juntar esse notebook no projeto principal

Esta é a parte de integração recomendada para o trabalho final.

## 8.1) Posicionamento no pipeline geral do projeto

Sugestão de macrofluxo:

1. **Dados originais e dicionário**  
   (`04829.SAV` + `survey_dictionary.json`)
2. **Geração sintética com LLM**  
   (`Percepcao_Racismo_LLM.ipynb`)
3. **Pós-processamento e análise comparativa**  
   (distribuições, cruzamentos, métricas)
4. **Relatório e visualizações finais**

Ou seja, este notebook é a ponte entre base empírica e base sintética.

## 8.2) Entregável de integração

Definir um "pacote de rodada" padronizado por execução:

- `sintetico_04829_N...csv`
- `sintetico_04829_log_N...json`
- `llm_run_metadata_N...json`
- identificador da rodada (data + seed + modelo)

Exemplo de id:

- `run_2026-03-30_seed42_qwen25-3b`

Isso facilita comparação entre rodadas e rastreabilidade.

## 8.3) Contrato de dados com os outros módulos

Para integrar sem retrabalho, manter:

- mesmas 144 colunas do banco original;
- mesmos códigos numéricos por variável;
- `NaN` como representação de faltantes;
- formato tabular único (`CSV`) para consumo pelos módulos de análise.

## 8.4) Módulo de análise comparativa (projeto principal)

Criar (ou conectar) um script/notebook de avaliação com, no mínimo:

- comparação marginal por variável (empírico vs sintético);
- comparação de pares relevantes (contingências);
- taxa de missing por variável;
- cobertura de códigos válidos;
- auditoria de incoerências detectadas no log;
- **métricas separadas** conforme a seção **2.2** (acurácia, precisão, revocação, F1, suporte — não misturar num único “índice” sem nome);
- **tratamento das variáveis** documentado conforme a seção **2.1**.

Saídas esperadas:

- tabelas resumo;
- gráficos comparativos;
- tabelas por métrica (ou `classification_report` por variável, com leitura explícita no texto);
- uma seção de interpretação metodológica.

## 8.5) Seção metodológica no relatório principal

Incluir explicitamente:

- por que blocos foram usados (limite de contexto);
- por que bootstrap sócio foi adotado;
- modelo e hiperparâmetros;
- critério de retries e fallback;
- limitações (possível suavização de respostas, viés de modelo etc.).

## 8.6) Operação em duas fases (recomendação prática)

Para rodar com mais segurança:

1. **Fase 1 - validação técnica**
   - `RUN_SMOKE_TEST=True`
   - `USE_MOCK_LLM=True` e depois `False`
2. **Fase 2 - rodada oficial**
   - `RUN_SMOKE_TEST=False`
   - `N_RESPONDENTES` definido pelo grupo
   - salvamento e congelamento dos artefatos de rodada

---

## 9) Riscos conhecidos e mitigações

- **Saída fora de JSON**  
  Mitigação: retries + parser robusto.

- **Código inválido para variável fechada**  
  Mitigação: validação por `value_labels` + log.

- **Falha de GPU/ambiente**  
  Mitigação: modo mock e teste smoke.

- **Dependência de token HF/rate limit**  
  Mitigação: configurar `HF_TOKEN` para estabilidade.

- **Diferença entre real e sintético**  
  Mitigação: avaliação comparativa sistemática no módulo de análise.

---

## 10) Checklist de integração no projeto principal

- [ ] Rodar smoke test e validar arquivos de saída.
- [ ] Rodar simulação oficial com parâmetros definidos pelo grupo.
- [ ] Congelar pacote de rodada (CSV + log + metadata).
- [ ] Executar módulo comparativo empírico vs sintético.
- [ ] Documentar **tratamento das variáveis** (seção 2.1) no relatório.
- [ ] Reportar **métricas separadas** — acurácia, precisão, recall, F1 (e suporte), sem agrupar tudo num único número opaco (seção 2.2).
- [ ] Inserir resultados e limitações no relatório final.
- [ ] Referenciar seed, modelo e data para reprodutibilidade.

---

## 11) Conclusão

O notebook `Percepcao_Racismo_LLM.ipynb` já está estruturado como um **componente reutilizável e auditável** do projeto principal: recebe o instrumento original, gera respostas sintéticas coerentes por blocos, valida domínio de códigos e produz artefatos completos para análise e documentação científica.

A integração ao projeto principal deve tratar esse notebook como **etapa de geração de dados sintéticos**, seguida obrigatoriamente de **avaliação comparativa** e **interpretação metodológica**.
