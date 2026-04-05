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
    private readonly CustomThreadPool _pool;
    private readonly object _reportsLock = new();
    public List<ClassTestReport> Reports { get; } = new();

    public TestExecutor(CustomThreadPool pool)
    {
        _pool = pool;
    }

    public void Execute(ExecutableNamespaceNode node)
    {
        var allClasses = new List<ExecutableClassNode>();
        CollectClasses(node, allClasses);

        if (allClasses.Count == 0) return;

        int totalUnits = allClasses.Sum(c => c.TestUnits.Count);
        if (totalUnits == 0)
        {
            foreach (var c in allClasses)
            {
                try
                {
                    foreach (var m in c.BeforeAll) m.Invoke(null, null);
                    foreach (var m in c.AfterAll) m.Invoke(null, null);
                }
                catch { }
            }
            return;
        }

        using var overallFinished = new CountdownEvent(totalUnits);

        foreach (var exeClass in allClasses)
        {
            var currentClass = exeClass;
            var report = new ClassTestReport { ClassName = currentClass.DisplayName };
            lock (_reportsLock)
            {
                Reports.Add(report);
            }

            try
            {
                // Run BeforeAll in the main thread (doesn't block the pool)
                foreach (var m in currentClass.BeforeAll) m.Invoke(null, null);

                if (currentClass.TestUnits.Count > 0)
                {
                    int unitsRemainingInClass = currentClass.TestUnits.Count;

                    foreach (var unit in currentClass.TestUnits)
                    {
                        var currentUnit = unit;
                        var result = new TestResult { Name = currentUnit.DisplayName, Status = TestStatus.Pending, StartTime = null };
                        lock (report.SyncLock)
                        {
                            report.Results.Add(result);
                        }

                        _pool.Enqueue(() =>
                        {
                            try
                            {
                                lock (report.SyncLock)
                                {
                                    result.Status = TestStatus.Running;
                                    result.StartTime = DateTime.Now;
                                }

                                TestResult actualResult;
                                if (currentUnit.TimeoutMs.HasValue)
                                {
                                    TestResult? innerResult = null;
                                    using var testDone = new ManualResetEvent(false);

                                    _pool.Enqueue(() =>
                                    {
                                        try
                                        {
                                            innerResult = ExecuteTestUnit(currentClass.Type, currentUnit);
                                        }
                                        finally
                                        {
                                            testDone.Set();
                                        }
                                    });

                                    if (testDone.WaitOne(currentUnit.TimeoutMs.Value))
                                    {
                                        actualResult = innerResult!;
                                    }
                                    else
                                    {
                                        actualResult = new TestResult
                                        {
                                            Name = currentUnit.DisplayName,
                                            Status = TestStatus.Failed,
                                            ErrorMessage = $"Test exceeded timeout of {currentUnit.TimeoutMs.Value}ms",
                                            StartTime = result.StartTime,
                                            Duration = TimeSpan.FromMilliseconds(currentUnit.TimeoutMs.Value)
                                        };
                                    }
                                }
                                else
                                {
                                    actualResult = ExecuteTestUnit(currentClass.Type, currentUnit);
                                }

                                lock (report.SyncLock)
                                {
                                    var index = report.Results.IndexOf(result);
                                    if (index != -1) report.Results[index] = actualResult;
                                }
                            }
                            finally
                            {
                                if (Interlocked.Decrement(ref unitsRemainingInClass) == 0)
                                {
                                    try
                                    {
                                        foreach (var m in currentClass.AfterAll) m.Invoke(null, null);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"[Error] AfterAll failure: {ex.Message}");
                                    }
                                }
                                overallFinished.Signal();
                            }
                        });
                    }
                }
                else
                {
                    foreach (var m in currentClass.AfterAll) m.Invoke(null, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Critical Error] Setup failure in {currentClass.DisplayName}: {ex.Message}");
            }
        }

        overallFinished.Wait();
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