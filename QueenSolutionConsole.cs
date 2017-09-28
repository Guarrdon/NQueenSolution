using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Stopwatch = System.Diagnostics.Stopwatch;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace QueenSolutionConsole
{
    public partial class Program
    {
        private const int _BoardSize = 18;
        public static void Main(string[] args)
        {
            var sw = Stopwatch.StartNew();

            long count = 0;
            long spin = 0;

            ParallelOptions po = new ParallelOptions()
            {
                MaxDegreeOfParallelism = 8,
            };

            var result = Parallel.For(0, _BoardSize, po,
                (index) =>
                {
                    var solver = new Solver(_BoardSize);
                    solver.BuildNew(0, index, index + 1);
                    Interlocked.Add(ref count, solver.Count);
                    Interlocked.Add(ref spin, solver.Spin);
                });

            sw.Stop();
            ReportSummary(sw, _BoardSize, count, spin);
        }

        public class Solver
        {
            public int BoardSize;
            public long Spin;
            public long Count;
            public Stack<int[]> Solutions;
            private int[] _Current;
            private long _YLine;
            private long _DownDiag;
            private long _UpDiag;
            public Solver(int boardSize)
            {
                BoardSize = boardSize;
                _Current = new int[boardSize];
                Solutions = new Stack<int[]>();
            }
            //recursive method to iterate all potential positions
            //maintain state as we go so when we come back to a previous loop and iterate, we don't have to rebuild the entire state
            //only iterate through half as we use Y-flip to find other solutions
            public void BuildNew(int position, int start, int end)
            {
                long YLine = 0;
                long UpDiag = 0;
                long DownDiag = 0;

                long tempY = 0;
                long tempUp = 0;
                long tempDown = 0;

                for (int x = start; x < end; x++)
                {
                    Spin++;

                    tempY = 2 << x;
                    tempUp = 2 << (BoardSize + x - position);
                    tempDown = 2 << (position + x);

                    if (((tempY & _YLine) == 0) &&
                        ((tempDown & _DownDiag) == 0) &&
                        ((tempUp & _UpDiag) == 0))
                    {
                        _Current[position] = x;

                        if (position == BoardSize - 1)
                        {
                            Count++;
                            int[] t1 = new int[BoardSize];
                            Array.Copy(_Current, t1, BoardSize);
                            Solutions.Push(t1);

                        }
                        if (position < BoardSize - 1)
                        {
                            YLine = _YLine;
                            UpDiag = _UpDiag;
                            DownDiag = _DownDiag;

                            _YLine = tempY | _YLine;
                            _DownDiag = tempDown | _DownDiag;
                            _UpDiag = tempUp | _UpDiag;

                            BuildNew(position + 1, 0, BoardSize);

                            _YLine = YLine;
                            _UpDiag = UpDiag;
                            _DownDiag = DownDiag;
                        }
                    }

                }
            }


            //not used yet
            private int[] XFlip(int[] placed)
            {
                int[] test = new int[BoardSize];
                int temp;
                for (int x = 0; x < BoardSize / 2; x++)
                {
                    temp = placed[BoardSize - x - 1];
                    test[BoardSize - x - 1] = placed[x];
                    test[x] = temp;
                }

                return test;
            }

            //not used
            private int[] YFlip(int[] placed)
            {
                int[] test = new int[BoardSize];
                for (int x = 0; x < BoardSize; x++)
                    test[x] = BoardSize - 1 - placed[x];

                return test;
            }

            //not used yet
            private int[] Rotate90(int[] placed)
            {
                int[] test = new int[BoardSize];
                //id mid point
                var mid = ((double)BoardSize - 1.0) / 2.0;
                double xStart;
                double yStart;
                double xEnd;
                double yEnd;

                for (int x = 0; x < BoardSize; x++)
                {
                    if (placed[x] >= 0)
                    {
                        if (x < mid)
                            xStart = Math.Floor((double)x - mid);
                        else
                            xStart = Math.Ceiling((double)x - mid);
                        if (placed[x] < mid)
                            yStart = Math.Floor((double)placed[x] - mid);
                        else
                            yStart = Math.Ceiling((double)placed[x] - mid);

                        xEnd = yStart;
                        yEnd = xStart * -1;

                        if (xEnd < 0)
                            xEnd = Math.Ceiling(mid + xEnd);
                        else
                            xEnd = Math.Floor(mid + xEnd);
                        if (yEnd < 0)
                            yEnd = Math.Ceiling(mid + yEnd);
                        else
                            yEnd = Math.Floor(mid + yEnd);

                        test[(int)xEnd] = (int)yEnd;
                    }
                }

                return test;
            }

            //not used yet
            private bool HasSymmetry(int[] placed)
            {
                int pos1 = 0;
                int pos2 = BoardSize - 1;
                int checks = BoardSize / 2;

                while (pos2 >= checks)
                {
                    if (BoardSize - placed[pos1] - 1 != placed[pos2])
                        return false;

                    pos1++;
                    pos2--;
                }
                return true;
            }



        }


    }
}
