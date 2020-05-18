﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Dependencies.Viewer.Wpf.Controls.Base;
using Dependencies.Viewer.Wpf.Controls.Models;

namespace Dependencies.Viewer.Wpf.Controls.Extensions
{
    public static class ReferenceModelExtensions
    {
        public static string ToDisplayString(this ReferenceModel reference, Func<AssemblyModel, string> nameProvider)
        {
            if (!reference.LoadedAssembly.IsResolved && !string.IsNullOrEmpty(reference.AssemblyVersion))
                return $"{nameProvider(reference.LoadedAssembly)} (v{reference.AssemblyVersion})";

            if (!reference.LoadedAssembly.IsResolved)
                return nameProvider(reference.LoadedAssembly);

            if (reference.AssemblyVersion != reference.LoadedAssembly.Version)
                return $"{nameProvider(reference.LoadedAssembly)}   (v{ reference.AssemblyVersion } ➜ v{ reference.LoadedAssembly.Version})";

            if (reference.LoadedAssembly.IsNative)
                return $"{nameProvider(reference.LoadedAssembly)}   (loaded v{ reference.LoadedAssembly.Version })";

            return nameProvider(reference.LoadedAssembly);
        }

        public static string ToDisplayString(this ReferenceModel reference) => reference.ToDisplayString(x => x.FullName);

        public static FilterCollection<AssemblyTreeModel> ToFilterModels(this IEnumerable<ReferenceModel> references, Predicate<object> predicate)
        {
            var transform = new ObjectCacheTransformer();

            var models = references.Select(x => x.ToFilterModel(predicate, transform));

            return new FilterCollection<AssemblyTreeModel>(models, predicate, nameof(AssemblyTreeModel.AssemblyFullName));
        }

        internal static AssemblyTreeModel ToFilterModel(this ReferenceModel baseItem, Predicate<object> predicate, ObjectCacheTransformer transformer)
        {
            return transformer.Transform(baseItem, x => new AssemblyTreeModel(baseItem)
            {
                Collection = x.LoadedAssembly.References.ToFilterModels(predicate)
            });
        }

        public static IEnumerable<AssemblyPath> GetAssemblyParentPath(this ReferenceModel reference, AssemblyModel assemblyRoot) => GetAssemblyParentPath(reference, assemblyRoot, null);

        public static IEnumerable<AssemblyPath> GetAssemblyParentPath(this ReferenceModel reference, AssemblyModel assemblyRoot, AssemblyPath currentPath)
        {
            var path = new AssemblyPath { Assembly = assemblyRoot };

            if (currentPath != null)
                path.Parents.Add(currentPath);

            if (assemblyRoot.ReferencedAssemblyNames.Contains(reference.AssemblyFullName))
                yield return path;

            var subPaths = assemblyRoot.References.SelectMany(x => reference.GetAssemblyParentPath(x.LoadedAssembly, path));

            foreach (var item in subPaths)
                yield return item;
        }

        public static AssemblyModel IsolatedShadowClone(this AssemblyModel assembly)
        {
            var referencedAssemblies = assembly.GetAllReferencedAssemblyNames().Distinct();

            var limitedReferencesProvider = referencedAssemblies.Select(x => assembly.ReferenceProvider[x]).ToDictionary(x => x.AssemblyFullName, x => x.ShadowClone());

            foreach (var item in limitedReferencesProvider)
                item.Value.LoadedAssembly = item.Value.LoadedAssembly.ShadowClone(limitedReferencesProvider);

            return limitedReferencesProvider[assembly.FullName].LoadedAssembly;
        }

        public static IEnumerable<string> GetAllReferencedAssemblyNames(this AssemblyModel assembly)
        {
            yield return assembly.FullName;

            foreach (var item in assembly.References.SelectMany(x => GetAllReferencedAssemblyNames(x.LoadedAssembly)))
                yield return item;
        }
    }
}
