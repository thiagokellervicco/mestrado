using ProjetoArvoreBinaria.Models;

namespace ProjetoArvoreBinaria.Trees;

public sealed class NoAvl
{
    public Estudante Estudante { get; set; } = null!;
    public NoAvl? Esquerda { get; set; }
    public NoAvl? Direita { get; set; }
    public int Altura { get; set; } = 1;
    public int Chave => Estudante.StudentId;
}
