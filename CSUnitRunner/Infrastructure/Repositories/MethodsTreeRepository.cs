using System.Reflection;
using CSUnitRunner.Core.Models;
using CSUnitRunner.Core.Repositories;
using CSUnitRunner.Infrastructure.DataSources;

namespace CSUnitRunner.Infrastructure.Repositories
{
    public class MethodsTreeRepository : IMethodsTreeRepository
    {
        private readonly MethodsTreeSource _dataSource;

        public MethodsTreeRepository()
        {
            _dataSource = new MethodsTreeSource();
        }

        public NamespaceNode BuildTree(Assembly assembly)
        {
            return _dataSource.LoadRawTree(assembly);
        }

        public NamespaceNode BuildTreeFromFile(string dllPath)
        {
            if (!File.Exists(dllPath))
                throw new FileNotFoundException($"DLL not found at {dllPath}");

            var assembly = Assembly.LoadFrom(dllPath);
            return BuildTree(assembly);
        }
    }
}