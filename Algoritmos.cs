using System;

namespace SortingBenchmark
{
    // Lista central de los 7 algoritmos. Validación y benchmark recorren esta lista,
    // así que si se agrega o cambia un algoritmo solo hay que tocar aquí.
    public record Algoritmo(string Nombre, Action<int[]> Sort, bool EsCuadratico);

    public static class Algoritmos
    {
        public static readonly Algoritmo[] Todos =
        {
            new("Selection", SelectionSort.Sort, true),
            new("Bubble",    BubbleSort.Sort,    true),
            new("Insertion", InsertionSort.Sort, true),
            new("Shell",     ShellSort.Sort,     false),
            new("Merge",     MergeSort.Sort,     false),
            new("Quicksort", QuickSort.Sort,     false),
            new("Radix",     RadixSort.Sort,     false),
        };

        public static Algoritmo Buscar(string nombre)
        {
            foreach (var alg in Todos)
                if (alg.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase))
                    return alg;
            throw new ArgumentException($"No existe el algoritmo '{nombre}'");
        }
    }
}
