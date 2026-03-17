namespace InverteLista;

abstract class Program
{
    static void Main(string[] args)
    {
        var list = new LinkedList<int>();
        list.Append(1);
        list.Append(2);
        list.Append(3);
        list.Append(4);
        list.Append(6);
        list.Append(7);
        list.Append(9);
        list.Append(10);
        list.Append(23);
        list.Append(45);
        list.Append(60);
        
        Console.WriteLine(string.Join(", ", list.ToArray()));
        list.Reverse();
        Console.WriteLine(string.Join(", ", list.ToArray())); 
    }
}

public class LinkedList<T>
{
    private Node<T>? head;
    private int _size;
    
    public void Reverse()
    {
        Node<T>? prev = null;
        var current = head;

        while (current != null)
        {
            var next = current.Next;
            current.Next = prev;
            prev = current;
            current = next;
        }
        head = prev;
    }

    private class Node<T>(T value)
    {
        public T Value { get; } = value;
        public Node<T>? Next { get; set; }
    }
    
    
    public void Append(T value)
    {
        var newNode = new Node<T>(value);
        if (head == null)
        {
            head = newNode;
        }
        else
        {
            var current = head;
            while (current.Next != null)
                current = current.Next;
            current.Next = newNode;
        }
        _size++;
    }
    
    public T[] ToArray()
    {
        var result = new List<T>();
        var current = head;
        while (current != null)
        {
            result.Add(current.Value);
            current = current.Next;
        }
        return result.ToArray();
    }
}