namespace SortingBenchmark
{
    public static class ShellSort
    {
        // Ordena in-place de forma ascendente.
        // Secuencia de gaps de Shell: n/2, n/4, ..., 1.
        public static void Sort(int[] a)
        {
            int n = a.Length;
            for (int gap = n / 2; gap > 0; gap /= 2)
            {
                for (int i = gap; i < n; i++)
                {
                    int temp = a[i];
                    int j = i;
                    while (j >= gap && a[j - gap] > temp)
                    {
                        a[j] = a[j - gap];
                        j -= gap;
                    }
                    a[j] = temp;
                }
            }
        }
    }
}
