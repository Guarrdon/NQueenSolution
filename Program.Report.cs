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
                .AppendLine($"Board Size = {boardSize}  Total Spin={spins}")
                .AppendLine($"Time {sw.ElapsedMilliseconds} milliseconds  Solutions={count}")
                .AppendLine($" at {DateTime.UtcNow:s}Z by {Environment.UserName}")
                .ToString();

            Console.WriteLine(summary);

            var dirName = Path.Combine("output", $"cores{cores:00}");
            Directory.CreateDirectory(dirName); // ensure directory exists
            File.WriteAllText(Path.Combine(dirName, $"board_{boardSize:00}_summary.txt"), summary);
        }
    }
}
