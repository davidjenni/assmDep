namespace AssmDep
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    class AssemblyReferences
    {
        HashSet<string> assemblyReferences = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        string basePath;

        public bool IncludeSystemAssemblies { get; set; }
        public int InspectedReferences { get; private set; }
        public int ReferencesCount
        {
            get { return this.assemblyReferences.Count; }
        }

        public IEnumerable<string> References
        {
            get
            {
                return this.assemblyReferences.OrderBy((a) => a);
            }
        }

        public void Enumerate(string assemblyName)
        {
            this.basePath = Path.GetDirectoryName(Path.GetFullPath(assemblyName));
            Enumerate(Assembly.LoadFile(assemblyName));
        }

        void Enumerate(Assembly assembly)
        {
            foreach (Assembly assm in GetReferences(assembly))
            {
                InspectedReferences++;
                if (Add(assm))
                {
                    Enumerate(assm);
                }
            }
        }

        bool Add(Assembly assembly)
        {
            string name = Path.GetFileName(assembly.Location);
            if (!this.assemblyReferences.Contains(name))
            {
                this.assemblyReferences.Add(name);
                return true;
            }
            return false;
        }

        IEnumerable<Assembly> GetReferences(Assembly parentAssembly)
        {
            foreach (var assemblyName in parentAssembly.GetReferencedAssemblies())
            {
                bool foundInGAC;
                Assembly candidate = TryLoadAssembly(assemblyName, out foundInGAC);
                if (candidate != null)
                {
                    if ((IncludeSystemAssemblies && foundInGAC) || !foundInGAC)
                    {
                        yield return candidate;
                    }
                }
            }
        }

        Assembly TryLoadAssembly(AssemblyName assemblyName, out bool foundInGAC)
        {
            Assembly candidate = null;
            try
            {
                candidate = Assembly.Load(assemblyName);
            }
            catch (FileNotFoundException)
            {
            }
            if (candidate != null)
            {
                foundInGAC = true;
                return candidate;
            }

            try
            {
                candidate = Assembly.LoadFile(Path.Combine(this.basePath, assemblyName.Name + ".dll"));
            }
            catch (FileNotFoundException)
            {
            }

            foundInGAC = false; 
            return candidate;
        }
    }
}
