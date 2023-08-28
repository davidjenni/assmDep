using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AssmDep
{
    [DebuggerDisplay("{AssemblyName} - {ReferencedBy.Count}")]
    public class Reference
    {
        public string AssemblyName;
        public List<string> ReferencedBy = new();
    }

    class ReferenceComparer : IEqualityComparer<Reference>
    {
        public bool Equals(Reference x, Reference y)
        {
            return ReferenceEquals(x, y) ||x is not null && y is not null && x.AssemblyName.Equals(y.AssemblyName);
        }
        public int GetHashCode(Reference reference) => reference.AssemblyName.GetHashCode();
        //public int GetHashCode(Reference reference) => reference.AssemblyName.GetHashCode() ^ reference.ReferencedBy.GetHashCode();
    }

    public class AssemblyReferences
    {
        readonly HashSet<Reference> _assemblyReferences = new(new ReferenceComparer());
        string _basePath;

        public bool IncludeSystemAssemblies { get; set; }
        public int InspectedReferences { get; private set; }
        public int ReferencesCount => _assemblyReferences.Count;

        public IEnumerable<Reference> References => _assemblyReferences.OrderBy((a) => a.AssemblyName);

        public void Enumerate(string assemblyName)
        {
            _basePath = Path.GetDirectoryName(Path.GetFullPath(assemblyName));
            Enumerate(Assembly.LoadFile(assemblyName), Enumerable.Empty<string>());
        }

        void Enumerate(Assembly assembly, IEnumerable<string> referencedByAccumulator)
        {
            var callstack = new List<string>(referencedByAccumulator)
            {
                assembly.ManifestModule.Name
            };
            foreach (Assembly assm in GetReferences(assembly))
            {
                InspectedReferences++;
                if (Add(assm, string.Join(";", callstack)))
                {
                    Enumerate(assm, callstack);
                }
            }
        }

        bool Add(Assembly assembly, string referencedBy)
        {
            string name = Path.GetFileName(assembly.Location);
            if (!IncludeSystemAssemblies
                && (name.StartsWith("System.")
                || name.StartsWith("mscorlib")
                || name.StartsWith("Presentation")
                ))
            {
                return false;
            }

            var candidate = new Reference { AssemblyName = name };
            if (_assemblyReferences.TryGetValue(candidate, out var existingEntry))
            {
                existingEntry.ReferencedBy.Add(referencedBy);
                return false;
            }
            else
            {
                candidate.ReferencedBy.Add(referencedBy);
                _assemblyReferences.Add(candidate);
                return true;
            }
        }

        IEnumerable<Assembly> GetReferences(Assembly parentAssembly)
        {
            foreach (var assemblyName in parentAssembly.GetReferencedAssemblies())
            {
                Assembly candidate = TryLoadAssembly(assemblyName);
                if (candidate != null)
                {
                    yield return candidate;
                }
            }
        }

        Assembly TryLoadAssembly(AssemblyName assemblyName)
        {
            Assembly candidate = null;
            try
            {
                candidate = Assembly.Load(assemblyName);
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is FileLoadException) { }

            if (candidate != null)
            {
                return candidate;
            }

            try
            {
                return Assembly.LoadFile(Path.Combine(_basePath, assemblyName.Name + ".dll"));
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is FileLoadException) { }

            return null;
        }
    }
}
