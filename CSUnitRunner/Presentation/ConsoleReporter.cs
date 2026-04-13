using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSUnitRunner.Core.Execution;
using CSUnitRunner.Core.Models;

namespace CSUnitRunner.Presentation;

internal static class ConsoleReporter
{
    private const string ANSI_RESET = "\u001b[0m";
    private const string ANSI_CLEAR_ALL = "\u001b[2J\u001b[3J\u001b[H";

    private const string C_YELLOW = "\u001b[33m";
    private const string C_CYAN = "\u001b[36m";
    private const string C_GREEN = "\u001b[32m";
    private const string C_RED = "\u001b[31m";
    private const string C_MAGENTA = "\u001b[35m";
    private const string C_GRAY = "\u001b[90m";
    private const string C_WHITE = "\u001b[37m";

    static ConsoleReporter()
    {
        try { Console.OutputEncoding = Encoding.UTF8; } catch { }
    }

    public static void PrintDynamicReports(IEnumerable<ClassTestReport> reports, ThreadPoolStatus? poolStatus = null, TimeSpan? elapsedTime = null, IEnumerable<string>? events = null)
    {
        try 
        {
            Console.CursorVisible = false;
            var sb = new StringBuilder();
            sb.Append(ANSI_CLEAR_ALL); 
            BuildReportString(sb, reports, poolStatus, elapsedTime, events);
            Console.Write(sb.ToString());
        } 
        catch { }
    }

    public static void PrintReports(IEnumerable<ClassTestReport> reports, ThreadPoolStatus? poolStatus = null, TimeSpan? elapsedTime = null, IEnumerable<string>? events = null)
    {
        try { Console.CursorVisible = true; } catch { }
        var sb = new StringBuilder();
        sb.Append(ANSI_CLEAR_ALL);
        BuildReportString(sb, reports, poolStatus, elapsedTime, events);
        Console.Write(sb.ToString());
    }

    private static void BuildReportString(StringBuilder sb, IEnumerable<ClassTestReport> reports, ThreadPoolStatus? poolStatus, TimeSpan? elapsedTime, IEnumerable<string>? events)
    {
        if (poolStatus.HasValue)
        {
            var p = poolStatus.Value;
            sb.AppendLine($"{C_MAGENTA}--- THREAD POOL STATUS ---{ANSI_RESET}");
            sb.Append($"{C_WHITE}Threads: {p.TotalThreads}/{p.MaxSize} (Core: {p.CoreSize}), ");
            sb.Append($"{C_CYAN}Busy: {p.BusyThreads}, ");
            sb.Append($"{C_YELLOW}Queue: {p.QueueSize}{ANSI_RESET}");
            sb.AppendLine();
            sb.AppendLine(new string('-', 40));
            sb.AppendLine();
        }

        sb.AppendLine($"{C_YELLOW}========================================{ANSI_RESET}");
        sb.AppendLine($"{C_YELLOW}       TEST EXECUTION REPORT            {ANSI_RESET}");
        sb.AppendLine($"{C_YELLOW}========================================{ANSI_RESET}");
        sb.AppendLine();

        int totalPassed = 0;
        int totalFailed = 0;
        int totalRunning = 0;
        int totalPending = 0;

        var sortedReports = reports.OrderBy(r => r.ClassName).ToList();

        foreach (var report in sortedReports)
        {
            sb.AppendLine($"{C_CYAN}● CLASS: {report.ClassName}{ANSI_RESET}");

            List<TestResult> results;
            lock (report.SyncLock)
            {
                results = report.Results.OrderBy(res => res.Name).ToList();
            }

            foreach (var res in results)
            {
                AppendTestLine(sb, res);

                if (res.Status == TestStatus.Passed) totalPassed++;
                else if (res.Status == TestStatus.Failed || res.Status == TestStatus.Error) totalFailed++;
                else if (res.Status == TestStatus.Running) totalRunning++;
                else if (res.Status == TestStatus.Pending) totalPending++;
            }
            sb.AppendLine();
        }

        sb.AppendLine(new string('-', 40));
        sb.Append("TOTAL: ");
        sb.Append($"{C_GREEN}{totalPassed} Passed{ANSI_RESET}, ");
        sb.Append($"{C_RED}{totalFailed} Failed{ANSI_RESET}");

        if (totalRunning > 0) 
        {
            sb.Append($", {C_YELLOW}{totalRunning} Running{ANSI_RESET}");
        }

        if (totalPending > 0)
        {
            sb.Append($", {C_GRAY}{totalPending} Queued{ANSI_RESET}");
        }
        
        sb.AppendLine();
        
        if (elapsedTime.HasValue)
        {
            sb.AppendLine($"{C_WHITE}Time: {elapsedTime.Value.TotalSeconds:F2} seconds{ANSI_RESET}");
        }
        
        sb.AppendLine(new string('-', 40));

        sb.AppendLine($"{C_MAGENTA}--- RECENT POOL EVENTS ---{ANSI_RESET}");
        if (events != null && events.Any())
        {
            foreach (var ev in events.Reverse().Take(10).Reverse())
            {
                sb.AppendLine($"{C_GRAY}{ev}{ANSI_RESET}");
            }
        }
        else
        {
            sb.AppendLine($"{C_GRAY}[None]{ANSI_RESET}");
        }
        
        sb.AppendLine();
    }

    private static void AppendTestLine(StringBuilder sb, TestResult res)
    {
        string icon;
        string iconStyle;
        string textStyle;
        
        if (res.Status == TestStatus.Passed) 
        { 
            icon = "\u2713";
            iconStyle = "\u001b[1;32m";
            textStyle = C_GREEN; 
        }
        else if (res.Status == TestStatus.Failed || res.Status == TestStatus.Error) 
        { 
            icon = "✘"; 
            iconStyle = C_RED; 
            textStyle = C_RED; 
        }
        else if (res.Status == TestStatus.Running) 
        { 
            icon = "⟳"; 
            iconStyle = C_YELLOW; 
            textStyle = C_YELLOW; 
        }
        else 
        { 
            icon = "…"; 
            iconStyle = C_GRAY; 
            textStyle = C_GRAY; 
        }

        string timeStr;
        if (res.Status == TestStatus.Pending)
        {
            timeStr = "  wait   ";
        }
        else
        {
            double ms = res.Status == TestStatus.Running && res.StartTime.HasValue
                ? (DateTime.Now - res.StartTime.Value).TotalMilliseconds
                : res.Duration.TotalMilliseconds;
            timeStr = $"{ms,6:F0} ms";
        }

        sb.Append("  ");
        sb.Append($"{iconStyle}{icon}{ANSI_RESET} ");
        sb.AppendLine($"{textStyle}{res.Name,-37} [{timeStr}]{ANSI_RESET}");

        if (res.Status == TestStatus.Failed || res.Status == TestStatus.Error)
        {
            sb.AppendLine($"{C_RED}     └─ Error: {res.ErrorMessage}{ANSI_RESET}");
            if (!string.IsNullOrEmpty(res.StackTrace))
            {
                var firstLine = res.StackTrace.Split('\n')[0].Trim();
                sb.AppendLine($"{C_RED}     └─ Stack: {firstLine}{ANSI_RESET}");
            }
        }
    }
}