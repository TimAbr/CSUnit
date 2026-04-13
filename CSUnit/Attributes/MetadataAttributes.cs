using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSUnit.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class DisplayNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class DisabledAttribute(string reason = "None") : Attribute
{
    public string Reason { get; } = reason;
}

[AttributeUsage(AttributeTargets.Method)]
public class TimeoutAttribute(int milliseconds) : Attribute
{
    public int Milliseconds { get; } = milliseconds;
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class CategoryAttribute(string category) : Attribute
{
    public string Category { get; } = category;
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class PriorityAttribute(int priority) : Attribute
{
    public int Priority { get; } = priority;
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class AuthorAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}
