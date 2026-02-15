using System;
using System.Collections.Generic;
using System.Text;
using CSUnitRunner.Core.Models;

namespace CSUnitRunner.Presentation
{
    internal static class ConsoleReporter
    {
        public static void PrintReports(List<ClassTestReport> reports)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("\n" + new string('=', 40));
            Console.WriteLine("       TEST EXECUTION REPORT");
            Console.WriteLine(new string('=', 40) + "\n");

            int totalPassed = 0;
            int totalFailed = 0;

            foreach (var report in reports)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"● CLASS: {report.ClassName}");
                Console.ResetColor();

                foreach (var res in report.Results)
                {
                    PrintTestLine(res);

                    if (res.Status == TestStatus.Passed) totalPassed++;
                    else totalFailed++;
                }
                Console.WriteLine();
            }

            PrintSummary(totalPassed, totalFailed);
        }

        private static void PrintTestLine(TestResult res)
        {
            string icon;
            ConsoleColor statusBg;
            ConsoleColor statusFg = ConsoleColor.White;

            if (res.Status == TestStatus.Passed)
            {
                icon = " ✔ ";
                statusBg = ConsoleColor.DarkGreen;
            }
            else
            {
                icon = " ✘ ";
                statusBg = ConsoleColor.DarkRed;
            }

            Console.Write("  "); 
            Console.BackgroundColor = statusBg;
            Console.ForegroundColor = statusFg;
            Console.Write(icon);
            Console.ResetColor();

            Console.Write($" {res.Name,-30}");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[{res.Duration.TotalMilliseconds,6:F0} ms]");
            Console.ResetColor();

            if (res.Status != TestStatus.Passed)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"     └─ Message: {res.ErrorMessage}");
                if (!string.IsNullOrEmpty(res.StackTrace))
                {
                    var lines = res.StackTrace.Split('\n');
                    Console.WriteLine($"     └─ Trace: {lines[0].Trim()}");
                }
                Console.ResetColor();
            }
        }

        private static void PrintSummary(int passed, int failed)
        {
            Console.WriteLine(new string('-', 40));
            Console.Write("TOTAL: ");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{passed} Passed");
            Console.ResetColor();

            Console.Write(", ");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"{failed} Failed");
            Console.ResetColor();

            Console.WriteLine("\n" + new string('-', 40) + "\n");
        }
    }
}