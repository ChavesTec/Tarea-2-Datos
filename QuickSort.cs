namespace SortingBenchmark
{
    public static class QuickSort
    {
        // Quicksort recursivo con pivote FIJO en el último elemento (partición de Lomuto).
        // Sin optimizaciones: no hay pivote aleatorio ni mediana de tres, no cambia a
        // insertion sort en subarreglos pequeños y no elimina la recursión de cola.
        // Esto es a propósito: la tarea pide ver cómo se degrada con entradas ordenadas.
        public static void Sort(int[] a)
        {
            SortRec(a, 0, a.Length - 1);
        }

        // Ordena a[low..high] (ambos extremos incluidos).
        private static void SortRec(int[] a, int low, int high)
        {
            if (low < high)
            {
                // Después de Partition, el pivote queda en su posición final p:
                // todo lo de la izquierda es <= pivote y todo lo de la derecha es > pivote.
                int p = Partition(a, low, high);

                // Si el arreglo ya venía ordenado (o al revés), p queda en un extremo,
                // una de estas llamadas recibe n-1 elementos y la otra 0. Así la
                // recursión baja n niveles: tiempo O(n^2) y riesgo de StackOverflow.
                SortRec(a, low, p - 1);
                SortRec(a, p + 1, high);
            }
        }

        private static int Partition(int[] a, int low, int high)
        {
            int pivot = a[high];   // pivote fijo: el último elemento del rango
            int i = low - 1;       // a[low..i] guarda los elementos <= pivote

            for (int j = low; j < high; j++)
            {
                // Con "<=" todos los valores iguales al pivote se van a la izquierda.
                // Por eso con muchos duplicados las particiones quedan desbalanceadas.
                if (a[j] <= pivot)
                {
                    i++;
                    int tmp = a[i];
                    a[i] = a[j];
                    a[j] = tmp;
                }
            }

            // Pone el pivote justo después del último elemento <= pivote.
            int t = a[i + 1];
            a[i + 1] = a[high];
            a[high] = t;
            return i + 1;
        }
    }
}
