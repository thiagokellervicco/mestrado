"""Árvore Binária de Busca (ABB) indexada por student_id."""

from typing import List, Optional

from .estudante import Estudante
from .nos import No


class ArvoreBinariaBusca:
    def __init__(self):
        self.raiz: Optional[No] = None
        self._total_nos: int = 0

    # ── Inserção ──────────────────────────
    def inserir(self, estudante: Estudante) -> None:
        novo_no = No(estudante=estudante)
        if self.raiz is None:
            self.raiz = novo_no
            self._total_nos += 1
            return
        if self._inserir_recursivo(self.raiz, novo_no):
            self._total_nos += 1

    def _inserir_recursivo(self, atual: No, novo: No) -> bool:
        if novo.chave < atual.chave:
            if atual.esquerda is None:
                atual.esquerda = novo
                return True
            return self._inserir_recursivo(atual.esquerda, novo)
        if novo.chave > atual.chave:
            if atual.direita is None:
                atual.direita = novo
                return True
            return self._inserir_recursivo(atual.direita, novo)
        return False  # chave duplicada: ignora

    # ── Busca ─────────────────────────────
    def buscar(self, student_id: int) -> Optional[Estudante]:
        no = self._buscar_recursivo(self.raiz, student_id)
        return no.estudante if no else None

    def _buscar_recursivo(self, atual: Optional[No], chave: int) -> Optional[No]:
        if atual is None or atual.chave == chave:
            return atual
        if chave < atual.chave:
            return self._buscar_recursivo(atual.esquerda, chave)
        return self._buscar_recursivo(atual.direita, chave)

    # ── Remoção ───────────────────────────
    def remover(self, student_id: int) -> bool:
        self.raiz, removido = self._remover_recursivo(self.raiz, student_id)
        if removido:
            self._total_nos -= 1
        return removido

    def _remover_recursivo(self, atual: Optional[No], chave: int):
        if atual is None:
            return atual, False
        removido = False
        if chave < atual.chave:
            atual.esquerda, removido = self._remover_recursivo(atual.esquerda, chave)
        elif chave > atual.chave:
            atual.direita, removido = self._remover_recursivo(atual.direita, chave)
        else:
            removido = True
            if atual.esquerda is None:
                return atual.direita, removido
            elif atual.direita is None:
                return atual.esquerda, removido
            # Substitui pelo sucessor (menor da subárvore direita)
            sucessor = self._minimo(atual.direita)
            atual.estudante = sucessor.estudante
            atual.direita, _ = self._remover_recursivo(atual.direita, sucessor.chave)
        return atual, removido

    # ── Mínimo / Máximo ───────────────────
    def _minimo(self, no: No) -> No:
        while no.esquerda:
            no = no.esquerda
        return no

    def minimo(self) -> Optional[Estudante]:
        if not self.raiz:
            return None
        return self._minimo(self.raiz).estudante

    def maximo(self) -> Optional[Estudante]:
        if not self.raiz:
            return None
        no = self.raiz
        while no.direita:
            no = no.direita
        return no.estudante

    # ── Altura ────────────────────────────
    def altura(self) -> int:
        return self._altura_recursivo(self.raiz)

    def _altura_recursivo(self, no: Optional[No]) -> int:
        if no is None:
            return 0
        return 1 + max(
            self._altura_recursivo(no.esquerda),
            self._altura_recursivo(no.direita)
        )

    # ── Traversals ────────────────────────
    def em_ordem(self) -> List[Estudante]:
        resultado: List[Estudante] = []
        self._em_ordem_recursivo(self.raiz, resultado)
        return resultado

    def _em_ordem_recursivo(self, no: Optional[No], lista: List[Estudante]) -> None:
        if no:
            self._em_ordem_recursivo(no.esquerda, lista)
            lista.append(no.estudante)
            self._em_ordem_recursivo(no.direita, lista)

    def pre_ordem(self) -> List[Estudante]:
        resultado: List[Estudante] = []
        self._pre_ordem_recursivo(self.raiz, resultado)
        return resultado

    def _pre_ordem_recursivo(self, no: Optional[No], lista: List[Estudante]) -> None:
        if no:
            lista.append(no.estudante)
            self._pre_ordem_recursivo(no.esquerda, lista)
            self._pre_ordem_recursivo(no.direita, lista)

    def pos_ordem(self) -> List[Estudante]:
        resultado: List[Estudante] = []
        self._pos_ordem_recursivo(self.raiz, resultado)
        return resultado

    def _pos_ordem_recursivo(self, no: Optional[No], lista: List[Estudante]) -> None:
        if no:
            self._pos_ordem_recursivo(no.esquerda, lista)
            self._pos_ordem_recursivo(no.direita, lista)
            lista.append(no.estudante)

    # ── Busca por faixa ───────────────────
    def buscar_por_faixa(self, id_min: int, id_max: int) -> List[Estudante]:
        resultado: List[Estudante] = []
        self._busca_faixa_recursiva(self.raiz, id_min, id_max, resultado)
        return resultado

    def _busca_faixa_recursiva(
        self, no: Optional[No], id_min: int, id_max: int, lista: List[Estudante]
    ) -> None:
        if no is None:
            return
        if no.chave > id_min:
            self._busca_faixa_recursiva(no.esquerda, id_min, id_max, lista)
        if id_min <= no.chave <= id_max:
            lista.append(no.estudante)
        if no.chave < id_max:
            self._busca_faixa_recursiva(no.direita, id_min, id_max, lista)

    # ── Busca por nota final (faixa) ──────
    def buscar_por_nota(self, nota_min: float, nota_max: float) -> List[Estudante]:
        resultado: List[Estudante] = []
        self._busca_nota_recursiva(self.raiz, nota_min, nota_max, resultado)
        return resultado

    def _busca_nota_recursiva(
        self, no: Optional[No], nota_min: float, nota_max: float, lista: List[Estudante]
    ) -> None:
        if no is None:
            return
        self._busca_nota_recursiva(no.esquerda, nota_min, nota_max, lista)
        if nota_min <= no.estudante.final_grade <= nota_max:
            lista.append(no.estudante)
        self._busca_nota_recursiva(no.direita, nota_min, nota_max, lista)

    # ── Busca por aprovação ────────────────
    def buscar_aprovados(self) -> List[Estudante]:
        return self._busca_por_campo(self.raiz, "pass_fail", "Pass")

    def buscar_reprovados(self) -> List[Estudante]:
        return self._busca_por_campo(self.raiz, "pass_fail", "Fail")

    def _busca_por_campo(self, no: Optional[No], campo: str, valor) -> List[Estudante]:
        if no is None:
            return []
        resultado = []
        resultado += self._busca_por_campo(no.esquerda, campo, valor)
        if getattr(no.estudante, campo) == valor:
            resultado.append(no.estudante)
        resultado += self._busca_por_campo(no.direita, campo, valor)
        return resultado

    # ── Busca por idade ────────────────────
    def buscar_por_idade(self, idade: int) -> List[Estudante]:
        return self._busca_por_campo(self.raiz, "age", idade)

    # ── Busca por gênero ───────────────────
    def buscar_por_genero(self, genero: str) -> List[Estudante]:
        return self._busca_por_campo(self.raiz, "gender", genero)

    # ── Propriedades ──────────────────────
    @property
    def total(self) -> int:
        return self._total_nos

    def __len__(self) -> int:
        return self._total_nos

    def __contains__(self, student_id: int) -> bool:
        return self.buscar(student_id) is not None

    def __str__(self) -> str:
        return (
            f"ArvoreBinariaBusca("
            f"total={self._total_nos}, "
            f"altura={self.altura()}, "
            f"raiz_id={self.raiz.chave if self.raiz else None})"
        )
