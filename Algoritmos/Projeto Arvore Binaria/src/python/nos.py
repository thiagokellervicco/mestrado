"""Nós das árvores (ABB e AVL)."""

from dataclasses import dataclass, field
from typing import Optional

from .estudante import Estudante


@dataclass
class No:
    estudante: Estudante
    esquerda: Optional["No"] = field(default=None, repr=False)
    direita: Optional["No"] = field(default=None, repr=False)

    @property
    def chave(self) -> int:
        return self.estudante.student_id


@dataclass
class NoAVL:
    estudante: Estudante
    esquerda: Optional["NoAVL"] = field(default=None, repr=False)
    direita: Optional["NoAVL"] = field(default=None, repr=False)
    altura: int = 1

    @property
    def chave(self) -> int:
        return self.estudante.student_id
