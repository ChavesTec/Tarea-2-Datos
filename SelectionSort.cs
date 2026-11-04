namespace SortingBenchmark
{
    public static class SelectionSort
    {
        // Ordena in-place de forma ascendente. Sin optimizaciones.
        public static void Sort(int[] a)
        {
            int n = a.Length;
            for (int i = 0; i < n - 1; i++)
            {
                int min = i;
                for (int j = i + 1; j < n; j++)
                {
                    if (a[j] < a[min])
                        min = j;
                }
                int tmp = a[i];
                a[i] = a[min];
                a[min] = tmp;
            }
        }
    }
}
