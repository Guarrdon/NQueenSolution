using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Stopwatch = System.Diagnostics.Stopwatch;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace QueenSolutionConsole
{
    // diagonal has to have number of bits: boardsize * 2
    // boardsize 16 is largest board that fits in UInt32
    // boardsize 17+ requires UInt64
    using TestIntType = System.UInt64;

    public partial class Program
    {
        static int BoardSize = 10;
        static int BoardSizeMinus1 = BoardSize - 1;

        public static void Main(string[] args)
        {
            if (args.Any())
            {
                BoardSize = int.Parse(args[0]);
                BoardSizeMinus1 = BoardSize - 1;
            }

            if (BoardSize <= 12)
            {
                BackTrackingSingleThreaded();
            }
            else
            {
                //BackTrackingParallel();
                FirstColumnHalfWithFlip();
            }
        }

        static void BackTrackingSingleThreaded()
        {
            var sw = Stopwatch.StartNew();
            var solver = new Solver();


            solver.Build1(0);

            sw.Stop();
            ReportSummary(sw, BoardSize, solver.SolutionCount, solver.Spin, 1);
        }

        static void BackTrackingParallel()
        {
            var sw = Stopwatch.StartNew();
            long activeSolvers = 0;
            long maxNumberActiveSolvers = 0;

            var bag = new ConcurrentBag<Solver>();

            var po = new ParallelOptions() { MaxDegreeOfParallelism = 8 };
            Parallel.For(0, BoardSize, po, (index) =>
            {
                try
                {
                    long active = Interlocked.Increment(ref activeSolvers);
                    if (active > maxNumberActiveSolvers)
                        maxNumberActiveSolvers = active;

                    var solver = new Solver();
                    solver.Build0(0, index, index + 1);
                    bag.Add(solver);
                }
                finally
                {
                    Interlocked.Decrement(ref activeSolvers);
                }
            });

            sw.Stop();
            ReportSummary(sw, BoardSize, bag.Sum(x => x.SolutionCount), bag.Sum(x => x.Spin), maxNumberActiveSolvers);
        }


        static void FirstColumnHalfWithFlip()
        {
            var sw = Stopwatch.StartNew();
            long activeSolvers = 0;
            long maxNumberActiveSolvers = 0;

            var bag = new ConcurrentBag<Solver>();

            // 8 => 4, 9 => 5, 10 => 5
            int midPoint = (BoardSize + 1) / 2;
            // 0, 1, 2, 3, 4 - - board size 9 or 10

            // TODO: test without MaxParallel=8.  IOW let the CPU oversubscribe a bit, IOW trust the OS & CPU to be "fully busy" and balance the work (over .NET Parallel.For() to schedule).
            var po = new ParallelOptions() { MaxDegreeOfParallelism = 8 };
            Parallel.For(0, midPoint, po, (index) =>
            {
                try
                {
                    bool isMidpointOdd = index == 0 && BoardSize % 2 == 1;

                    // 4, 3, 2, 1, 0 - board size 9 or 10
                    index = (midPoint - 1) - index; // count down, not up
                    long active = Interlocked.Increment(ref activeSolvers);
                    if (active > maxNumberActiveSolvers)
                        maxNumberActiveSolvers = active; // this doesn't have to be threadsafe... 

                    // Start/run the mid-point solver first, b/c it has to flip and de-duplicate each solution, and we don't want the long-poll to run last.
                    var solver = isMidpointOdd ? new Solver() as Solver
                        : new SolverFlipNoDedupe();
                    solver.Build0(0, index, index + 1);
                    bag.Add(solver);
                }
                finally
                {
                    Interlocked.Decrement(ref activeSolvers);
                }
            });

            sw.Stop();
            ReportSummary(sw, BoardSize, bag.Sum(x => x.SolutionCount), bag.Sum(x => x.Spin), maxNumberActiveSolvers);
        }
        const TestIntType _1u = 1u;

        public class Solver
        {
            public long Spin;
            protected long Count;
            protected int[] _Current;
            protected TestIntType _YLine;
            protected TestIntType _DownDiag;
            protected TestIntType _UpDiag;

            public TreeNode root = new TreeNode();
            int TreeCount;

            public Solver()
            {
                _Current = new int[BoardSize];
            }

            public virtual long SolutionCount => Count;

            //recursive method to iterate all potential positions
            //maintain state as we go so when we come back to a previous loop and iterate, we don't have to rebuild the entire state
            //only iterate through half as we use Y-flip to find other solutions
            public void Build0(int position, int start, int end)
            {
                root = ((end - start) == 1) ? new TreeNode() : new TreeNode(0);
                TestIntType tempY, tempDown, tempUp;

                if (start == 0)
                {
                    tempY = _1u;
                    tempUp = _1u << position;
                    // Natural down diaganal would be -8 (h1) => +8 (a8).
                    // We shift it by +BoardSize to keep values positive.  So our 
                    // down diaganal is 0 (h1) => +16 (a8).
                    tempDown = _1u << BoardSize + position;
                }
                else if (position == 0)
                {
                    tempY = _1u << start;
                    tempUp = _1u << start;
                    tempDown = _1u << BoardSize - start;
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

                        Build1(position + 1, () => root.Add(position, x));

                        _YLine -= tempY;
                        _DownDiag -= tempDown;
                        _UpDiag -= tempUp;
                    }

                }
            }

            public void Build1(int position, Func<TreeNode> parentNodeFunc)
            {
                Lazy<TreeNode> parentNode = new Lazy<TreeNode>(parentNodeFunc);

                TestIntType tempY = _1u;
                TestIntType tempUp = _1u << position;
                TestIntType tempDown = _1u << BoardSize + position;

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

                            Build1(position + 1, () => parentNode.Value.Add(position, x));

                            _YLine -= tempY;
                            _DownDiag -= tempDown;
                            _UpDiag -= tempUp;
                        }
                        else //if (position == BoardSize - 1)
                        {
                            if (!parentNode.Value.Children.ContainsKey(x))
                            {
                                Count++;
                                parentNode.Value.Children.Add(x, null);
                            }
                            //RecordSolution();
                        }
                    }
                }
            }

            public static int[] YFlip(int[] placed)
            {
                int[] test = new int[BoardSize];
                for (int x = 0; x < BoardSize; x++)
                    test[x] = BoardSizeMinus1 - placed[x];

                return test;
            }

            public class TreeNode
            {
                public TreeNode()
                {
                    Children = new Dictionary<int, TreeNode>();
                }

                public TreeNode(int position)
                {
                    if (position <= BoardSize / 2)
                    {
                        Flat = new ValueTuple<int, TreeNode>[BoardSize];
                    }
                    else
                        Children = new Dictionary<int, TreeNode>();
                }

                public Dictionary<int, TreeNode> Children { get; set; } //= new Dictionary<byte, TreeNode>();

                public ValueTuple<int, TreeNode> [] Flat;

                public TreeNode Add(int position, int y)
                {
                    TreeNode node;
                    if (Flat != null)
                    {
                        if (Flat[y].Item2 == null)
                            Flat[y] = ValueTuple.Create(y, node = new TreeNode(position));
                        else
                            return Flat[y].Item2;
                    }
                    else if (!Children.ContainsKey(y))
                    {
                        node = new TreeNode(position);
                        Children.Add(y, node);
                    }
                    else
                        node = Children[y];
                    return node;
                }
            }

            public virtual void RecordSolution()
            {
                //AddSolution(_Current);
                AddSolution(YFlip(_Current));
            }

            public void AddSolution(int[] solution)
            {
                var node = root;
                for (int i = 0; i < BoardSize / 2; ++i)
                {
                    var y = solution[i];
                    if (!node.Children.ContainsKey(y))
                    {
                        var newNode = new TreeNode();
                        node.Children.Add(y, newNode);
                        node = newNode;
                    }
                    else
                        node = node.Children[y];
                }

                for (int i = BoardSize / 2; i < BoardSizeMinus1; ++i)
                {
                    byte y = (byte)solution[i];
                    if (!node.Children.ContainsKey(y))
                    {
                        var newNode = new TreeNode();
                        node.Children.Add(y, newNode);
                        node = newNode;
                    }
                    else
                        node = node.Children[y];
                }

                byte y2 = (byte)solution[BoardSizeMinus1];
                if (!node.Children.ContainsKey(y2))
                {
                    node.Children.Add(y2, null);
                    ++TreeCount;
                }
            }

            public void Build1(int y)
            {
                root = new TreeNode(0);
                Build1(0, () => root);
            }

            #region not used yet
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
            #endregion // not used yet
        }

        public class SolverFlipNoDedupe : Solver
        {
            public override long SolutionCount => Count * 2;
        }
    }
}
