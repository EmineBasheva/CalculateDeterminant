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
            DateTime dt = DateTime.Now;
            TimeSpan before = dt.TimeOfDay;
            //Console.WriteLine(before);
            Matrix myMatrix = new Matrix("../../../matrix4x4.txt");
            var bla = myMatrix.GetDeterminant();
            //Console.WriteLine(bla);

            Matrix m = new Matrix(10);
            var det = m.GetDeterminant();
            //m.PrintLong();
            DateTime dt2 = DateTime.Now;
            TimeSpan after = dt2.TimeOfDay;
            TimeSpan executingTime = after - before;
            Console.WriteLine("Executing time: {0}", executingTime);
        }
    }
}
