using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchIris
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string basepath = "Berhasil";

            int[] success = { 1, 3, 4, 5, 6, 7, 9, 10, 12, 14, 1, 3, 4, 5, 6, 7, 9, 10, 12, 14,
            1, 3, 4, 5, 6, 7, 9, 10, 12, 14, 1, 3, 4, 5, 6, 7, 9, 10, 12, 14, 1, 3, 4, 5, 6, 7, 9, 10, 12, 14,
            1, 3, 4, 5, 6, 7, 9, 10, 12, 14, 1, 3, 4, 5, 6, 7, 9, 10, 12, 14, 1, 3, 4, 5, 6, 7, 9, 10, 12, 14,
            1, 3, 4, 5, 6, 7, 9, 10, 12, 14, 1, 3, 4, 5, 6, 7, 9, 10, 12, 14};

            string[] firstPath = new string[success.Length];
            string[] secondPath = new string[success.Length];

            byte[][] arr_iris_01 = new byte[success.Length][];
            byte[][] arr_iris_02 = new byte[success.Length][];

            double[] single_runtime_old = new double[100];
            double[] single_runtime_new = new double[100];
            double runtime_total_new=0.0, runtime_total_old=0.0;

            for (int i = 0; i < success.Length; i++)
            {
                firstPath[i] = basepath + "\\percobaan_" + success[i].ToString("D3") + "_l1.iris";
                secondPath[i] = basepath + "\\percobaan_" + success[i].ToString("D3") + "_l2.iris";

                arr_iris_01[i] = System.IO.File.ReadAllBytes(firstPath[i]);
                arr_iris_02[i] = System.IO.File.ReadAllBytes(secondPath[i]);
            }

            for (int k = 0; k < 100; k++)
            {
                // Create new stopwatch
                Stopwatch stopwatch = new Stopwatch();
                // Begin timing
                stopwatch.Start();
                for (int j = 0; j < success.Length; j++)
                {
                    IrisCode irisData_A = IrisTemplate.Import(arr_iris_01[j]);
                    IrisCode irisData_B = IrisTemplate.Import(arr_iris_02[j]);
                    Matching matching = new Matching(irisData_A, irisData_B);
                    float res = matching.PerformMatching();
                    //Console.WriteLine((j + 1) + " : " + res);
                }
                TimeSpan ts = stopwatch.Elapsed;
                // Stop timing
                stopwatch.Stop();
                //Console.WriteLine("OLD APPROACH : ");
                //Console.WriteLine("Running Time : " + ts.TotalMilliseconds);
                single_runtime_old[k] = ts.TotalMilliseconds;
                runtime_total_old += single_runtime_old[k];
                //Console.WriteLine("\n");

                // Create new stopwatch
                Stopwatch stopwatch2 = new Stopwatch();
                // Begin timing
                stopwatch2.Start();
                for (int j = 0; j < success.Length; j++)
                {
                    Matching m = new Matching();
                    IrisCode irisData_01 = IrisTemplate.NewImport(arr_iris_01[j]);
                    IrisCode irisData_02 = IrisTemplate.NewImport(arr_iris_02[j]);
                    float hasil = m.PerformByteMatch(irisData_01, irisData_02, 10);
                    //Console.WriteLine((j + 1) + " : " + hasil);
                }
                TimeSpan ts2 = stopwatch2.Elapsed;
                // Stop timing
                stopwatch2.Stop();
                //Console.WriteLine("NEW APPROACH : ");
                //Console.WriteLine("Running Time : " + ts2.TotalMilliseconds);
                single_runtime_new[k] = ts2.TotalMilliseconds;
                runtime_total_new += single_runtime_new[k];
                //Console.WriteLine("\n");
            }

            for (int k=0; k<100; k++)
            {
                Console.WriteLine(single_runtime_new[k]);
            }
            Console.WriteLine("-----------------");
            for (int k = 0; k < 100; k++)
            {
                Console.WriteLine(single_runtime_old[k]);
            }
            Console.WriteLine();

            Console.WriteLine("Metode Lama : " + runtime_total_old);
            Console.WriteLine("Metode Baru : " + runtime_total_new);

            Console.WriteLine("Press Enter to Exit...");
            Console.ReadLine();
        }
    }
}
