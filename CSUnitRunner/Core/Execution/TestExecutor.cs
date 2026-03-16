using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CSUnit.Exceptions;
using CSUnitRunner.Core.Models;

namespace CSUnitRunner.Core.Execution;

internal class TestExecutor
{
    private readonly int _maxThreads;
    private readonly object _reportsLock = new();
    public List<ClassTestReport> Reports { get; } = new();

    public TestExecutor(int maxThreads)
    {
        _maxThreads = maxThreads;
    }

    public void Execute(ExecutableNamespaceNode node)
    {
        var allClasses = new List<ExecutableClassNode>();
        CollectClasses(node, allClasses);

        using var semaphore = new SemaphoreSlim(_maxThreads);

        var classTasks = allClasses.Select(async exeClass =>
        {
            var report = new ClassTestReport { ClassName = exeClass.DisplayName };
            lock (_reportsLock)
            {
                Reports.Add(report);
            }

            try
            {
                foreach (var m in exeClass.BeforeAll) m.Invoke(null, null);

                var tasks = exeClass.TestUnits.Select(async unit =>
                {
                    var result = new TestResult { Name = unit.DisplayName, Status = TestStatus.Pending, StartTime = null };
                    lock (report.SyncLock)
                    {
                        report.Results.Add(result);
                    }

                    await semaphore.WaitAsync();

                    lock (report.SyncLock)
                    {
                        result.Status = TestStatus.Running;
                        result.StartTime = DateTime.Now;
                    }

                    TestResult actualResult;
                    try
                    {
                        var testTask = Task.Run(() => ExecuteTestUnit(exeClass.Type, unit));
                        if (unit.TimeoutMs.HasValue)
                        {
                            var timeoutTask = Task.Delay(unit.TimeoutMs.Value);
                            var completedTask = await Task.WhenAny(testTask, timeoutTask);
                            if (completedTask == timeoutTask)
                            {
                                actualResult = new TestResult
                                {
                                    Name = unit.DisplayName,
                                    Status = TestStatus.Failed,
                                    ErrorMessage = $"Test exceeded timeout of {unit.TimeoutMs.Value}ms",
                                    StartTime = result.StartTime,
                                    Duration = TimeSpan.FromMilliseconds(unit.TimeoutMs.Value)
                                };
                            }
                            else
                            {
                                actualResult = await testTask;
                            }
                        }
                        else
                        {
                            actualResult = await testTask;
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                    }

                    lock (report.SyncLock)
                    {
                        var index = report.Results.IndexOf(result);
                        if (index != -1) report.Results[index] = actualResult;
                    }
                }).ToArray();

                await Task.WhenAll(tasks);

                foreach (var m in exeClass.AfterAll) m.Invoke(null, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Critical Error] Context failure in {exeClass.DisplayName}: {ex.Message}");
            }
        }).ToArray();

        Task.WaitAll(classTasks);
    }

    private void CollectClasses(ExecutableNamespaceNode node, List<ExecutableClassNode> list)
    {
        list.AddRange(node.Classes);
        foreach (var subNs in node.SubNamespaces)
        {
            CollectClasses(subNs, list);
        }
    }

    private TestResult ExecuteTestUnit(Type classType, TestUnit unit)
    {
        var result = new TestResult { Name = unit.DisplayName, StartTime = DateTime.Now };
        var sw = Stopwatch.StartNew();

        try
        {
            var instance = Activator.CreateInstance(classType)!;

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