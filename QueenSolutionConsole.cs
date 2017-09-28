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
        private const int _BoardSize = 16;
        //private static long _Count;
        //private static long _Spin;

        //private static int[] _Current;
        //private static int[] _Rotated;
        //private static Stack<int[]> _Solutions = new Stack<int[]>();

        public static void Main(string[] args)
        {
            //if (args.Any())
            //    _BoardSize = int.Parse(args[0]);

            var sw = Stopwatch.StartNew();

            long count = 0;
            int[][] current = new int[_BoardSize][];
            //long[] spins = new long[_BoardSize];
            Stack<int[]>[] solutions = new Stack<int[]>[_BoardSize];

            for (int x = 0; x < _BoardSize; x++)
            {
                current[x] = new int[_BoardSize];
                //spins[x] = 0;
                solutions[x] = new Stack<int[]>();
            }

            ParallelOptions po = new ParallelOptions()
            {
                MaxDegreeOfParallelism = 8,
            };

            var result = Parallel.For(0, _BoardSize, po,
                (index) =>
                {
                    var cnt = BuildNew(0, index, index + 1, 0, 0, 0, ref current[index],  ref solutions[index]);
                    Interlocked.Add(ref count, cnt);
                });

            sw.Stop();
            ReportSummary(sw, count, 0);
        }

        //recursive method to iterate all potential positions
        //maintain state as we go so when we come back to a previous loop and iterate, we don't have to rebuild the entire state
        //only iterate through half as we use Y-flip to find other solutions
        private static long BuildNew(int position, int start, int end, long yline, long updiag, long downdiag, ref int[] current, ref Stack<int[]> solutions)
        {
            long YLine = 0;
            long UpDiag = 0;
            long DownDiag = 0;

            long tempY = 0;
            long tempUp = 0;
            long tempDown = 0;

            long count = 0;

            for (int x = start; x < end; x++)
            {
                //spins++;

                tempY = 2 << x;
                tempUp = 2 << (_BoardSize + x - position);
                tempDown = 2 << (position + x);

                if (((tempY & yline) == 0) &&
                    ((tempDown & downdiag) == 0) &&
                    ((tempUp & updiag) == 0))
                {

                    if (position == _BoardSize - 1)
                    {
                        //current[position] = x;

                        count++;
                        //int[] t1 = new int[_BoardSize];
                        //Array.Copy(current, t1, _BoardSize);
                        //solutions.Push(t1);

                    }
                    if (position < _BoardSize - 1)
                    {
                        YLine = yline;
                        UpDiag = updiag;
                        DownDiag = downdiag;

                        yline = tempY | yline;
                        downdiag = tempDown | downdiag;
                        updiag = tempUp | updiag;

                        count += BuildNew(position + 1, 0, _BoardSize, yline, updiag, downdiag, ref current,  ref solutions);

                        yline = YLine;
                        updiag = UpDiag;
                        downdiag = DownDiag;
                    }
                }

            }
            return count;

        }


        //not used yet
        private static int[] XFlip(int[] placed)
        {
            int[] test = new int[_BoardSize];
            int temp;
            for (int x = 0; x < _BoardSize / 2; x++)
            {
                temp = placed[_BoardSize - x - 1];
                test[_BoardSize - x - 1] = placed[x];
                test[x] = temp;
            }

            return test;
        }

        //not used
        private static int[] YFlip(int[] placed)
        {
            int[] test = new int[_BoardSize];
            for (int x = 0; x < _BoardSize; x++)
                test[x] = _BoardSize - 1 - placed[x];

            return test;
        }

        //not used yet
        private static int[] Rotate90(int[] placed)
        {
            int[] test = new int[_BoardSize];
            //id mid point
            var mid = ((double)_BoardSize - 1.0) / 2.0;
            double xStart;
            double yStart;
            double xEnd;
            double yEnd;

            for (int x = 0; x < _BoardSize; x++)
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
        private static bool HasSymmetry(int[] placed)
        {
            int pos1 = 0;
            int pos2 = _BoardSize - 1;
            int checks = _BoardSize / 2;

            while (pos2 >= checks)
            {
                if (_BoardSize - placed[pos1] - 1 != placed[pos2])
                    return false;

                pos1++;
                pos2--;
            }
            return true;
        }


    }
}
