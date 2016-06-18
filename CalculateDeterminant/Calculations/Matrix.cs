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
        public static bool IsQuiet;
        public int Size { get; set; }
        public double Determinant { get; set; }

        public double[][] Content;
        private Random rand;
        private static List<ChunkDeterminant> uniqueChunks = new List<ChunkDeterminant>();

        #region Constructors
        public Matrix(int size)
        {
            rand = new Random();
            this.Size = size;
            this.Content = new double[Size][];
            this.Content = this.Content
                .Select(row => new double[Size])
                .Select(row => row
                    .Select(item => rand.NextDouble())
                    .ToArray()
                ).ToArray();
        }

        public Matrix(int size, double[][] content)
        {
            this.Size = size;
            this.Content = content;
        }

        /// <summary>
        /// The file Content looks like this:
        /// n
        /// a11 a12 a13 … a1n
        /// a21 a22 a23 … a2n
        /// …
        /// an1 an2 an3 … ann
        /// </summary>
        /// <param name="fileName">Text file name</param>
        public Matrix(string fileName)
        {
            this.Content = new double[Size][];
            string[] lines = File.ReadAllLines(fileName);

            #region set size

            var _size = Convert.ToInt32(lines[0]);
            this.Size = _size;

            #endregion set size

            #region set content

            var strContent = lines.Skip(1);

            this.Content = strContent
                .Select(line => line.Split(' ')
                    .Where(item => !string.IsNullOrEmpty(item))
                    .Select(strItem => Convert.ToDouble(strItem))
                    .ToArray()
                ).ToArray();
            
            #endregion set content
        }
        #endregion Constructors

        public override bool Equals(object obj)
        {
            var other = obj as Matrix;
            if (other == null || other.Size != this.Size) return false;

            for (int i = 0; i < this.Size; i++)
            {
                for (int j = 0; j < this.Size; j++)
                {
                    if (this.Content[i][j] != other.Content[i][j])
                        return false;
                }
            }

            return true;
        }

        #region Print
        public void PrintShort()
        {
            foreach (var line in this.Content)
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
            foreach (var line in this.Content)
            {
                foreach (var item in line)
                {
                    Console.Write("{0,-20}", item);
                }
                Console.WriteLine();
            }
            Console.WriteLine("size is {0}.\n", Size);
        }
        #endregion Print

        #region Calculate determinant

        public double GetDeterminant()
        {
            if (this.Size == 2)
            {
                var result = this.Content[0][0] * this.Content[1][1]
                    - this.Content[0][1] * this.Content[1][0];
                return result;
            }
            var adjugateMatrix = 0d;
            var allAdjugateMatrixes = GetAllAdjugateMatrixes();
            foreach (var _adjugateMatrix in allAdjugateMatrixes)
            {
                adjugateMatrix += _adjugateMatrix.Coeficient * _adjugateMatrix.SubMatrix.GetDeterminant();
            }
            if (this.Size > 8 && !IsQuiet)
                Console.WriteLine("Determinant of submatrix {0}x{1} = {2}", this.Size, this.Size, adjugateMatrix);
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
            var coeficient = this.Content[i][j] * Math.Pow(-1, i + j + 2);
            var newContent = this.Content
                .Where(row => row != this.Content[i])
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
        
        private List<ChunkDeterminant> GetAllSubChuncks()
        {
            var allAdjugateMatrixes = GetAllAdjugateMatrixes();
            var allSubAdjugateMatrixes = allAdjugateMatrixes
                .SelectMany(chunk => chunk.SubMatrix.GetAllAdjugateMatrixes()
                    .Select(subChunk =>
                        new ChunkDeterminant(subChunk.Coeficient * chunk.Coeficient,
                            subChunk.SubMatrix))
                    ).ToList();
            return allSubAdjugateMatrixes;
        }

        public void CalculateDeterminantWithThreads(int numberOfThreads)
        {
            var allAdjugateMatrixes = GetAllSubChuncks().ToArray();
            var sizeOfDoneEvents = Math.Min(numberOfThreads, allAdjugateMatrixes.Length);
            var doneEvents = new List<ManualResetEvent>(); 
            int counter = 0;
            var finished = false;

            // Configure and start threads using ThreadPool.
            Console.WriteLine("launching {0} tasks...", numberOfThreads);
            while (counter < this.Size && !finished)
            {
                for (int i = 0; i < numberOfThreads && !finished; i++)
                {
                    if (i + counter >= allAdjugateMatrixes.Length)
                    {
                        finished = true;
                        break;
                    }
                    var newManualResetEven = new ManualResetEvent(false);
                    doneEvents.Add(newManualResetEven);

                    allAdjugateMatrixes[i + counter].DoneEvent = newManualResetEven;

                    ThreadPool.QueueUserWorkItem(allAdjugateMatrixes[i + counter].ThreadPoolCallback, i);
                    counter++;
                }
                WaitHandle.WaitAll(doneEvents.ToArray());
            }
            if (!IsQuiet)
                Console.WriteLine("All calculations are complete.");

            foreach (var chunk in allAdjugateMatrixes.Where(ch => ch != null))
            {
                this.Determinant += chunk.Product;
            }
        }

        #endregion Calculate determinant
    }
}
