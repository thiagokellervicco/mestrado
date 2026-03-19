namespace ArvoreBalancead;

using System;

class Program
{
    static void Main(string[] args)
    {
        var arvore = new ArvoreBalanceada<char>();
        char[] letras = { 'A', 'Z', 'B', 'Y', 'C', 'X', 'D', 'W', 'E', 'V', 'F' };

        Console.WriteLine("Inserindo sequência alternada...");
        foreach (var letra in letras)
        {
            arvore.Inserir(letra);
        }

        Console.WriteLine("\nÁrvore Balanceada (AVL) - Raiz no topo:");
        arvore.ExibirArvore();
    }
}

public class ArvoreBalanceada<T> where T : IComparable<T>
{
    private Node<T>? head;
    
    // Altura de um nó nulo é 0. O nó folha terá altura 1.
    private int Altura(Node<T>? no) => no?.Height ?? 0;

    private int FatorBalanceamento(Node<T>? node) => 
        node == null ? 0 : Altura(node.Left) - Altura(node.Right);
    
    private Node<T> RotacaoDireita(Node<T> y)
    {
        Node<T> x = y.Left!;
        Node<T>? T2 = x.Right;

        x.Right = y;
        y.Left = T2;

        y.Height = Math.Max(Altura(y.Left), Altura(y.Right)) + 1;
        x.Height = Math.Max(Altura(x.Left), Altura(x.Right)) + 1;

        return x;
    }
    
    private Node<T> RotacaoEsquerda(Node<T> x)
    {
        Node<T> y = x.Right!;
        Node<T>? T2 = y.Left;

        y.Left = x;
        x.Right = T2;

        x.Height = Math.Max(Altura(x.Left), Altura(x.Right)) + 1;
        y.Height = Math.Max(Altura(y.Left), Altura(y.Right)) + 1;

        return y;
    }
    
    public void Inserir(T valor) => head = InserirRecursivo(head, valor);
    
    private Node<T> InserirRecursivo(Node<T>? node, T value)
    {
        if (node == null) 
        {
            var newNode = new Node<T>(value);
            newNode.Height = 1; // Inicializa altura do novo nó
            return newNode;
        }

        int comp = value.CompareTo(node.Value);
        
        switch (comp)
        {
            case < 0:
                node.Left = InserirRecursivo(node.Left, value);
                break;
            case > 0:
                node.Right = InserirRecursivo(node.Right, value);
                break;
            default:
                return node;
        } 

        // 1. Atualiza altura do nó atual
        node.Height = 1 + Math.Max(Altura(node.Left), Altura(node.Right));

        // 2. Obtém o fator de balanceamento
        int balanceamento = FatorBalanceamento(node);

        // 3. Verifica os 4 casos de desbalanceamento
        // Caso Esquerda-Esquerda
        if (balanceamento > 1 && value.CompareTo(node.Left!.Value) < 0)
            return RotacaoDireita(node);

        // Caso Direita-Direita
        if (balanceamento < -1 && value.CompareTo(node.Right!.Value) > 0)
            return RotacaoEsquerda(node);

        // Caso Esquerda-Direita
        if (balanceamento > 1 && value.CompareTo(node.Left!.Value) > 0)
        {
            node.Left = RotacaoEsquerda(node.Left!);
            return RotacaoDireita(node);
        }

        // Caso Direita-Esquerda
        if (balanceamento >= -1 || value.CompareTo(node.Right!.Value) >= 0) return node;
        
        node.Right = RotacaoDireita(node.Right!);
        return RotacaoEsquerda(node);

    }
    
#pragma warning disable CS0693
    public class Node<T>(T value)
#pragma warning restore CS0693
    {
        public T Value { get; } = value;
        public Node<T>? Right { get; set; }
        public Node<T>? Left { get; set; }
        public int Height { get; set; }
    }

    /// <summary>
    /// Exibe a árvore em formato visual: raiz no topo, filhos abaixo com linhas de conexão.
    /// </summary>
    public void ExibirArvore()
    {
        if (head == null)
        {
            Console.WriteLine("(árvore vazia)");
            return;
        }

        var (linhas, _) = ConstruirLinhas(head);
        foreach (var linha in linhas)
            Console.WriteLine(linha);
    }

    private static (List<string> linhas, int largura) ConstruirLinhas(Node<T>? node)
    {
        if (node == null) return ([], 0);

        string valor = node.Value?.ToString() ?? "?";
        var (linhasEsq, largEsq) = ConstruirLinhas(node.Left);
        var (linhasDir, largDir) = ConstruirLinhas(node.Right);

        int larguraValor = valor.Length;
        int espaco = 2;
        int larguraTotal = largEsq + espaco + largDir;
        larguraTotal = Math.Max(larguraTotal, larguraValor);

        string linhaRaiz = Centralizar(valor, larguraTotal, largEsq, largDir);

        var resultado = new List<string> { linhaRaiz };

        if (node.Left != null || node.Right != null)
        {
            int posEsq = largEsq / 2;
            int posDir = largEsq + espaco + largDir / 2;
            string linhaConector = new string(' ', larguraTotal);
            if (node.Left != null)
                linhaConector = Substituir(linhaConector, posEsq, '/');
            if (node.Right != null)
                linhaConector = Substituir(linhaConector, posDir, '\\');
            resultado.Add(linhaConector);

            int maxAltura = Math.Max(linhasEsq.Count, linhasDir.Count);
            for (int i = 0; i < maxAltura; i++)
            {
                string esq = i < linhasEsq.Count ? linhasEsq[i].PadRight(largEsq) : new string(' ', largEsq);
                string dir = i < linhasDir.Count ? linhasDir[i].PadRight(largDir) : new string(' ', largDir);
                resultado.Add(esq + new string(' ', espaco) + dir);
            }
        }

        return (resultado, larguraTotal);
    }

    private static string Centralizar(string texto, int larguraTotal, int largEsq, int largDir)
    {
        int centro = larguraTotal / 2;
        int inicio = Math.Max(0, centro - texto.Length / 2);
        return (new string(' ', inicio) + texto).PadRight(larguraTotal);
    }

    private static string Substituir(string s, int pos, char c)
    {
        if (pos < 0 || pos >= s.Length) return s;
        return s[..pos] + c + s[(pos + 1)..];
    }
}