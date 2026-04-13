using System.Reflection;
using CSUnit.Attributes;
using CSUnitRunner.Core.Models;

namespace CSUnitRunner.Core.Preparation;

internal class TreeAnalyzer
{
    public ExecutableNamespaceNode Analyze(NamespaceNode cleanNode)
    {
        var exeNode = new ExecutableNamespaceNode { Name = cleanNode.Name };

        foreach (var rawClass in cleanNode.Classes)
        {
            exeNode.Classes.Add(CreateExecutableClass(rawClass));
        }

        foreach (var subNs in cleanNode.SubNamespaces)
        {
            exeNode.SubNamespaces.Add(Analyze(subNs));
        }

        return exeNode;
    }

    private ExecutableClassNode CreateExecutableClass(ClassNode rawClass)
    {
        var methods = rawClass.Methods;
        var exeClass = new ExecutableClassNode
        {
            Type = rawClass.Type,
            DisplayName = rawClass.Type.GetCustomAttribute<DisplayNameAttribute>()?.Name ?? rawClass.Name,
            BeforeAll = methods.Where(m => m.GetCustomAttribute<BeforeAllAttribute>() != null).ToList(),
            AfterAll = methods.Where(m => m.GetCustomAttribute<AfterAllAttribute>() != null).ToList()
        };

        var beforeEach = methods.Where(m => m.GetCustomAttribute<BeforeEachAttribute>() != null).ToList();
        var afterEach = methods.Where(m => m.GetCustomAttribute<AfterEachAttribute>() != null).ToList();
        
        var testMethods = methods.Where(m => 
            m.GetCustomAttribute<TestAttribute>() != null || 
            m.GetCustomAttribute<ParameterizedTestAttribute>() != null).ToList();

        foreach (var m in testMethods)
        {
            TestCaseExpander.Expand(m, rawClass, exeClass.TestUnits, beforeEach, afterEach);
        }

        return exeClass;
    }
}