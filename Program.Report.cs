using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace QueenSolutionConsole
{
    public partial class Program
    {
        static void ReportSummary(Stopwatch sw, int boardSize, long count, long spins, long cores)
        {
            var summary = new StringBuilder()
                .AppendLine($"Board Size = {boardSize}  Total Spin={spins:n0}")
                .AppendLine($"  Time {sw.ElapsedMilliseconds:n0} milliseconds")
                .AppendLine($"  Solutions={count:n0}")
                .AppendLine($" at {DateTime.UtcNow:s}Z by {Environment.UserName}")
                .ToString();

            Console.WriteLine(summary);

            // 1: 1
            // 2: 2
            // 4: 4
            // 8: 5 => 8, 6 => 8, 7=> 8, 8 => 8,
            // 12: 9 => 12, etc
            if (cores > 3)
            {
                // round up to power of 4: 
                cores += 3;
                cores -= cores % 4;
            }

            var dirName = Path.Combine("output", $"cores{cores:00}");
            Directory.CreateDirectory(dirName); // ensure directory exists
            File.WriteAllText(Path.Combine(dirName, $"board_{boardSize:00}_summary.txt"), summary);
        }
    }
}
