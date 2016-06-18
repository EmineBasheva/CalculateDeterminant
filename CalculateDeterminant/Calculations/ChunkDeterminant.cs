using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Calculations
{
    public class ChunkDeterminant
    {
        public double Coeficient { get; set; }
        public Matrix SubMatrix { get; set; }
        public double Product { get; set; }
        public ManualResetEvent DoneEvent;

        public ChunkDeterminant(double coeficient, Matrix subMatrix)
        {
            this.Coeficient = coeficient;
            this.SubMatrix = subMatrix;
        }

        public void ThreadPoolCallback(Object threadContext)
        {
            int threadIndex = (int)threadContext;
            if (!Matrix.IsQuiet)
                Console.WriteLine("thread {0} started...", threadIndex);
            this.Product = this.Coeficient * this.SubMatrix.GetDeterminant();
            if (!Matrix.IsQuiet)
                Console.WriteLine("thread {0} result calculated...", threadIndex);
            DoneEvent.Set();
        }
    }
}
