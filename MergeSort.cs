namespace SortingBenchmark
{
    public static class MergeSort
    {
        // Merge sort recursivo (top-down), ordena in-place de forma ascendente.
        // Sin optimizaciones: no usa insertion sort para subarreglos pequeños
        // ni revisa si las dos mitades ya están en orden antes de mezclar.
        public static void Sort(int[] a)
        {
            if (a.Length < 2) return;

            // Arreglo auxiliar del mismo tamaño. Se crea una sola vez y se reutiliza
            // en todas las mezclas; por eso Merge usa O(n) de memoria extra.
            int[] aux = new int[a.Length];
            SortRec(a, aux, 0, a.Length - 1);
        }

        // Ordena el rango a[lo..hi] (ambos extremos incluidos).
        private static void SortRec(int[] a, int[] aux, int lo, int hi)
        {
            // Caso base: un rango de 0 o 1 elementos ya está ordenado.
            if (lo >= hi) return;

            // lo + (hi - lo) / 2 en vez de (lo + hi) / 2 para evitar overflow con n grandes.
            int mid = lo + (hi - lo) / 2;

            // Divide: siempre parte en dos mitades iguales, sin importar los datos.
            // Por eso la profundidad es log2(n) en cualquier tipo de entrada.
            SortRec(a, aux, lo, mid);
            SortRec(a, aux, mid + 1, hi);

            // Combina: mezcla las dos mitades ordenadas en O(hi - lo + 1).
            Merge(a, aux, lo, mid, hi);
        }

        // Mezcla a[lo..mid] y a[mid+1..hi], que ya vienen ordenados.
        private static void Merge(int[] a, int[] aux, int lo, int mid, int hi)
        {
            // Copia el rango al auxiliar para poder sobrescribir a[] mientras se mezcla.
            for (int k = lo; k <= hi; k++)
                aux[k] = a[k];

            int i = lo;       // siguiente elemento de la mitad izquierda
            int j = mid + 1;  // siguiente elemento de la mitad derecha

            for (int k = lo; k <= hi; k++)
            {
                if (i > mid)
                    a[k] = aux[j++];          // se acabó la izquierda: toma de la derecha
                else if (j > hi)
                    a[k] = aux[i++];          // se acabó la derecha: toma de la izquierda
                else if (aux[j] < aux[i])
                    a[k] = aux[j++];          // el de la derecha es menor
                else
                    a[k] = aux[i++];          // empate o izquierda menor: toma la izquierda (estable)
            }
        }
    }
}
