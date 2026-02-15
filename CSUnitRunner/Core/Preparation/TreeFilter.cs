using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CSUnit.Attributes;
using CSUnitRunner.Core.Models;

namespace CSUnitRunner.Core.Preparation
{
    internal class TreeFilter
    {
        private readonly string? _targetClass;
        private readonly string? _targetMethod;

        public TreeFilter(string? targetClass = null, string? targetMethod = null)
        {
            _targetClass = targetClass;
            _targetMethod = targetMethod;
        }

        public NamespaceNode? Filter(NamespaceNode? node)
        {
            if (node == null) return null;

 
            RemoveDisabled(node);

            ApplyCliFilters(node);

            return CleanupEmptyNodes(node);
        }

        private void RemoveDisabled(NamespaceNode node)
        {
            node.Classes = node.Classes
                .Where(c => c.Type.GetCustomAttribute<DisabledAttribute>() == null)
                .ToList();

            foreach (var @class in node.Classes)
            {
                @class.Methods = @class.Methods
                    .Where(m => m.GetCustomAttribute<DisabledAttribute>() == null)
                    .ToList();
            }

            foreach (var subNs in node.SubNamespaces)
            {
                RemoveDisabled(subNs);
            }
        }

        private void ApplyCliFilters(NamespaceNode node)
        {
            if (string.IsNullOrEmpty(_targetClass)) return;

            node.Classes = node.Classes
                .Where(c => c.Name.Equals(_targetClass, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!string.IsNullOrEmpty(_targetMethod))
            {
                foreach (var @class in node.Classes)
                {
                    @class.Methods = @class.Methods
                        .Where(m => m.Name.Equals(_targetMethod, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }
            }

            foreach (var subNs in node.SubNamespaces)
            {
                ApplyCliFilters(subNs);
            }
        }

        private NamespaceNode? CleanupEmptyNodes(NamespaceNode node)
        {

            var filteredSubNamespaces = new List<NamespaceNode>();
            foreach (var subNs in node.SubNamespaces)
            {
                var cleaned = CleanupEmptyNodes(subNs);
                if (cleaned != null) filteredSubNamespaces.Add(cleaned);
            }
            node.SubNamespaces = filteredSubNamespaces;

            node.Classes = node.Classes
                .Where(c => c.Methods.Any(m => m.GetCustomAttribute<TestAttribute>() != null))
                .ToList();

            if (!node.Classes.Any() && !node.SubNamespaces.Any())
            {
                return null;
            }

            return node;
        }
    }
}