using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AssmDep
{
    [DebuggerDisplay("{Name}({Version}) - {ReferencedBy.Count}")]
    public class Reference
    {
        public string Name;
        public string Version;
        public string FileVersion;
        public List<string> ReferencedBy = new();
    }

    class ReferenceComparer : IEqualityComparer<Reference>
    {
        public bool Equals(Reference x, Reference y)
        {
            return ReferenceEquals(x, y) ||x is not null && y is not null && x.Name.Equals(y.Name);
        }
        public int GetHashCode(Reference reference) => reference.Name.GetHashCode();
        //public int GetHashCode(Reference reference) => reference.AssemblyName.GetHashCode() ^ reference.ReferencedBy.GetHashCode();
    }

    public class AssemblyReferences
    {
        readonly HashSet<Reference> _assemblyReferences = new(new ReferenceComparer());
        string _basePath;

        public bool IncludeSystemAssemblies { get; set; }
        public int InspectedReferences { get; private set; }
        public int ReferencesCount => _assemblyReferences.Count;

        public Reference Root { get; private set; }

        public IEnumerable<Reference> References => _assemblyReferences.OrderBy((a) => a.Name);

        public bool Enumerate(string assemblyName)
        {
            var assemblyFqn = Path.GetFullPath(assemblyName);
            if (!File.Exists(assemblyFqn))
            {
                return false;
            }
            _basePath = Path.GetDirectoryName(assemblyFqn);
            var assembly = Assembly.LoadFile(assemblyFqn);
            Root = CreateFrom(assembly);
            Enumerate(assembly, Enumerable.Empty<string>());
            return true;
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
            if (IsSystemAssembly(assembly)) return false;

            var candidate = CreateFrom(assembly);
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

        static void RunWithIgnoredFileExceptions(Action act)
        {
            try
            {
                act();
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is FileLoadException) { }
        }

        Assembly TryLoadAssembly(AssemblyName assemblyName)
        {
            Assembly candidate = null;

            RunWithIgnoredFileExceptions(() => {
                candidate = Assembly.Load(assemblyName);
            });

            if (candidate != null)
            {
                return candidate;
            }

            RunWithIgnoredFileExceptions(() => {
                candidate = Assembly.LoadFile(Path.Combine(_basePath, assemblyName.Name + ".dll"));
            });

            return candidate;
        }

        static Reference CreateFrom(Assembly assembly)
        {
            string name = Path.GetFileName(assembly.Location);
            var version = assembly.GetName().Version.ToString();
            string fileVersion = string.Empty;
            RunWithIgnoredFileExceptions(() => {
                fileVersion = (assembly.GetCustomAttribute(typeof(AssemblyFileVersionAttribute)) as AssemblyFileVersionAttribute)?.Version;
            });

            return new Reference { Name = name, Version = version, FileVersion = fileVersion };
        }

        bool IsSystemAssembly(Assembly assembly)
        {
            string name = Path.GetFileName(assembly.Location);
            return (!IncludeSystemAssemblies
                && (name.StartsWith("System.")
                || name.StartsWith("mscorlib")
                || name.StartsWith("netstandard")
                || name.StartsWith("Presentation")
                || name.StartsWith("WindowsBase")
                ));
        }

    }
}
