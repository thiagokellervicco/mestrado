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
  - log detalhado por respondente, bloco e variável;
- `llm_run_metadata_...json`
  - metadados de execução (modelo, temperatura, retries, seed etc.).

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

- notebook executado com configuracao final;
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
- auditoria de incoerências detectadas no log.

Saídas esperadas:

- tabelas resumo;
- gráficos comparativos;
- uma seção de interpretação metodológica.

## 8.5) Seção metodológica no relatório principal

Incluir explicitamente:

- por que blocos foram usados (limite de contexto);
- por que bootstrap socio foi adotado;
- modelo e hiperparametros;
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
- [ ] Inserir resultados e limitações no relatório final.
- [ ] Referenciar seed, modelo e data para reprodutibilidade.

---

## 11) Conclusão

O notebook `Percepcao_Racismo_LLM.ipynb` já está estruturado como um **componente reutilizável e auditável** do projeto principal: recebe o instrumento original, gera respostas sintéticas coerentes por blocos, valida domínio de códigos e produz artefatos completos para análise e documentação científica.

A integração ao projeto principal deve tratar esse notebook como **etapa de geração de dados sintéticos**, seguida obrigatoriamente de **avaliação comparativa** e **interpretação metodológica**.
