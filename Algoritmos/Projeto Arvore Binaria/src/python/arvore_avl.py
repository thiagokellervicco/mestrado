"""Árvore AVL indexada por student_id."""

from typing import List, Optional

from .estudante import Estudante
from .nos import NoAVL


class ArvoreAVL:
    def __init__(self):
        self.raiz: Optional[NoAVL] = None
        self._total_nos: int = 0

    # ── Utilitários de altura e balanço ───
    def _altura_no(self, no: Optional[NoAVL]) -> int:
        return no.altura if no else 0

    def _atualizar_altura(self, no: NoAVL) -> None:
        no.altura = 1 + max(self._altura_no(no.esquerda), self._altura_no(no.direita))

    def _fator_balanceamento(self, no: NoAVL) -> int:
        return self._altura_no(no.esquerda) - self._altura_no(no.direita)

    # ── Rotações ──────────────────────────
    def _rotacao_direita(self, y: NoAVL) -> NoAVL:
        x = y.esquerda
        T2 = x.direita
        x.direita = y
        y.esquerda = T2
        self._atualizar_altura(y)
        self._atualizar_altura(x)
        return x

    def _rotacao_esquerda(self, x: NoAVL) -> NoAVL:
        y = x.direita
        T2 = y.esquerda
        y.esquerda = x
        x.direita = T2
        self._atualizar_altura(x)
        self._atualizar_altura(y)
        return y

    def _balancear(self, no: NoAVL) -> NoAVL:
        self._atualizar_altura(no)
        fb = self._fator_balanceamento(no)

        # Caso LL
        if fb > 1 and self._fator_balanceamento(no.esquerda) >= 0:
            return self._rotacao_direita(no)
        # Caso LR
        if fb > 1 and self._fator_balanceamento(no.esquerda) < 0:
            no.esquerda = self._rotacao_esquerda(no.esquerda)
            return self._rotacao_direita(no)
        # Caso RR
        if fb < -1 and self._fator_balanceamento(no.direita) <= 0:
            return self._rotacao_esquerda(no)
        # Caso RL
        if fb < -1 and self._fator_balanceamento(no.direita) > 0:
            no.direita = self._rotacao_direita(no.direita)
            return self._rotacao_esquerda(no)

        return no

    # ── Inserção ──────────────────────────
    def inserir(self, estudante: Estudante) -> None:
        self.raiz = self._inserir_recursivo(self.raiz, estudante)
        self._total_nos += 1

    def _inserir_recursivo(self, no: Optional[NoAVL], estudante: Estudante) -> NoAVL:
        if no is None:
            return NoAVL(estudante=estudante)
        if estudante.student_id < no.chave:
            no.esquerda = self._inserir_recursivo(no.esquerda, estudante)
        elif estudante.student_id > no.chave:
            no.direita = self._inserir_recursivo(no.direita, estudante)
        else:
            # chave duplicada: ignora
            self._total_nos -= 1
            return no
        return self._balancear(no)

    # ── Busca ─────────────────────────────
    def buscar(self, student_id: int) -> Optional[Estudante]:
        no = self._buscar_recursivo(self.raiz, student_id)
        return no.estudante if no else None

    def _buscar_recursivo(self, atual: Optional[NoAVL], chave: int) -> Optional[NoAVL]:
        if atual is None or atual.chave == chave:
            return atual
        if chave < atual.chave:
            return self._buscar_recursivo(atual.esquerda, chave)
        return self._buscar_recursivo(atual.direita, chave)

    # ── Remoção ───────────────────────────
    def remover(self, student_id: int) -> bool:
        raiz_nova, removido = self._remover_recursivo(self.raiz, student_id)
        self.raiz = raiz_nova
        if removido:
            self._total_nos -= 1
        return removido

    def _remover_recursivo(self, no: Optional[NoAVL], chave: int):
        if no is None:
            return no, False
        removido = False
        if chave < no.chave:
            no.esquerda, removido = self._remover_recursivo(no.esquerda, chave)
        elif chave > no.chave:
            no.direita, removido = self._remover_recursivo(no.direita, chave)
        else:
            removido = True
            if no.esquerda is None:
                return no.direita, removido
            elif no.direita is None:
                return no.esquerda, removido
            # Substitui pelo sucessor (menor da subárvore direita)
            sucessor = self._minimo(no.direita)
            no.estudante = sucessor.estudante
            no.direita, _ = self._remover_recursivo(no.direita, sucessor.chave)

        return self._balancear(no), removido

    # ── Mínimo / Máximo ───────────────────
    def _minimo(self, no: NoAVL) -> NoAVL:
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
        return self._altura_no(self.raiz)

    # ── Traversals ────────────────────────
    def em_ordem(self) -> List[Estudante]:
        resultado: List[Estudante] = []
        self._em_ordem_recursivo(self.raiz, resultado)
        return resultado

    def _em_ordem_recursivo(self, no: Optional[NoAVL], lista: List[Estudante]) -> None:
        if no:
            self._em_ordem_recursivo(no.esquerda, lista)
            lista.append(no.estudante)
            self._em_ordem_recursivo(no.direita, lista)

    def pre_ordem(self) -> List[Estudante]:
        resultado: List[Estudante] = []
        self._pre_ordem_recursivo(self.raiz, resultado)
        return resultado

    def _pre_ordem_recursivo(self, no: Optional[NoAVL], lista: List[Estudante]) -> None:
        if no:
            lista.append(no.estudante)
            self._pre_ordem_recursivo(no.esquerda, lista)
            self._pre_ordem_recursivo(no.direita, lista)

    def pos_ordem(self) -> List[Estudante]:
        resultado: List[Estudante] = []
        self._pos_ordem_recursivo(self.raiz, resultado)
        return resultado

    def _pos_ordem_recursivo(self, no: Optional[NoAVL], lista: List[Estudante]) -> None:
        if no:
            self._pos_ordem_recursivo(no.esquerda, lista)
            self._pos_ordem_recursivo(no.direita, lista)
            lista.append(no.estudante)

    # ── Busca por faixa de IDs ────────────
    def buscar_por_faixa(self, id_min: int, id_max: int) -> List[Estudante]:
        resultado: List[Estudante] = []
        self._busca_faixa_recursiva(self.raiz, id_min, id_max, resultado)
        return resultado

    def _busca_faixa_recursiva(
        self, no: Optional[NoAVL], id_min: int, id_max: int, lista: List[Estudante]
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
        self, no: Optional[NoAVL], nota_min: float, nota_max: float, lista: List[Estudante]
    ) -> None:
        if no is None:
            return
        self._busca_nota_recursiva(no.esquerda, nota_min, nota_max, lista)
        if nota_min <= no.estudante.final_grade <= nota_max:
            lista.append(no.estudante)
        self._busca_nota_recursiva(no.direita, nota_min, nota_max, lista)

    # ── Busca por campo genérico ───────────
    def _busca_por_campo(self, no: Optional[NoAVL], campo: str, valor) -> List[Estudante]:
        if no is None:
            return []
        resultado = []
        resultado += self._busca_por_campo(no.esquerda, campo, valor)
        if getattr(no.estudante, campo) == valor:
            resultado.append(no.estudante)
        resultado += self._busca_por_campo(no.direita, campo, valor)
        return resultado

    def buscar_aprovados(self) -> List[Estudante]:
        return self._busca_por_campo(self.raiz, "pass_fail", "Pass")

    def buscar_reprovados(self) -> List[Estudante]:
        return self._busca_por_campo(self.raiz, "pass_fail", "Fail")

    def buscar_por_idade(self, idade: int) -> List[Estudante]:
        return self._busca_por_campo(self.raiz, "age", idade)

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
            f"ArvoreAVL("
            f"total={self._total_nos}, "
            f"altura={self.altura()}, "
            f"raiz_id={self.raiz.chave if self.raiz else None})"
        )
