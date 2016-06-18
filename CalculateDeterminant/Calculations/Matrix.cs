using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Calculations
{
    public class Matrix
    {
        public int Size { get; set; }
        public double Determinant { get; set; }

        private double[][] content;
        private Random rand;

        public Matrix(int size)
        {
            rand = new Random();
            this.Size = size;
            this.content = new double[Size][];
            this.content = this.content
                .Select(row => new double[Size])
                .Select(row => row
                    .Select(item => rand.NextDouble())
                    .ToArray()
                ).ToArray();
        }

        public Matrix(int size, double[][] content)
        {
            this.Size = size;
            this.content = content;
        }

        /// <summary>
        /// The file content looks like this:
        /// n
        /// a11 a12 a13 … a1n
        /// a21 a22 a23 … a2n
        /// …
        /// an1 an2 an3 … ann
        /// </summary>
        /// <param name="fileName">Text file name</param>
        public Matrix(string fileName)
        {
            this.content = new double[Size][];
            string[] lines = File.ReadAllLines(fileName);

            #region set size

            var _size = Convert.ToInt32(lines[0]);
            this.Size = _size;

            #endregion set size

            #region set content

            var strContent = lines.Skip(1);

            this.content = strContent
                .Select(line => line.Split(' ')
                    .Where(item => !string.IsNullOrEmpty(item))
                    .Select(strItem => Convert.ToDouble(strItem))
                    .ToArray()
                ).ToArray();
            
            #endregion set content
        }
        
        public void PrintShort()
        {
            foreach (var line in this.content)
            {
                foreach (var item in line)
                {
                    Console.Write("{0,-5}", item);
                }
                Console.WriteLine();
            }
            Console.WriteLine("size is {0}.\n", Size);
        }

        public void PrintLong()
        {
            foreach (var line in this.content)
            {
                foreach (var item in line)
                {
                    Console.Write("{0,-20}", item);
                }
                Console.WriteLine();
            }
            Console.WriteLine("size is {0}.\n", Size);
        }

        public double GetDeterminant()
        {
            if (this.Size == 2)
            {
                var result = this.content[0][0] * this.content[1][1]
                    - this.content[0][1] * this.content[1][0];
                return result;
            }
            var adjugateMatrix = 0d;
            var allAdjugateMatrixes = GetAllAdjugateMatrixes();
            foreach (var _adjugateMatrix in allAdjugateMatrixes)
            {
                adjugateMatrix += _adjugateMatrix.Coeficient * _adjugateMatrix.SubMatrix.GetDeterminant();
            }
            //Console.WriteLine("Determinant of submatrix {0}x{1} = {2}", this.Size, this.Size, adjugateMatrix);
            return adjugateMatrix;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i">i coord</param>
        /// <param name="j">j coord</param>
        /// <returns>The coecifient before adjugate matrix and adjugate matrix</returns>
        public ChunkDeterminant GetAdjugateMatrix(int i, int j)
        {
            var coeficient = this.content[i][j] * Math.Pow(-1, i + j + 2);
            var newContent = this.content
                .Where(row => row != this.content[i])
                .Select(row => row
                    .Where((item, index) => index != j)
                    .ToArray()
                ).ToArray();

            var matrix = new Matrix(newContent.Length, newContent);

            return new ChunkDeterminant(coeficient, matrix);
        }

        /// <summary>
        /// Gets all adjugate matrix from first row
        /// </summary>
        /// <returns></returns>
        public List<ChunkDeterminant> GetAllAdjugateMatrixes()
        {
            var result = new List<ChunkDeterminant>();
            for (int j = 0; j < this.Size; j++)
            {
                var j_adjugateMatrix = GetAdjugateMatrix(0, j);
                if (j_adjugateMatrix.Coeficient != 0)
                    result.Add(j_adjugateMatrix);
            }
            return result;
        }

        /// <summary>
        /// Wrapper method for use with thread pool
        /// </summary>
        /// <param name="threadContext"></param>
        //public void ThreadPoolCallback(Object threadContext)
        //{
        //    int threadIndex = (int)threadContext;
        //    Console.WriteLine("thread {0} started...", threadIndex);
        //    //_fibOfN = Calculate(_n);
        //    Console.WriteLine("thread {0} result calculated...", threadIndex);
        //    _doneEvent.Set();
        //}

        public void CalculateDeterminantWithThreads(int numberOfThreads)
        {
            var allAdjugateMatrixes = GetAllAdjugateMatrixes();
            var sizeOfDoneEvents = Math.Min(numberOfThreads, allAdjugateMatrixes.Count);
            var doneEvents = new ManualResetEvent[sizeOfDoneEvents];
            var chunksOfDeterminant = new ChunkDeterminant [this.Size];
            int counter = 0;
            var finished = false;

            // Configure and start threads using ThreadPool.
            Console.WriteLine("launching {0} tasks...", numberOfThreads);
            while (counter < this.Size && !finished)
            {
                for (int i = 0; i < numberOfThreads && !finished; i++)
                {
                    if (!allAdjugateMatrixes.Any())
                    {
                        finished = true;
                        break;
                    }
                    doneEvents[i] = new ManualResetEvent(false);
                    var firstAllAdjugateMatrix = allAdjugateMatrixes.First();
                    allAdjugateMatrixes = allAdjugateMatrixes.Skip(1).ToList();

                    firstAllAdjugateMatrix.DoneEvent = doneEvents[i];
                    chunksOfDeterminant[counter] = firstAllAdjugateMatrix;

                    ThreadPool.QueueUserWorkItem(firstAllAdjugateMatrix.ThreadPoolCallback, i);
                    counter++;
                }
                WaitHandle.WaitAll(doneEvents);
            }
            Console.WriteLine("All calculations are complete.");

            foreach (var chunk in chunksOfDeterminant.Where(ch => ch != null))
            {
                this.Determinant += chunk.Product;
            }
        }
    }
}
