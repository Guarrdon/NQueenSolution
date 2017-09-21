using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace QueenSolutionConsole
{
    public class Program
    {

        //constant table size >=4
        private const int _BoardSize = 12;
        private static long _Count;
        private static long _Spin;
        private static int[][] _Loops = new int[_BoardSize][];
        private static int[] _Current = new int[_BoardSize];
        private static int[] _Rotated;
        private static ConcurrentDictionary<string, int[]> _Solutions = new ConcurrentDictionary<string, int[]>();


        public static void Main(string[] args)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            BuildBlankSet();
            BuildStateLoops();

            _Count = 0;
            _Spin = 0;

            BuildNew(0, (_BoardSize + 1) / 2);
            sw.Stop();
            Console.WriteLine($"Board Size = {_BoardSize}  Total Spin={_Spin}");
            Console.WriteLine($"Time {sw.ElapsedMilliseconds} milliseconds  Solutions={_Solutions.Count}");
           
        }

        //initialize static current solution array
        private static void BuildBlankSet()
        {
            for (int j = 0; j < _BoardSize; j++)
                _Current[j] = -1;
        }
        //initialize static stateful loop arrays
        private static void BuildStateLoops()
        {
            for (int x = 0; x < _BoardSize; x++)
            {
                _Loops[x] = new int[_BoardSize];
                for (int y = 0; y < _BoardSize; y++)
                    _Loops[x][y] = y;
            }
        }

        //recursive method to iterate all potential positions
        //only include valid solutions
        //maintain state as we go so when we come back to a previous loop and iterate, we don't have to rebuild the entire state
        //only iterate through half as we use Y-flip to find other solutions
        private static void BuildNew(int position, int end)
        {
            int[][] state = new int[_BoardSize][];
            for (int x = 0; x < _BoardSize; x++)
                state[x] = new int[_BoardSize];

            for (int x = 0; x < end; x++)
            {
                if (_Loops[position][x] >= 0)
                {
                    _Spin++;
                    _Current[position] = x;

                    if (position == _BoardSize - 1)
                    {
                        _Count++;
                        _Solutions.TryAdd(string.Join(",", _Current), _Current);

                        _Rotated = YFlip(_Current);
                        _Solutions.TryAdd(string.Join(",", _Rotated), _Rotated);
                    }
                    if (position < _BoardSize - 1)
                    {
                        for (int i = 1; position + i < _BoardSize; i++)
                            Array.Copy(_Loops[position + i], state[position + i], _BoardSize);

                        for (int i = 1; position + i < _BoardSize; i++)
                        {
                            _Loops[position + i][x] = -1;
                            if (x - i >= 0)
                                _Loops[position + i][x - i] = -1;
                            if (x + i < _BoardSize)
                                _Loops[position + i][x + i] = -1;
                        }
                        BuildNew(position + 1, _BoardSize);


                        for (int i = 1; position + i < _BoardSize; i++)
                            Array.Copy(state[position + i], _Loops[position + i], _BoardSize);
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
