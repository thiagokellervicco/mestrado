"""Informações do ambiente de execução (CPU, SO, RAM)."""

import platform
from typing import Any, Dict, Optional

import cpuinfo
import psutil


def coletar_info_sistema() -> Dict[str, Any]:
    """Dados do ambiente para console e relatório (valores serializáveis)."""
    so = platform.uname()
    info_cpu = cpuinfo.get_cpu_info()
    ram = psutil.virtual_memory()
    freqs = psutil.cpu_freq()

    out: Dict[str, Any] = {
        "os": f"{so.system} {so.release}",
        "arquitetura": so.machine,
        "cpu_modelo": info_cpu.get("brand_raw", "—"),
        "nucleos_fisicos": psutil.cpu_count(logical=False),
        "nucleos_logicos": psutil.cpu_count(logical=True),
        "ram_total_gb": round(ram.total / 1e9, 2),
        "ram_disponivel_gb": round(ram.available / 1e9, 2),
        "ram_usada_gb": round(ram.used / 1e9, 2),
        "ram_percent": ram.percent,
    }
    if freqs is not None:
        out["clock_mhz_atual"] = round(freqs.current, 0)
        out["clock_mhz_min"] = round(freqs.min, 0)
        out["clock_mhz_max"] = round(freqs.max, 0)
    else:
        out["clock_mhz_atual"] = None
        out["clock_mhz_min"] = None
        out["clock_mhz_max"] = None
    return out


def imprimir_info_sistema(s: Optional[Dict[str, Any]] = None) -> Dict[str, Any]:
    """Imprime resumo no console e devolve o mesmo dicionário para o relatório."""
    s = s or coletar_info_sistema()
    print("SISTEMA")
    print("=" * 40)
    print(f"OS:           {s['os']}")
    print(f"Arquitetura:  {s['arquitetura']}")

    print("\n" + "=" * 40)
    print("PROCESSADOR")
    print("=" * 40)
    print(f"Modelo:       {s['cpu_modelo']}")
    print(f"Núcleos físicos:  {s['nucleos_fisicos']}")
    print(f"Núcleos lógicos:  {s['nucleos_logicos']}")
    if s.get("clock_mhz_atual") is not None:
        print(f"Clock atual:  {s['clock_mhz_atual']:.0f} MHz")
        print(f"Clock mín:    {s['clock_mhz_min']:.0f} MHz")
        print(f"Clock máx:    {s['clock_mhz_max']:.0f} MHz")
    else:
        print("Frequência CPU: (não disponível neste sistema)")

    print("\n" + "=" * 40)
    print("MEMÓRIA RAM")
    print("=" * 40)
    print(f"Total:        {s['ram_total_gb']:.2f} GB")
    print(f"Disponível:   {s['ram_disponivel_gb']:.2f} GB")
    print(f"Usada:        {s['ram_usada_gb']:.2f} GB")
    print(f"Uso (%):      {s['ram_percent']}%")
    return s
