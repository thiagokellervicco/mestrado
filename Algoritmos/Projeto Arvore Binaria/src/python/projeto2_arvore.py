"""
Ponto de entrada alternativo.

  python -m src.python.projeto2_arvore     (a partir da raiz do projeto)
  python3 src/python/projeto2_arvore.py    (também funciona)
"""

from __future__ import annotations

import sys
from pathlib import Path


def _main() -> None:
    if __package__ in (None, ""):
        # Executado como ficheiro: imports relativos não funcionam
        raiz = Path(__file__).resolve().parents[2]
        sys.path.insert(0, str(raiz))
        from src.python.main import main as run
    else:
        from .main import main as run
    run()


if __name__ == "__main__":
    _main()
