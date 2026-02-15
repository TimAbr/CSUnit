using System.Reflection;
using CSUnitRunner.Core.Models;

namespace CSUnitRunner.Infrastructure.DataSources
{
    internal class MethodsTreeSource
    {
        public NamespaceNode LoadRawTree(Assembly assembly)
        {
            var root = new NamespaceNode { Name = assembly.GetName().Name ?? "Root" };
            var types = GetValidTypes(assembly);

            foreach (var type in types)
            {
                var targetNamespace = GetOrCreateNamespace(root, type.Namespace ?? "Global");

                targetNamespace.Classes.Add(new ClassNode
                {
                    Name = type.Name,
                    Type = type,
                    Methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly).ToList()
                });
            }
            return root;
        }

        private IEnumerable<Type> GetValidTypes(Assembly assembly)
        {
            try { return assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract); }
            catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t != null)!; }
        }

        private NamespaceNode GetOrCreateNamespace(NamespaceNode root, string fullNamespace)
        {
            var current = root;
            foreach (var part in fullNamespace.Split('.'))
            {
                var next = current.SubNamespaces.FirstOrDefault(n => n.Name == part);
                if (next == null)
                {
                    next = new NamespaceNode { Name = part };
                    current.SubNamespaces.Add(next);
                }
                current = next;
            }
            return current;
        }
    }
}