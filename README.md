# Tarea 2 de Datos: benchmark de algoritmos de ordenamiento

Proyecto de consola en C# (.NET 8 o más nuevo) con los 7 algoritmos, la validación contra `Array.Sort` y el programa de mediciones.

## Cómo correrlo

Desde la carpeta del proyecto, siempre en Release (en Debug los tiempos no sirven):

```
dotnet run -c Release -- validar     # prueba los 7 algoritmos contra Array.Sort
dotnet run -c Release -- corto       # benchmark rápido con n pequeños, para revisar que todo corre
dotnet run -c Release -- benchmark   # mediciones completas
dotnet run -c Release -- radix       # prueba especial de Radix (n = 100 000, de 1 a 9 dígitos)
dotnet run -c Release -- todo        # validar + benchmark + radix
```

Los resultados quedan en `resultados/`:

- `mediciones.csv`: cada medición individual (la repetición 1 es el calentamiento y viene marcada como descartada).
- `medianas.csv`: la mediana de las 5 repeticiones válidas, la razón t(2n)/t(n) y el estado (`ok`, `desbordamiento_pila` o `limite_tiempo`).
- `radix_digitos.csv`: la prueba especial de Radix.

## Metodología

- Entradas: aleatoria (0 a 999 999), ascendente, descendente y con duplicados (0 a 10). Se generan con semilla fija, así que todos los algoritmos reciben exactamente el mismo arreglo para cada tamaño.
- Tamaños: Selection, Bubble e Insertion de 1 000 a 16 000; Shell, Merge, Quicksort y Radix de 1 000 a 1 024 000, duplicando n.
- Se cronometra solo el ordenamiento con `Stopwatch`. La copia del arreglo y la generación quedan fuera del tiempo.
- 1 corrida de calentamiento que se descarta (JIT) + 5 corridas válidas; se reporta la mediana.
- Quicksort (pivote fijo en el último elemento) corre en un proceso aparte con pila de 1 MB, porque en .NET un StackOverflow no se puede atrapar y cerraría todo el programa. Cuando se desborda, queda anotado en `medianas.csv` y no se prueban n más grandes para esa entrada.
- La compilación por niveles del JIT está desactivada en el `.csproj` (`TieredCompilation = false`) para que el código no cambie de velocidad a mitad de las mediciones.

## Algoritmos

| Archivo | Algoritmo |
| --- | --- |
| `SelectionSort.cs` | Selection |
| `BubbleSort.cs` | Bubble, sin bandera de corte temprano |
| `InsertionSort.cs` | Insertion |
| `ShellSort.cs` | Shell con gaps n/2, n/4, ..., 1 |
| `MergeSort.cs` | Merge top-down con un arreglo auxiliar |
| `QuickSort.cs` | Quicksort, pivote último, partición de Lomuto |
| `RadixSort.cs` | Radix LSD en base 10 |
