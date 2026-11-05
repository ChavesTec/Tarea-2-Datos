using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace SortingBenchmark
{
    // Parte 3: mide los tiempos y escribe los CSV.
    public static class Benchmark
    {
        // Cada caso se corre 1 vez de calentamiento (se descarta por el JIT) + 5 veces que cuentan.
        public const int Calentamiento = 1;
        public const int Repeticiones = 5;

        // Si la mediana de un caso pasa de este tiempo, ya no se sigue duplicando n para ese
        // algoritmo y entrada (el siguiente tardaría ~4 veces más si es cuadrático).
        public const double LimiteMs = 60_000;

        // Pila de 1 MB para Quicksort, que es lo que trae por defecto el hilo principal en
        // Windows. Se fija explícitamente para que el punto de desbordamiento no cambie
        // dependiendo del sistema operativo.
        public const int PilaQuicksort = 1024 * 1024;

        // Tamaños: se duplica n en cada medición para poder calcular t(2n)/t(n).
        public static int[] TamanosCuadraticos(bool corto) => Duplicando(1_000, corto ? 4_000 : 16_000);
        public static int[] TamanosRapidos(bool corto) => Duplicando(1_000, corto ? 64_000 : 1_024_000);

        private static int[] Duplicando(int desde, int hasta)
        {
            var lista = new List<int>();
            for (int n = desde; n <= hasta; n *= 2) lista.Add(n);
            return lista.ToArray();
        }

        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        // ------------------------------------------------------------------
        // Benchmark principal: 7 algoritmos x 4 entradas x todos los tamaños
        // ------------------------------------------------------------------
        public static void Ejecutar(string carpeta, bool corto)
        {
            Directory.CreateDirectory(carpeta);
            string rutaMediciones = Path.Combine(carpeta, "mediciones.csv");
            string rutaMedianas = Path.Combine(carpeta, "medianas.csv");

            using var mediciones = new StreamWriter(rutaMediciones);
            using var medianas = new StreamWriter(rutaMedianas);
            mediciones.WriteLine("algoritmo,entrada,n,repeticion,tiempo_ms,descartada");
            medianas.WriteLine("algoritmo,entrada,n,mediana_ms,razon_t2n_tn,estado");

            var tamanos = TamanosCuadraticos(corto).Union(TamanosRapidos(corto)).OrderBy(n => n).ToArray();

            // Casos (algoritmo, entrada) que ya no se siguen midiendo y la mediana anterior de cada caso.
            var detenidos = new HashSet<(string, TipoEntrada)>();
            var medianaAnterior = new Dictionary<(string, TipoEntrada), double>();

            Console.WriteLine("=== Benchmark ===");
            Console.WriteLine($"{"Algoritmo",-10} {"Entrada",-12} {"n",9} {"mediana (ms)",14} {"t(2n)/t(n)",11}");

            foreach (TipoEntrada tipo in Enum.GetValues<TipoEntrada>())
            {
                foreach (int n in tamanos)
                {
                    // Se genera UNA vez por tamaño y todos los algoritmos usan este mismo arreglo.
                    int[] original = Generador.Generar(tipo, n);

                    foreach (var alg in Algoritmos.Todos)
                    {
                        var clave = (alg.Nombre, tipo);
                        if (detenidos.Contains(clave)) continue;

                        int[] permitidos = alg.EsCuadratico ? TamanosCuadraticos(corto) : TamanosRapidos(corto);
                        if (!permitidos.Contains(n)) continue;

                        double[] tiempos;
                        if (alg.Nombre == "Quicksort")
                        {
                            // Quicksort va en un proceso aparte: en .NET un StackOverflow no se
                            // puede atrapar con try/catch y cerraría todo el benchmark.
                            var (t, error) = MedirQuicksortAparte(tipo, n);
                            if (t == null)
                            {
                                medianas.WriteLine($"{alg.Nombre},{tipo},{n},,,desbordamiento_pila");
                                medianas.Flush();
                                Console.WriteLine($"{alg.Nombre,-10} {tipo,-12} {n,9} {"--",14}   {error}");
                                detenidos.Add(clave);
                                continue;
                            }
                            tiempos = t;
                        }
                        else
                        {
                            tiempos = Medir(alg.Sort, original);
                        }

                        // CSV con TODAS las mediciones individuales, incluida la descartada.
                        for (int r = 0; r < tiempos.Length; r++)
                            mediciones.WriteLine(string.Join(",", alg.Nombre, tipo, n, r + 1,
                                tiempos[r].ToString("F4", Inv), r < Calentamiento ? "si" : "no"));
                        mediciones.Flush();

                        double mediana = Mediana(tiempos.Skip(Calentamiento).ToArray());
                        string razon = "";
                        if (medianaAnterior.TryGetValue(clave, out double previa) && previa > 0)
                            razon = (mediana / previa).ToString("F3", Inv);
                        medianaAnterior[clave] = mediana;

                        string estado = "ok";
                        if (mediana > LimiteMs)
                        {
                            estado = "limite_tiempo";   // se midió, pero ya no se sigue con n más grandes
                            detenidos.Add(clave);
                        }

                        medianas.WriteLine($"{alg.Nombre},{tipo},{n},{mediana.ToString("F4", Inv)},{razon},{estado}");
                        medianas.Flush();
                        Console.WriteLine($"{alg.Nombre,-10} {tipo,-12} {n,9} {mediana,14:F3} {razon,11}");
                    }
                }
            }

            Console.WriteLine($"\nListo. Archivos:\n  {Path.GetFullPath(rutaMediciones)}\n  {Path.GetFullPath(rutaMedianas)}");
        }

        // ------------------------------------------------------------------
        // Prueba especial de Radix: n fijo, variando la cantidad de dígitos
        // ------------------------------------------------------------------
        public static void EjecutarRadixDigitos(string carpeta, int n = 100_000)
        {
            Directory.CreateDirectory(carpeta);
            string ruta = Path.Combine(carpeta, "radix_digitos.csv");
            using var csv = new StreamWriter(ruta);
            csv.WriteLine("digitos,n,repeticion,tiempo_ms,descartada,mediana_ms");

            Console.WriteLine($"=== Radix con n = {n} fijo, variando dígitos ===");
            Console.WriteLine($"{"dígitos",8} {"mediana (ms)",14} {"ms por dígito",14}");

            // De 1 a 9 dígitos: incluye los 3, 6 y 9 que sugiere el enunciado,
            // con más puntos para que se vea mejor si la relación es lineal en d.
            for (int d = 1; d <= 9; d++)
            {
                int[] original = Generador.ConDigitos(n, d);
                double[] tiempos = Medir(RadixSort.Sort, original);
                double mediana = Mediana(tiempos.Skip(Calentamiento).ToArray());

                for (int r = 0; r < tiempos.Length; r++)
                    csv.WriteLine(string.Join(",", d, n, r + 1, tiempos[r].ToString("F4", Inv),
                        r < Calentamiento ? "si" : "no", mediana.ToString("F4", Inv)));

                Console.WriteLine($"{d,8} {mediana,14:F3} {mediana / d,14:F3}");
            }

            Console.WriteLine($"\nListo. Archivo: {Path.GetFullPath(ruta)}");
        }

        // ------------------------------------------------------------------
        // Medición
        // ------------------------------------------------------------------

        // Corre el algoritmo Calentamiento + Repeticiones veces sobre copias de la misma entrada.
        // Devuelve todos los tiempos en milisegundos (el primero es el de calentamiento).
        public static double[] Medir(Action<int[]> sort, int[] original)
        {
            var tiempos = new double[Calentamiento + Repeticiones];
            int[] copia = Array.Empty<int>();

            for (int r = 0; r < tiempos.Length; r++)
            {
                // La copia se hace FUERA del tiempo medido: solo se cronometra el ordenamiento.
                copia = (int[])original.Clone();

                // Limpia la basura antes de medir para que una recolección del GC
                // no caiga en medio del ordenamiento y meta ruido.
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                long inicio = Stopwatch.GetTimestamp();
                sort(copia);
                long fin = Stopwatch.GetTimestamp();

                tiempos[r] = (fin - inicio) * 1000.0 / Stopwatch.Frequency;
            }

            // Revisión rápida (fuera del tiempo) de que el último resultado sí quedó ordenado.
            for (int i = 1; i < copia.Length; i++)
                if (copia[i - 1] > copia[i])
                    throw new InvalidOperationException("El algoritmo no dejó el arreglo ordenado.");

            return tiempos;
        }

        private static double Mediana(double[] valores)
        {
            var orden = valores.OrderBy(v => v).ToArray();
            int m = orden.Length / 2;
            return orden.Length % 2 == 1 ? orden[m] : (orden[m - 1] + orden[m]) / 2;
        }

        // Lanza este mismo programa con "quick-uno <tipo> <n>" y lee los tiempos que imprime.
        // Si el proceso hijo muere (StackOverflow), devuelve null y el motivo.
        private static (double[]? tiempos, string error) MedirQuicksortAparte(TipoEntrada tipo, int n)
        {
            var psi = new ProcessStartInfo
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };

            // Si se corrió con "dotnet Programa.dll", hay que pasarle el .dll a dotnet.
            string exe = Environment.ProcessPath!;
            psi.FileName = exe;
            if (Path.GetFileNameWithoutExtension(exe).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
                psi.ArgumentList.Add(typeof(Benchmark).Assembly.Location);

            psi.ArgumentList.Add("quick-uno");
            psi.ArgumentList.Add(tipo.ToString());
            psi.ArgumentList.Add(n.ToString(Inv));

            using var p = Process.Start(psi)!;
            // Se leen las dos salidas en paralelo para que ninguna llene su buffer y se trabe.
            var salida = p.StandardOutput.ReadToEndAsync();
            var errores = p.StandardError.ReadToEndAsync();
            p.WaitForExit();

            if (p.ExitCode != 0)
            {
                bool esPila = errores.Result.Contains("Stack overflow", StringComparison.OrdinalIgnoreCase)
                              || p.ExitCode == -1073741571;   // código de StackOverflow en Windows
                return (null, esPila ? "desbordamiento de pila" : $"el proceso terminó con código {p.ExitCode}");
            }

            var tiempos = salida.Result
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Where(l => l.StartsWith("T "))
                .Select(l => double.Parse(l.Substring(2), Inv))
                .ToArray();
            return (tiempos, "");
        }

        // Lo que corre el proceso hijo: mide Quicksort en un hilo con pila de 1 MB
        // e imprime un tiempo por línea.
        public static void QuicksortUno(TipoEntrada tipo, int n)
        {
            int[] original = Generador.Generar(tipo, n);
            double[] tiempos = Array.Empty<double>();

            var hilo = new Thread(() => tiempos = Medir(QuickSort.Sort, original), PilaQuicksort);
            hilo.Start();
            hilo.Join();

            foreach (double t in tiempos)
                Console.WriteLine("T " + t.ToString("R", Inv));
        }
    }
}
