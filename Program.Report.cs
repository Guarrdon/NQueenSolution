using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace QueenSolutionConsole
{
    public partial class Program
    {
        static void ReportSummary(Stopwatch sw, int boardSize, long count, long spins)
        {
            var summary = new StringBuilder()
                .AppendLine($"Board Size = {boardSize}  Total Spin={spins}")
                .AppendLine($"Time {sw.ElapsedMilliseconds} milliseconds  Solutions={count}")
                .AppendLine($" at {DateTime.UtcNow:s}Z by {Environment.UserName}")
                .ToString();

            Console.WriteLine(summary);

            System.IO.Directory.CreateDirectory("output"); // ensure directory exists
            System.IO.File.WriteAllText(System.IO.Path.Combine("output", $"board_{boardSize:00}_summary.txt"), summary);
        }
    }
}
