"""Carga de dados (CSV ou Excel) e construção das árvores."""

import time
from pathlib import Path
from typing import Tuple, Union

import pandas as pd

from .arvore_avl import ArvoreAVL
from .arvore_binaria import ArvoreBinariaBusca
from .estudante import Estudante


def linha_para_estudante(row: dict) -> Estudante:
    return Estudante(
        student_id=int(row["student_id"]),
        age=int(row["age"]),
        gender=str(row["gender"]),
        study_hours=float(row["study_hours"]),
        attendance=float(row["attendance"]),
        sleep_hours=float(row["sleep_hours"]),
        previous_grade=float(row["previous_grade"]),
        assignments_completed=float(row["assignments_completed"]),
        practice_tests_taken=float(row["practice_tests_taken"]),
        group_study_hours=float(row["group_study_hours"]),
        notes_quality_score=float(row["notes_quality_score"]),
        time_management_score=float(row["time_management_score"]),
        motivation_level=float(row["motivation_level"]),
        mental_health_score=float(row["mental_health_score"]),
        screen_time=float(row["screen_time"]),
        social_media_hours=float(row["social_media_hours"]),
        family_income=str(row["family_income"]),
        parent_education=str(row["parent_education"]),
        internet_access=str(row["internet_access"]),
        device_type=str(row["device_type"]),
        school_type=str(row["school_type"]),
        extracurriculars=str(row["extracurriculars"]),
        final_grade=float(row["final_grade"]),
        grade_category=str(row["grade_category"]),
        pass_fail=str(row["pass_fail"]),
    )


def _ler_dataframe(caminho: Path) -> pd.DataFrame:
    suf = caminho.suffix.lower()
    if suf == ".csv":
        return pd.read_csv(caminho)
    if suf in (".xlsx", ".xls"):
        return pd.read_excel(caminho)
    raise ValueError(f"Formato não suportado: {caminho.suffix} (use .csv, .xlsx ou .xls)")


def carregar_dataset(caminho: Union[str, Path]) -> Tuple[ArvoreBinariaBusca, ArvoreAVL, float, float, int]:
    """
    Lê CSV ou Excel, monta a lista de Estudante e insere na ABB e na AVL.
    Retorna (abb, avl, t_insercao_abb, t_insercao_avl, n_linhas).
    Usa to_dict('records') em vez de iterrows() — bem mais rápido em bases grandes.
    """
    caminho = Path(caminho)
    df = _ler_dataframe(caminho)
    n_linhas = len(df)
    estudantes = [linha_para_estudante(r) for r in df.to_dict("records")]

    t0 = time.perf_counter()
    abb = ArvoreBinariaBusca()
    for e in estudantes:
        abb.inserir(e)
    t_abb = time.perf_counter() - t0

    t0 = time.perf_counter()
    avl = ArvoreAVL()
    for e in estudantes:
        avl.inserir(e)
    t_avl = time.perf_counter() - t0

    return abb, avl, t_abb, t_avl, n_linhas


def carregar_excel(caminho: Union[str, Path]) -> Tuple[ArvoreBinariaBusca, ArvoreAVL, float, float]:
    """Compatibilidade: mesmo retorno de antes (sem contagem de linhas)."""
    abb, avl, t_abb, t_avl, _ = carregar_dataset(caminho)
    return abb, avl, t_abb, t_avl
