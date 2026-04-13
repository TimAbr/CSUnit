using System.Reflection;
using CSUnit.Attributes;
using CSUnitRunner.Core.Execution;
using CSUnitRunner.Core.Preparation;
using CSUnitRunner.Core.Logging;
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
            TestLogger.Clear();
            var repository = new MethodsTreeRepository();
            var rawTree = repository.BuildTreeFromFile(config.DllPath);

            Func<MemberInfo, bool>? filterDelegate = null;
            if (!string.IsNullOrEmpty(config.Category))
            {
                filterDelegate = member => 
                {
                    var categories = member.GetCustomAttributes<CategoryAttribute>();
                    return categories.Any(c => c.Category.Equals(config.Category, StringComparison.OrdinalIgnoreCase));
                };
            }

            var filter = new TreeFilter(config.TargetClass, config.TargetMethod, filterDelegate);
            var filteredTree = filter.Filter(rawTree);

            if (filteredTree == null)
            {
                Console.WriteLine("No active tests found in the assembly.");
                return;
            }

            var analyzer = new TreeAnalyzer();
            var executableTree = analyzer.Analyze(filteredTree);

            var poolEvents = new System.Collections.Concurrent.ConcurrentQueue<string>();

            var pool = new CustomThreadPool(
                coreSize: config.CoreThreads, 
                maxSize: config.MaxThreads > 0 ? Math.Max(config.CoreThreads, config.MaxThreads) : config.CoreThreads, 
                keepAliveTime: TimeSpan.FromSeconds(config.KeepAliveSeconds));

            pool.OnThreadCreated += t => { 
                var msg = $"[THREAD]: Created -> {t.Name}";
                poolEvents.Enqueue(msg);
                TestLogger.Info(msg);
            };
            pool.OnThreadRemoved += t => {
                var msg = $"[THREAD]: Retired -> {t.Name}";
                poolEvents.Enqueue(msg);
                TestLogger.Info(msg);
            };
            pool.OnThreadHung += (t, d) => {
                var msg = $"[THREAD]: !!! HUNG !!! -> {t.Name} ({d.TotalSeconds}s)";
                poolEvents.Enqueue(msg);
                TestLogger.Info(msg);
            };

            pool.OnTaskStarted += (t, a) => {
                TestLogger.Info($"[TASK]: Started on {t.Name}");
            };
            pool.OnTaskCompleted += (t, a) => {
                TestLogger.Info($"[TASK]: Completed on {t.Name}");
            };

            var executor = new TestExecutor(pool);

            var startTime = DateTime.Now;
            bool isFinished = false;
            pool.Enqueue(() =>
            {
                while (!isFinished)
                {
                    try { ConsoleReporter.PrintDynamicReports(executor.Reports, pool.GetStatus(), DateTime.Now - startTime, poolEvents); } catch { }
                    Thread.Sleep(200);
                }
            });

            executor.Execute(executableTree);
            
            isFinished = true;
            Thread.Sleep(300);

            ThreadPoolStatus? finalStatus = null;
            try { finalStatus = pool.GetStatus(); } catch { }

            ConsoleReporter.PrintReports(executor.Reports, finalStatus, DateTime.Now - startTime, poolEvents);
            pool.Dispose();

            Console.WriteLine("\nExecution finished. Press any key to exit...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[FATAL ERROR]: {ex.Message}");
            Console.ResetColor();
            Console.WriteLine("\nExecution finished with error. Press any key to exit...");
            Console.ReadKey();
        }
    }

    private class RunnerConfig
    {
        public string DllPath { get; set; } = string.Empty;
        public int CoreThreads { get; set; } = 3;
        public int MaxThreads { get; set; } = 0;
        public int KeepAliveSeconds { get; set; } = 10;
        public string? TargetClass { get; set; }
        public string? TargetMethod { get; set; }
        public string? Category { get; set; }
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
                case "--category":
                    if (value != null) config.Category = value;
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
        
        Console.WriteLine("Usage: CSUnitRunner --dll <path> [--threads <n>] [--max-threads <n>] [--keep-alive <sec>] [--class <name>] [--method <name>] [--category <name>]");
        return null;
    }
}