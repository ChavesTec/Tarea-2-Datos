namespace SortingBenchmark
{
    public static class BubbleSort
    {
        // Ordena in-place de forma ascendente.
        // Sin optimizaciones: no hay bandera de "ya ordenado" (no termina antes).
        public static void Sort(int[] a)
        {
            int n = a.Length;
            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - 1 - i; j++)
                {
                    if (a[j] > a[j + 1])
                    {
                        int tmp = a[j];
                        a[j] = a[j + 1];
                        a[j + 1] = tmp;
                    }
                }
            }
        }
    }
}
