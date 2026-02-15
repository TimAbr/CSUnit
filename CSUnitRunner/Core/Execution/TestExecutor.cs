using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using CSUnit.Exceptions;
using CSUnitRunner.Core.Models;

namespace CSUnitRunner.Core.Execution
{
    internal class TestExecutor
    {
        public List<ClassTestReport> Execute(ExecutableNamespaceNode node)
        {
            var reports = new List<ClassTestReport>();
            ExecuteRecursive(node, reports);
            return reports;
        }

        private void ExecuteRecursive(ExecutableNamespaceNode node, List<ClassTestReport> reports)
        {
            foreach (var exeClass in node.Classes)
            {
                reports.Add(ExecuteClass(exeClass));
            }

            foreach (var subNs in node.SubNamespaces)
            {
                ExecuteRecursive(subNs, reports);
            }
        }

        private ClassTestReport ExecuteClass(ExecutableClassNode exeClass)
        {
            var report = new ClassTestReport { ClassName = exeClass.DisplayName };

            try
            {
                foreach (var m in exeClass.BeforeAll) m.Invoke(null, null);

                foreach (var unit in exeClass.TestUnits)
                {
                    report.Results.Add(ExecuteTestUnit(exeClass.Type, unit));
                }

                foreach (var m in exeClass.AfterAll) m.Invoke(null, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Critical Error] Context failure in {exeClass.DisplayName}: {ex.Message}");
            }

            return report;
        }

        private TestResult ExecuteTestUnit(Type classType, TestUnit unit)
        {
            var result = new TestResult { Name = unit.DisplayName };
            var sw = Stopwatch.StartNew();

            try
            {
                object instance = Activator.CreateInstance(classType)!;

                foreach (var m in unit.BeforeEach) InvokeMethod(m, instance);

                InvokeMethod(unit.TestMethod, instance);

                foreach (var m in unit.AfterEach) InvokeMethod(m, instance);

                result.Status = TestStatus.Passed;
            }
            catch (TargetInvocationException ex)
            {
                var inner = ex.InnerException;
                if (inner is AssertionFailedException)
                {
                    result.Status = TestStatus.Failed;
                    result.ErrorMessage = inner.Message;
                }
                else
                {
                    result.Status = TestStatus.Error;
                    result.ErrorMessage = inner?.Message ?? ex.Message;
                    result.StackTrace = inner?.StackTrace;
                }
            }
            catch (Exception ex)
            {
                result.Status = TestStatus.Error;
                result.ErrorMessage = ex.Message;
            }
            finally
            {
                sw.Stop();
                result.Duration = sw.Elapsed;
            }

            return result;
        }

        private void InvokeMethod(MethodInfo method, object instance)
        {
            var result = method.Invoke(instance, null);

            if (result is Task task)
            {
                task.GetAwaiter().GetResult();
            }
        }
    }
}