using System.Reflection;
using CSUnit.Attributes;
using CSUnitRunner.Core.Models;

namespace CSUnitRunner.Core.Preparation;

internal static class TestCaseExpander
{
    public static void Expand(MethodInfo method, ClassNode rawClass, List<TestUnit> units, List<MethodInfo> beforeEach, List<MethodInfo> afterEach)
    {
        var paramAttr = method.GetCustomAttribute<ParameterizedTestAttribute>();
        if (paramAttr == null)
        {
            units.Add(CreateUnit(method, beforeEach, afterEach));
            return;
        }

        ExpandFromMethodSource(method, rawClass, units, beforeEach, afterEach);
        ExpandFromValueSource(method, units, beforeEach, afterEach);
    }

    private static void ExpandFromMethodSource(MethodInfo method, ClassNode rawClass, List<TestUnit> units, List<MethodInfo> beforeEach, List<MethodInfo> afterEach)
    {
        var methodSources = method.GetCustomAttributes<MethodSourceAttribute>();
        foreach (var source in methodSources)
        {
            var sourceMethod = rawClass.Type.GetMethod(source.MethodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            if (sourceMethod == null) continue;

            if (sourceMethod.Invoke(null, null) is System.Collections.IEnumerable data)
            {
                foreach (var item in data)
                {
                    var args = item as object[] ?? new object[] { item! };
                    units.Add(CreateUnit(method, beforeEach, afterEach, args, $"{method.Name}({string.Join(", ", args)})"));
                }
            }
        }
    }

    private static void ExpandFromValueSource(MethodInfo method, List<TestUnit> units, List<MethodInfo> beforeEach, List<MethodInfo> afterEach)
    {
        var valueSources = method.GetCustomAttributes<ValueSourceAttribute>();
        foreach (var vs in valueSources)
        {
            IEnumerable<object>? vals = vs.Values ?? vs.Ints?.Cast<object>() ?? vs.Strings?.Cast<object>();
            if (vals == null) continue;

            foreach (var val in vals)
            {
                units.Add(CreateUnit(method, beforeEach, afterEach, new[] { val }, $"{method.Name}({val})"));
            }
        }
    }

    private static TestUnit CreateUnit(MethodInfo method, List<MethodInfo> beforeEach, List<MethodInfo> afterEach, object[]? args = null, string? displayName = null)
    {
        return new TestUnit
        {
            TestMethod = method,
            DisplayName = displayName ?? method.GetCustomAttribute<DisplayNameAttribute>()?.Name ?? method.Name,
            Arguments = args,
            BeforeEach = beforeEach,
            AfterEach = afterEach,
            TimeoutMs = method.GetCustomAttribute<TimeoutAttribute>()?.Milliseconds,
            Priority = method.GetCustomAttribute<PriorityAttribute>()?.Priority ?? 2
        };
    }
}
