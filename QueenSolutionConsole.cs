using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace QueenSolutionConsole
{
    public class Program
    {

        //constant table size >=4
        private const int _BoardSize = 15;
        private static long _Count;
        private static long _Spin;
        private static int[][] _Loops = new int[_BoardSize][];
        private static int[] _Current = new int[_BoardSize];
        private static int[] _Rotated;
        private static ConcurrentDictionary<string, int[]> _Solutions = new ConcurrentDictionary<string, int[]>();


        public static void Main(string[] args)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
  
            _Count = 0;
            _Spin = 0;

            BuildNew(0, (_BoardSize + 1) / 2, 0, 0, 0);
            
            sw.Stop();
            Console.WriteLine($"Board Size = {_BoardSize}  Total Spin={_Spin}");
            Console.WriteLine($"Time {sw.ElapsedMilliseconds} milliseconds  Solutions={_Solutions.Count}");

        }

      
   
        //recursive method to iterate all potential positions
        //maintain state as we go so when we come back to a previous loop and iterate, we don't have to rebuild the entire state
        //only iterate through half as we use Y-flip to find other solutions
        private static void BuildNew(int position, int end, long yline, long updiag, long downdiag)
        {
            long YLine = 0;
            long UpDiag = 0;
            long DownDiag = 0;
            bool test = true;

            long tempY = 0;
            long tempUp = 0;
            long tempDown = 0;

            for (int x = 0; x < end; x++)
            {
                _Spin++;
                _Current[position] = x;

                tempY = 2 << x;
                tempUp = 2 << (_BoardSize + x - position);
                tempDown = 2 << (position + x);

                test = (tempY & yline) != tempY;
                test = test && ((tempDown & downdiag) != tempDown);
                test = test && ((tempUp & updiag) != tempUp);

                if (test)
                {
                    if (position == _BoardSize - 1)
                    {
                        _Count++;
                        _Solutions.TryAdd(string.Join(",", _Current), _Current);
                        _Rotated = YFlip(_Current);
                        _Solutions.TryAdd(string.Join(",", _Rotated), _Rotated);
                    }
                    if (position < _BoardSize - 1)
                    {
                        YLine = yline;
                        UpDiag = updiag;
                        DownDiag = downdiag;

                        yline = tempY | yline;
                        downdiag = tempDown | downdiag;
                        updiag = tempUp | updiag;

                        BuildNew(position + 1, _BoardSize, yline, updiag, downdiag);

                        yline = YLine;
                        updiag = UpDiag;
                        downdiag = DownDiag;
                    }
                }
                test = true;
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
