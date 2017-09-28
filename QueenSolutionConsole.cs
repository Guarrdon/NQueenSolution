using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace QueenSolutionConsole
{
    public partial class Program
    {
        static int _BoardSize = 15;
        static int _Parallelism = 8;
        static int _MidY;
        static Program[] _Solvers;

        long _Count;
        long _Spin;
        int[] _Current;

        int xStart; // first column start
        int xEnd;   // first column end

        public static void Main(string[] args)
        {
            if (args.Any())
                _BoardSize = int.Parse(args[0]);
            if (_BoardSize > 16)
                throw new ArgumentOutOfRangeException(nameof(_BoardSize), "Branches derived from dedupe_long cannot run board sizes over 16.");

            var sw = Stopwatch.StartNew();

            var firstColumnEnd = (_BoardSize + 1) / 2;
            if (_BoardSize % 2 == 1)
                _MidY = firstColumnEnd - 1; // odd: last column computed
            else
                _MidY = -1; // no midpoint column for even number of columns

#if SINGLE_THREADED
            var solver = new Program() { xStart = 0, xEnd = firstColumnEnd };
            _Parallelism = 1;                     // for reporting
            _Solvers = new Program[1] { solver }; // for reporting
            solver.Solve();
#else
            _Solvers = new Program[firstColumnEnd];
            for (int i = 0; i < _Solvers.Length; ++i)
            {
                _Solvers[i] = new Program() { xStart = i, xEnd = i + 1 };
            }

            var parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = _Parallelism };
            var result = Parallel.ForEach(_Solvers, parallelOptions, solver => solver.Solve());
#endif
            sw.Stop();
            ReportSummary(sw);
        }

        public Program()
        {
            _Current = new int[_BoardSize];
        }

        void Solve()
        {
            BuildNew(0, 0, 0, 0);
        }

        //recursive method to iterate all potential positions
        //maintain state as we go so when we come back to a previous loop and iterate, we don't have to rebuild the entire state
        //only iterate through half as we use Y-flip to find other solutions
        void BuildNew(int position, long yline, long updiag, long downdiag)
        {
            long YLine = 0;
            long UpDiag = 0;
            long DownDiag = 0;

            long tempY = 0;
            long tempUp = 0;
            long tempDown = 0;

            int start = (position == 0) ? xStart : 0;
            int end = (position == 0) ? xEnd : _BoardSize;
            for (int x = start; x < end; x++)
            {
                _Spin++;
                _Current[position] = x;

                tempY = 2 << x;
                tempUp = 2 << (_BoardSize + x - position);
                tempDown = 2 << (position + x);

                if (((tempY & yline) == 0) &&
                    ((tempDown & downdiag) == 0) &&
                    ((tempUp & updiag) == 0))
                {
                    if (position == _BoardSize - 1)
                    {
                        _Count++;
                        if (_Current[0] != _MidY)
                            _Count++; // count the y-flip without checking it: there is no posibility of duplicates or collisions
                    }
                    else if (position < _BoardSize - 1)
                    {
                        YLine = yline;
                        UpDiag = updiag;
                        DownDiag = downdiag;

                        yline = tempY | yline;
                        downdiag = tempDown | downdiag;
                        updiag = tempUp | updiag;

                        BuildNew(position + 1, yline, updiag, downdiag);

                        yline = YLine;
                        updiag = UpDiag;
                        downdiag = DownDiag;
                    }
                }

            }
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

        //used to add y axis flipped solution
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
