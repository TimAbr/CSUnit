using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSUnit.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class TestAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Method)]
public class BeforeEachAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Method)]
public class AfterEachAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Method)]
public class BeforeAllAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Method)]
public class AfterAllAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Method)]
public class ParameterizedTestAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class MethodSourceAttribute(string methodName) : Attribute
{
    public string MethodName { get; } = methodName;
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class ValueSourceAttribute : Attribute
{
    public object[]? Values { get; }
    
    public ValueSourceAttribute(params object[] values)
    {
        Values = values;
    }

    public int[]? Ints { get; init; }
    public string[]? Strings { get; init; }
}
