using System.Linq.Expressions;
using CSUnit.Exceptions;

namespace CSUnit.Assertions;

public static class Assertions
{
    public static void AssertEquals(object expected, object actual)
    {
        if (!Equals(expected, actual))
            throw new AssertionFailedException($"Expected <{expected}>, but was <{actual}>.");
    }

    public static void AssertNotEquals(object unexpected, object actual)
    {
        if (Equals(unexpected, actual))
            throw new AssertionFailedException($"Expected values to be different, but both were <{actual}>.");
    }

    public static void AssertTrue(bool condition)
    {
        if (!condition)
            throw new AssertionFailedException("Expected condition to be true.");
    }

    public static void AssertFalse(bool condition)
    {
        if (condition)
            throw new AssertionFailedException("Expected condition to be false.");
    }

    public static void AssertNull(object? obj)
    {
        if (obj != null)
            throw new AssertionFailedException($"Expected null, but was <{obj}>.");
    }

    public static void AssertNotNull(object? obj)
    {
        if (obj == null)
            throw new AssertionFailedException("Expected non-null object.");
    }

    public static void AssertSame(object expected, object actual)
    {
        if (!ReferenceEquals(expected, actual))
            throw new AssertionFailedException("Expected same instance, but got different references.");
    }

    public static void AssertNotSame(object unexpected, object actual)
    {
        if (ReferenceEquals(unexpected, actual))
            throw new AssertionFailedException("Expected different instances, but got the same reference.");
    }

    public static void AssertThrows<T>(Action action) where T : Exception
    {
        try
        {
            action();
        }
        catch (T)
        {
            return;
        }
        catch (Exception ex)
        {
            throw new AssertionFailedException($"Expected {typeof(T).Name} to be thrown, but {ex.GetType().Name} was thrown.");
        }
        throw new AssertionFailedException($"Expected {typeof(T).Name} to be thrown, but nothing was thrown.");
    }

    public static void AssertTimeout(TimeSpan timeout, Action action)
    {
        var task = Task.Run(action);
        if (!task.Wait(timeout))
        {
            throw new AssertionFailedException($"Execution exceeded timeout of {timeout.TotalMilliseconds}ms.");
        }
    }

    public static void AssertThat(Expression<Func<bool>> expression)
    {
        var func = expression.Compile();
        if (!func())
        {
            var details = ExpressionAnalyzer.Decompose(expression.Body);
            var bodyStr = ExpressionAnalyzer.CleanExpressionString(expression.Body.ToString());
            throw new AssertionFailedException($"Assertion failed: {bodyStr}\nDetailed analysis: {details}");
        }
    }
}