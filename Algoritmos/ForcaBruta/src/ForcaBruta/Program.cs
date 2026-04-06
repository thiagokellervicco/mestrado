using ForcaBruta.Algoritmos;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("=== ForcaBruta ===");
Console.WriteLine("Projeto para algoritmos de força bruta.");
Console.WriteLine();

var texto = "abracadabra";
var padrao = "cad";
var idx = BuscaSubstringForcaBruta.PrimeiraOcorrencia(texto, padrao);
Console.WriteLine($"Exemplo: primeira ocorrência de \"{padrao}\" em \"{texto}\" → índice {idx}");
