using CSUnitRunner.Core.Execution;
using CSUnitRunner.Core.Models;
using CSUnitRunner.Core.Preparation;
using CSUnitRunner.Infrastructure.Repositories;
using CSUnitRunner.Presentation;

namespace CSUnitRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("No arguments provided");
                Console.WriteLine("Usage: CSUnitRunner <path_to_dll> [className] [methodName]");
                return;
            }

            string dllPath = args[0];
            string? targetClass = args.Length > 1 ? args[1] : null;
            string? targetMethod = args.Length > 2 ? args[2] : null;

            try
            {
                var repository = new MethodsTreeRepository();
                NamespaceNode rawTree = repository.BuildTreeFromFile(dllPath);

                var filter = new TreeFilter();
                NamespaceNode? filteredTree = filter.Filter(rawTree);

                if (filteredTree == null)
                {
                    Console.WriteLine("No active tests found in the assembly.");
                    return;
                }

                var analyzer = new TreeAnalyzer();
                ExecutableNamespaceNode executableTree = analyzer.Analyze(filteredTree);

                var executor = new TestExecutor();
                List<ClassTestReport> reports = executor.Execute(executableTree);

                ConsoleReporter.PrintReports(reports);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[FATAL ERROR]: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}