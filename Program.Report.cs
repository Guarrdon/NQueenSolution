using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;

namespace QueenSolutionConsole
{
    public partial class Program
    {
        static void ReportSummary(Stopwatch sw)
        {
            var count = _Solvers.Sum(x => x._Count);
            var spin = _Solvers.Sum(x => x._Spin);

            var summary = new StringBuilder()
                .AppendLine($"Board Size = {_BoardSize}  Total Spin={spin}")
                .AppendLine($"Time {sw.ElapsedMilliseconds} milliseconds  Solutions={count}")
                .AppendLine($"Max Parallel Cores {_Parallelism: 00}")
                .AppendLine($" at {DateTime.UtcNow:s}Z by {Environment.UserName}")
                .ToString();

            Console.WriteLine(summary);

            var cores = _Parallelism == 1 ? "" : $"_cores_{_Parallelism: 00}";
            System.IO.Directory.CreateDirectory("output"); // ensure directory exists
            System.IO.File.WriteAllText(System.IO.Path.Combine("output", $"board_{_BoardSize:00}{cores}_summary.txt"), summary);
        }
    }
}
