using System;
using System.Linq;

namespace SortingBenchmark
{
    // Parte 2: verifica que los 7 algoritmos ordenan bien, comparando contra
    // Array.Sort de la biblioteca estándar de .NET como referencia.
    public static class Validacion
    {
        public static bool Ejecutar()
        {
            Console.WriteLine("=== Validación contra Array.Sort ===");

            // Tamaños pequeños para casos borde (0, 1, 2 elementos) y algunos medianos.
            // 5000 alcanza para Quicksort sobre ordenados sin que se desborde la pila.
            int[] tamanos = { 0, 1, 2, 3, 10, 100, 1000, 5000 };
            int pruebas = 0, fallos = 0;

            foreach (var alg in Algoritmos.Todos)
            {
                int fallosAlg = 0;

                // Los 4 tipos de entrada del benchmark, en varios tamaños.
                foreach (TipoEntrada tipo in Enum.GetValues<TipoEntrada>())
                    foreach (int n in tamanos)
                    {
                        pruebas++;
                        if (!Probar(alg, Generador.Generar(tipo, n), $"{tipo}, n={n}")) fallosAlg++;
                    }

                // Casos extra que suelen romper implementaciones.
                pruebas += 3;
                if (!Probar(alg, Enumerable.Repeat(7, 500).ToArray(), "todos iguales")) fallosAlg++;
                if (!Probar(alg, new[] { 0, 999_999, 0, 999_999, 5 }, "extremos del rango")) fallosAlg++;
                if (!Probar(alg, Generador.ConDigitos(2000, 9), "valores de 9 dígitos")) fallosAlg++;

                fallos += fallosAlg;
                Console.WriteLine($"{alg.Nombre,-10} {(fallosAlg == 0 ? "OK" : $"FALLÓ en {fallosAlg} casos")}");
            }

            Console.WriteLine($"\n{pruebas - fallos}/{pruebas} pruebas correctas.");
            return fallos == 0;
        }

        private static bool Probar(Algoritmo alg, int[] entrada, string descripcion)
        {
            int[] esperado = (int[])entrada.Clone();
            Array.Sort(esperado);                 // referencia de la biblioteca estándar

            int[] obtenido = (int[])entrada.Clone();
            alg.Sort(obtenido);

            // Comparar contra la copia ordenada revisa dos cosas a la vez: que quede en orden
            // y que tenga exactamente los mismos elementos (que no se perdió ni duplicó nada).
            bool ok = esperado.SequenceEqual(obtenido);
            if (!ok)
                Console.WriteLine($"  {alg.Nombre} falló con entrada: {descripcion}");
            return ok;
        }
    }
}
