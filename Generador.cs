using System;

namespace SortingBenchmark
{
    public enum TipoEntrada { Aleatorio, Ascendente, Descendente, Duplicados }

    public static class Generador
    {
        public const int ValorMaximo = 999_999;   // la tarea pide enteros entre 0 y 999 999
        public const int MaxDuplicados = 10;      // "muchos duplicados": valores entre 0 y 10

        // Genera la entrada de un tipo y tamaño. Usa una semilla fija que depende solo de n
        // y del tipo, así que llamar dos veces con los mismos parámetros devuelve EXACTAMENTE
        // el mismo arreglo. Eso garantiza que todos los algoritmos reciben la misma entrada,
        // incluso Quicksort, que se mide en un proceso aparte.
        public static int[] Generar(TipoEntrada tipo, int n)
        {
            switch (tipo)
            {
                case TipoEntrada.Aleatorio:
                    return Aleatorio(n, ValorMaximo, Semilla(n, 0));

                case TipoEntrada.Ascendente:
                {
                    // Los mismos números del arreglo aleatorio, pero ya ordenados.
                    // Array.Sort aquí es parte de la generación, no se mide.
                    int[] a = Aleatorio(n, ValorMaximo, Semilla(n, 0));
                    Array.Sort(a);
                    return a;
                }

                case TipoEntrada.Descendente:
                {
                    int[] a = Aleatorio(n, ValorMaximo, Semilla(n, 0));
                    Array.Sort(a);
                    Array.Reverse(a);
                    return a;
                }

                case TipoEntrada.Duplicados:
                    return Aleatorio(n, MaxDuplicados, Semilla(n, 1));

                default:
                    throw new ArgumentOutOfRangeException(nameof(tipo));
            }
        }

        // Para la prueba especial de Radix: n números con a lo sumo "digitos" dígitos
        // (de 0 hasta 10^digitos - 1). Con 9 dígitos llega a 999 999 999.
        public static int[] ConDigitos(int n, int digitos)
        {
            int max = (int)Math.Pow(10, digitos) - 1;
            return Aleatorio(n, max, Semilla(n, 100 + digitos));
        }

        // Enteros aleatorios uniformes en [0, maxInclusivo].
        private static int[] Aleatorio(int n, int maxInclusivo, int semilla)
        {
            var rnd = new Random(semilla);
            int[] a = new int[n];
            for (int i = 0; i < n; i++)
                a[i] = rnd.Next(0, maxInclusivo + 1);
            return a;
        }

        private static int Semilla(int n, int variante) => unchecked(2026 * 7919 + n * 31 + variante);
    }
}
