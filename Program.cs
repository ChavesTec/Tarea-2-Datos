using System;
using System.Diagnostics;
using System.Globalization;

namespace SortingBenchmark
{
    public static class Program
    {
        // Uso (desde la carpeta del proyecto):
        //   dotnet run -c Release -- validar        pruebas contra Array.Sort
        //   dotnet run -c Release -- benchmark      todas las mediciones (tarda bastante)
        //   dotnet run -c Release -- corto          benchmark con tamaños pequeños, para probar que todo corre
        //   dotnet run -c Release -- radix          prueba especial de Radix variando dígitos
        //   dotnet run -c Release -- todo           validar + benchmark + radix
        public static int Main(string[] args)
        {
            // Punto como separador decimal en todo el programa, para que el CSV no se
            // rompa en una compu configurada en español (que usa coma decimal).
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            // Proceso hijo que usa el benchmark para medir Quicksort sin arriesgar el programa principal.
            if (args.Length == 3 && args[0] == "quick-uno")
            {
                Benchmark.QuicksortUno(Enum.Parse<TipoEntrada>(args[1]), int.Parse(args[2]));
                return 0;
            }

#if DEBUG
            Console.WriteLine("OJO: esto está compilado en Debug. Para medir tiempos usen -c Release.\n");
#endif
            // Prioridad alta para que otros procesos interrumpan menos durante las mediciones.
            try { Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High; } catch { }

            string opcion = args.Length > 0 ? args[0].ToLowerInvariant() : Menu();
            const string carpeta = "resultados";

            switch (opcion)
            {
                case "validar":
                    return Validacion.Ejecutar() ? 0 : 1;

                case "benchmark":
                case "corto":
                    if (!Validacion.Ejecutar()) return 1;   // no se mide nada si algún algoritmo está mal
                    Console.WriteLine();
                    Benchmark.Ejecutar(carpeta, corto: opcion == "corto");
                    return 0;

                case "radix":
                    Benchmark.EjecutarRadixDigitos(carpeta);
                    return 0;

                case "todo":
                    if (!Validacion.Ejecutar()) return 1;
                    Console.WriteLine();
                    Benchmark.Ejecutar(carpeta, corto: false);
                    Console.WriteLine();
                    Benchmark.EjecutarRadixDigitos(carpeta);
                    return 0;

                default:
                    Console.WriteLine($"Opción desconocida: {opcion}");
                    return 1;
            }
        }

        private static string Menu()
        {
            Console.WriteLine("1) validar   2) benchmark   3) corto   4) radix   5) todo");
            Console.Write("Opción: ");
            return Console.ReadLine()?.Trim() switch
            {
                "1" => "validar",
                "2" => "benchmark",
                "3" => "corto",
                "4" => "radix",
                "5" => "todo",
                var otra => otra ?? "",
            };
        }
    }
}
