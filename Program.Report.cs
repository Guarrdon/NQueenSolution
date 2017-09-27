using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace QueenSolutionConsole
{
    public partial class Program
    {
        static void ReportSummary(Stopwatch sw)
        {
            var summary = new StringBuilder()
                .AppendLine($"Board Size = {_BoardSize}  Total Spin={_Spin}")
                .AppendLine($"Time {sw.ElapsedMilliseconds} milliseconds  Solutions={_Solutions.Count}")
                .AppendLine($" at {DateTime.UtcNow:s}Z by {Environment.UserName}")
                .ToString();

            Console.WriteLine(summary);

            System.IO.Directory.CreateDirectory("output"); // ensure directory exists
            System.IO.File.WriteAllText(System.IO.Path.Combine("output", $"board_{_BoardSize:00}_summary.txt"), summary);
        }
    }
}
