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

            var executor = new TestExecutor(config.Threads);
            try { Console.Clear(); } catch { }

            using var cts = new CancellationTokenSource();
            var timerTask = Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    try { ConsoleReporter.PrintDynamicReports(executor.Reports); } catch { }
                    await Task.Delay(100, cts.Token);
                }
            }, cts.Token);

            executor.Execute(executableTree);
            
            cts.Cancel();
            try { timerTask.Wait(); } catch { }

            try { ConsoleReporter.PrintDynamicReports(executor.Reports); } 
            catch { ConsoleReporter.PrintReports(executor.Reports); }

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
        public int Threads { get; set; } = Environment.ProcessorCount-1;
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
            switch (args[i])
            {
                case "--dll" when i + 1 < args.Length:
                    config.DllPath = args[++i];
                    break;
                case "--threads" when i + 1 < args.Length:
                {
                    if (int.TryParse(args[++i], out var t) && t >= 1) config.Threads = t;
                    else
                    {
                        Console.WriteLine("Invalid thread count. Must be >= 1.");
                        return null;
                    }
                    break;
                }
                case "--class" when i + 1 < args.Length:
                    config.TargetClass = args[++i];
                    break;
                case "--method" when i + 1 < args.Length:
                    config.TargetMethod = args[++i];
                    break;
                default:
                {
                    if (!args[i].StartsWith("--") && string.IsNullOrEmpty(config.DllPath))
                    {
                        config.DllPath = args[i];
                    }
                    break;
                }
            }
        }

        if (!string.IsNullOrEmpty(config.DllPath)) return config;
        
        Console.WriteLine($"Usage: CSUnitRunner --dll <path_to_dll> [--threads <count> (default: {Environment.ProcessorCount})] [--class <className>] [--method <methodName>]");
        return null;
    }
}