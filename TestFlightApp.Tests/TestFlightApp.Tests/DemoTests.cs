using System;
using System.Collections.Generic;
using System.Threading;
using CSUnit.Attributes;
using CSUnit.Assertions;

namespace TestFlightApp.Tests;

public class DemoTests
{
    [ParameterizedTest]
    [MethodSource(nameof(GetSquareCases))]
    [Category("Math"), Priority(1), Author("Antigravity")]
    public void SquareTest(int input, int expected)
    {
        var actual = input * input;
        Assertions.AssertEquals(expected, actual);
    }

    private static IEnumerable<object[]> GetSquareCases()
    {
        yield return new object[] { 2, 4 };
        yield return new object[] { 3, 9 };
        yield return new object[] { 5, 25 };
    }

    [ParameterizedTest]
    [ValueSource(1, 2, 3, 4, 5, 6, 7, 8, 9, 10)]
    [Category("System")]
    public void ThreadPoolScalingDemo(int id)
    {
        Thread.Sleep(200);
        Assertions.AssertTrue(id > 0);
    }

    [Test]
    [Category("Aesthetics"), Author("DesignTeam")]
    public void ExpressionTreeAssertionDemo()
    {
        int x = 5;
        int y = 5;
        Assertions.AssertThat(() => x + y == 10);
        
        int balance = 100;
        int pendingTransaction = 50;
        int limit = 120;
        
        Assertions.AssertThat(() => balance + pendingTransaction <= limit);
    }
}
