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
        static int BoardSize = 15;
        static int BoardSizeMinus1 = BoardSize - 1;

        public static void Main(string[] args)
        {
            if (args.Any())
            {
                BoardSize = int.Parse(args[0]);
                BoardSizeMinus1 = BoardSize - 1;
            }

            var sw = Stopwatch.StartNew();

            long count = 0;
            long spin = 0;
            long activeSolvers = 0;
            long maxNumberActiveSolvers = 0;

            if (BoardSize < 12)
            {
                var solver = new Solver(BoardSize);
                solver.Build1(0);
                spin = solver.Spin;
                count = solver.Count;
                maxNumberActiveSolvers = 1;
            }
            else
            {
                var po = new ParallelOptions() { MaxDegreeOfParallelism = 8 };

                Parallel.For(0, BoardSize, po, (index) =>
                {
                    try
                    {
                        long active = Interlocked.Increment(ref activeSolvers);
                        if (active > maxNumberActiveSolvers)
                            maxNumberActiveSolvers = active; // this doesn't have to be threadsafe... 
                        var solver = new Solver(BoardSize);
                        solver.Build0(0, index, index + 1);
                        Interlocked.Add(ref count, solver.Count);
                        Interlocked.Add(ref spin, solver.Spin);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref activeSolvers);
                    }
                });
            }

            sw.Stop();
            ReportSummary(sw, BoardSize, count, spin, maxNumberActiveSolvers);
        }

        public class Solver
        {
            public long Spin;
            public long Count;
            private int[] _Current;
            private uint _YLine;
            private uint _DownDiag;
            private uint _UpDiag;

            public Solver(int boardSize)
            {
                _Current = new int[BoardSize];
            }

            //recursive method to iterate all potential positions
            //maintain state as we go so when we come back to a previous loop and iterate, we don't have to rebuild the entire state
            //only iterate through half as we use Y-flip to find other solutions
            public void Build0(int position, int start, int end)
            {
                uint tempY, tempDown, tempUp;

                if (start == 0)
                {
                    tempY = 1u;
                    tempUp = 1u << position;
                    // Natural down diaganal would be -8 (h1) => +8 (a8).
                    // We shift it by +BoardSize to keep values positive.  So our 
                    // down diaganal is 0 (h1) => +16 (a8).
                    tempDown = 1u << BoardSize + position;
                }
                else if (position == 0)
                {
                    tempY = 1u << start;
                    tempUp = 1u << start;
                    tempDown = 1u << BoardSize - start;
                }
                else
                    throw new Exception("bad state for optimized shift");

                for (int x = start; x < end; x++, tempY <<= 1, tempUp <<= 1, tempDown >>= 1)
                {
                    Spin++;

                    if ((tempY & _YLine) == 0 &&
                        (tempDown & _DownDiag) == 0 &&
                        (tempUp & _UpDiag) == 0)
                    {
                        _Current[position] = x;

                        _YLine += tempY;
                        _DownDiag += tempDown;
                        _UpDiag += tempUp;

                        Build1(position + 1);

                        _YLine -= tempY;
                        _DownDiag -= tempDown;
                        _UpDiag -= tempUp;
                    }

                }
            }

            public void Build1(int position)
            {
                uint tempY = 1u;
                uint tempUp = 1u << position;

                // Natural down diaganal would be -8 (h1) => +8 (a8).
                // We shift it by +BoardSize to keep values positive.  So our 
                // down diaganal is 0 (h1) => +16 (a8).
                uint tempDown = 1u << BoardSize + position;

                for (int x = 0; x < BoardSize; x++, tempY <<= 1, tempUp <<= 1, tempDown >>= 1)
                {
                    Spin++;

                    if ((tempY & _YLine) == 0 &&
                        (tempDown & _DownDiag) == 0 &&
                        (tempUp & _UpDiag) == 0)
                    {
                        _Current[position] = x;

                        if (position < BoardSizeMinus1)
                        {
                            _YLine += tempY;
                            _DownDiag += tempDown;
                            _UpDiag += tempUp;

                            Build1(position + 1);

                            _YLine -= tempY;
                            _DownDiag -= tempDown;
                            _UpDiag -= tempUp;
                        }
                        else //if (position == BoardSize - 1)
                        {
                            Count++;
                            //int[] t1 = new int[BoardSize];
                            //Array.Copy(_Current, t1, BoardSize);
                            //Solutions.Push(t1);
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
