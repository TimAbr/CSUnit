using CSUnitRunner.Core.Execution;
using CSUnitRunner.Core.Preparation;
using CSUnitRunner.Infrastructure.Repositories;
using CSUnitRunner.Presentation;

namespace CSUnitRunner;

class Program
{
    static void Main(string[] args)
    {
        var config = ParseArgs(args);
        if (config == null) return;

        try
        {
            var repository = new MethodsTreeRepository();
            var rawTree = repository.BuildTreeFromFile(config.DllPath);

            var filter = new TreeFilter();
            var filteredTree = filter.Filter(rawTree);

            if (filteredTree == null)
            {
                Console.WriteLine("No active tests found in the assembly.");
                return;
            }

            var analyzer = new TreeAnalyzer();
            var executableTree = analyzer.Analyze(filteredTree);

            var pool = new CustomThreadPool(
                coreSize: config.CoreThreads, 
                maxSize: config.MaxThreads > 0 ? Math.Max(config.CoreThreads, config.MaxThreads) : config.CoreThreads, 
                keepAliveTime: TimeSpan.FromSeconds(config.KeepAliveSeconds));

            var executor = new TestExecutor(pool);

            var startTime = DateTime.Now;
            bool isFinished = false;
            pool.Enqueue(() =>
            {
                while (!isFinished)
                {
                    try { ConsoleReporter.PrintDynamicReports(executor.Reports, pool.GetStatus(), DateTime.Now - startTime); } catch { }
                    Thread.Sleep(200);
                }
            });

            executor.Execute(executableTree);
            
            isFinished = true;
            var finalDuration = DateTime.Now - startTime;
            Thread.Sleep(300); // Give reporter time for final frame

            ConsoleReporter.PrintDynamicReports(executor.Reports, pool.GetStatus(), finalDuration);

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[FATAL ERROR]: {ex.Message}");
            Console.ResetColor();
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }

    private class RunnerConfig
    {
        public string DllPath { get; set; } = string.Empty;
        public int CoreThreads { get; set; } = 3;
        public int MaxThreads { get; set; } = 0; // 0 means not specified by user
        public int KeepAliveSeconds { get; set; } = 10;
        public string? TargetClass { get; set; }
        public string? TargetMethod { get; set; }
    }

    private static RunnerConfig? ParseArgs(string[] args)
    {
        var config = new RunnerConfig();

        if (args.Length == 1 && !args[0].StartsWith("--"))
        {
            config.DllPath = args[0];
            return config;
        }

        for (var i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            string? value = null;

            if (arg.Contains('='))
            {
                var parts = arg.Split('=', 2);
                arg = parts[0];
                value = parts[1];
            }
            else if (i + 1 < args.Length && !args[i + 1].StartsWith("--"))
            {
                value = args[++i];
            }

            switch (arg)
            {
                case "--dll":
                    if (value != null) config.DllPath = value;
                    break;
                case "--threads":
                    if (value != null && int.TryParse(value, out var core) && core >= 1) config.CoreThreads = core;
                    break;
                case "--max-threads":
                    if (value != null && int.TryParse(value, out var max) && max >= 1) config.MaxThreads = max;
                    break;
                case "--keep-alive":
                    if (value != null && int.TryParse(value, out var keep) && keep >= 1) config.KeepAliveSeconds = keep;
                    break;
                case "--class":
                    if (value != null) config.TargetClass = value;
                    break;
                case "--method":
                    if (value != null) config.TargetMethod = value;
                    break;
                default:
                    if (!arg.StartsWith("--") && string.IsNullOrEmpty(config.DllPath))
                    {
                        config.DllPath = arg;
                    }
                    break;
            }
        }

        if (!string.IsNullOrEmpty(config.DllPath)) return config;
        
        Console.WriteLine("Usage: CSUnitRunner --dll <path> [--threads <n>] [--max-threads <n>] [--keep-alive <sec>] [--class <name>] [--method <name>]");
        return null;
    }
}