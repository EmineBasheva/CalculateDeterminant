using Calculations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainCD
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!args.Any())
            {
                Console.WriteLine("Please use these parameters:");
                Console.WriteLine("   -f <fileName>       -> to specify matrix by file");
                Console.WriteLine("   -n <sizeOfMatrix>   -> to specify size of random generated matrix");
                Console.WriteLine("   -o <outputFileName> -> to specify output file name");
                Console.WriteLine("   -t <numberOfTasks>  -> to specify number of tasks");
                Console.WriteLine("   -q                  -> program to be executed without comments");
                return;
            }

            // Start count time
            DateTime dt = DateTime.Now;
            TimeSpan before = dt.TimeOfDay;

            Matrix myMatrix = null;

            string fileName = "";
            int sizeOfMatrix = 0;
            string outputFileName = "";
            int tasks = 1;
            bool quiet = false;

            #region set arguments
            var f = args
                .Select((arg, i) => new { arg, i })
                .Where(r => r.arg.Equals("-f"))
                .Select(r => r.i + 1)
                .FirstOrDefault();
            if (f != 0)
                fileName = args[f];

            var n = args
                .Select((arg, i ) => new {arg, i })
                .Where(r => r.arg.Equals("-n"))
                .Select(r => r.i + 1)
                .FirstOrDefault();
            if (n != 0 && fileName.Equals(""))
                sizeOfMatrix = Convert.ToInt32(args[n]);

            var o = args
                .Select((arg, i) => new { arg, i })
                .Where(r => r.arg.Equals("-o"))
                .Select(r => r.i + 1)
                .FirstOrDefault();
            if (o != 0)
                outputFileName = args[o];

            var t = args
                .Select((arg, i) => new { arg, i })
                .Where(r => r.arg.Equals("-t"))
                .Select(r => r.i + 1)
                .FirstOrDefault();
            if (t != 0)
                tasks = Convert.ToInt32(args[t]);

            quiet = args.Any(arg => arg.Equals("-q"));

            if (!fileName.Equals(""))
                Console.WriteLine("\nFile name: {0}", fileName);
            if (sizeOfMatrix > 0)
                Console.WriteLine("\nSize of matrix: {0}", sizeOfMatrix);
            if (!outputFileName.Equals(""))
                Console.WriteLine("\nOutput file name: {0}", outputFileName);
            Console.WriteLine("\nTasks: {0}", tasks);
            #endregion set arguments

            Matrix.IsQuiet = quiet;

            if (!fileName.Equals(""))
                myMatrix = new Matrix(fileName);

            if (sizeOfMatrix > 2)
                myMatrix = new Matrix(sizeOfMatrix);

            if (myMatrix != null)
            {
                if (!quiet) Console.WriteLine("\nStart...\n");
                myMatrix.CalculateDeterminantWithThreads(tasks);
                Console.WriteLine("Determinant = {0}", myMatrix.Determinant);
            }

            // Finish count time
            DateTime dt2 = DateTime.Now;
            TimeSpan after = dt2.TimeOfDay;
            TimeSpan executingTime = after - before;

            if (!outputFileName.Equals(""))
                System.IO.File.WriteAllLines(outputFileName, new string[] { executingTime.ToString() });
            Console.WriteLine("Executing time: {0}", executingTime);
        }
    }
}
