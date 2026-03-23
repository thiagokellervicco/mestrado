using ProjetoArvoreBinaria.Models;

namespace ProjetoArvoreBinaria.Trees;

public sealed class No
{
    public Estudante Estudante { get; set; } = null!;
    public No? Esquerda { get; set; }
    public No? Direita { get; set; }
    public int Chave => Estudante.StudentId;
}
