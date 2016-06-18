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
            // Start count time
            DateTime dt = DateTime.Now;
            TimeSpan before = dt.TimeOfDay;


            //Console.WriteLine(before);
            //Matrix myMatrix = new Matrix("../../../matrix10x10.txt");
            //myMatrix.CalculateDeterminantWithThreads(11);
            //Console.WriteLine("\nDeterminant = {0}\n", myMatrix.Determinant);

            Matrix m = new Matrix(15);
            m.CalculateDeterminantWithThreads(15);
            Console.WriteLine("\nDeterminant = {0}\n", m.Determinant);
            //var det = m.GetDeterminant();
            //m.PrintLong();

            // Finish count time
            DateTime dt2 = DateTime.Now;
            TimeSpan after = dt2.TimeOfDay;
            TimeSpan executingTime = after - before;
            Console.WriteLine("Executing time: {0}", executingTime);
        }
    }
}
