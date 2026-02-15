using CSUnitRunner.Core.Models;
using System.Reflection;

namespace CSUnitRunner.Core.Repositories
{
    public interface IMethodsTreeRepository
    {
        NamespaceNode BuildTree(Assembly assembly);
        NamespaceNode BuildTreeFromFile(string dllPath);
    }
}