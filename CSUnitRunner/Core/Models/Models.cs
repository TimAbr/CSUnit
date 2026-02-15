using System.Reflection;

namespace CSUnitRunner.Core.Models
{

    public class ClassNode
    {
        public string Name { get; set; } = string.Empty;
        public Type Type { get; set; } = null!;
        public List<MethodInfo> Methods { get; set; } = new();
    }

    public class NamespaceNode
    {
        public string Name { get; set; } = string.Empty;
        public List<NamespaceNode> SubNamespaces { get; set; } = new();
        public List<ClassNode> Classes { get; set; } = new();
    }

    public class TestUnit
    {
        public MethodInfo TestMethod { get; set; } = null!;
        public string DisplayName { get; set; } = string.Empty;
        public List<MethodInfo> BeforeEach { get; set; } = new();
        public List<MethodInfo> AfterEach { get; set; } = new();
    }

    public class ExecutableClassNode
    {
        public Type Type { get; set; } = null!;
        public string DisplayName { get; set; } = string.Empty;
        public List<MethodInfo> BeforeAll { get; set; } = new();
        public List<TestUnit> TestUnits { get; set; } = new();
        public List<MethodInfo> AfterAll { get; set; } = new();

        public bool HasSharedContext { get; set; }
    }

    public class ExecutableNamespaceNode
    {
        public string Name { get; set; } = string.Empty;
        public List<ExecutableNamespaceNode> SubNamespaces { get; set; } = new();
        public List<ExecutableClassNode> Classes { get; set; } = new();
    }
}