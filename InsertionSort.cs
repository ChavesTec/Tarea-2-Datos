namespace SortingBenchmark
{
    public static class InsertionSort
    {
        // Ordena in-place de forma ascendente. Sin optimizaciones
        // (búsqueda lineal de la posición, sin búsqueda binaria).
        public static void Sort(int[] a)
        {
            int n = a.Length;
            for (int i = 1; i < n; i++)
            {
                int key = a[i];
                int j = i - 1;
                while (j >= 0 && a[j] > key)
                {
                    a[j + 1] = a[j];
                    j--;
                }
                a[j + 1] = key;
            }
        }
    }
}
