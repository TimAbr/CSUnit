using System;
using System.Collections.Generic;

namespace CSUnitRunner.Core.Models;

public enum TestStatus { Pending, Running, Passed, Failed, Error }

public class TestResult
{
    public string Name { get; set; } = string.Empty;
    public TestStatus Status { get; set; } = TestStatus.Pending;
    public string? ErrorMessage { get; set; }
    public string? StackTrace { get; set; }
    public TimeSpan Duration { get; set; }
    public DateTime? StartTime { get; set; }
}

public class ClassTestReport
{
    public string ClassName { get; set; } = string.Empty;
    public List<TestResult> Results { get; } = new();
    public object SyncLock { get; } = new();
}