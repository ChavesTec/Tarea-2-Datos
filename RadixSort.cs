namespace SortingBenchmark
{
    public static class RadixSort
    {
        // Radix sort LSD (dígito menos significativo primero) en base 10.
        // Solo sirve para enteros no negativos, que es lo que pide la tarea (0 a 999 999 999).
        // Hace una pasada de counting sort por cada dígito del número más grande,
        // así que el costo es O(d * (n + b)), con d = cantidad de dígitos y b = 10.
        public static void Sort(int[] a)
        {
            int n = a.Length;
            if (n < 2) return;

            // El máximo define cuántas pasadas hacen falta (cuántos dígitos tiene).
            int max = a[0];
            for (int i = 1; i < n; i++)
                if (a[i] > max) max = a[i];

            int[] output = new int[n];

            // exp = 1, 10, 100, ... selecciona el dígito de unidades, decenas, centenas...
            // Es long porque con valores cercanos a int.MaxValue, exp * 10 se desbordaría en int.
            for (long exp = 1; max / exp > 0; exp *= 10)
                CountingSortByDigit(a, output, exp);
        }

        // Counting sort estable usando solo el dígito que indica exp.
        private static void CountingSortByDigit(int[] a, int[] output, long exp)
        {
            int n = a.Length;
            int[] count = new int[10];

            // 1) Cuenta cuántos números tienen cada dígito (0 a 9).
            for (int i = 0; i < n; i++)
            {
                int digit = (int)(a[i] / exp % 10);
                count[digit]++;
            }

            // 2) Suma acumulada: count[d] pasa a ser la posición final (+1)
            //    del último número con dígito d.
            for (int d = 1; d < 10; d++)
                count[d] += count[d - 1];

            // 3) Recorre de atrás hacia adelante para que sea ESTABLE: si dos números tienen
            //    el mismo dígito, conservan el orden que les dio la pasada anterior.
            //    Sin estabilidad el radix LSD no funcionaría.
            for (int i = n - 1; i >= 0; i--)
            {
                int digit = (int)(a[i] / exp % 10);
                count[digit]--;
                output[count[digit]] = a[i];
            }

            // 4) Copia el resultado de vuelta al arreglo original.
            for (int i = 0; i < n; i++)
                a[i] = output[i];
        }
    }
}
