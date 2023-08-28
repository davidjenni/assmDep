using System;
using System.CommandLine;
using System.IO;
using System.Linq;

namespace AssmDep
{
    internal class Program
    {
        static int Main(string[] args)
        {
            var assmOption = new Option<FileInfo>(
                aliases: new[] { "--assembly", "-a" },
                description: ".NET assembly to have its link references enumerated"
                );
            var skipSystemOption = new Option<bool>(
                aliases: new[] { "--skipSystem", "-s" },
                description: "Skip system assemblies from enumeration",
                getDefaultValue: () => true);
            var filterOption = new Option<string>(
                aliases: new[] { "--filter", "-f" },
                description: "Filter by this assembly name and emit its referencedBy list"
                );

            var cmdParser = new RootCommand("Enumerate all link-time defined references of a .NET assembly")
            {
                assmOption,
                skipSystemOption,
                filterOption
            };

            int exitCode = 99;
            cmdParser.SetHandler((assemblyFile, skipSystem, filter) => exitCode = Enumerate(assemblyFile, skipSystem, filter),
            assmOption, skipSystemOption, filterOption);
            cmdParser.Invoke(args);
            return exitCode;
        }

        static int Enumerate(FileInfo assemblyFile, bool skipSystem, string filter)
        {
            if (assemblyFile == null)
            {
                Console.WriteLine("--assembly parameter is required!");
                return 1;
            }

            var refs = new AssemblyReferences
            { IncludeSystemAssemblies = !skipSystem };

            refs.Enumerate(assemblyFile.FullName);

            Console.WriteLine("Inspected {0} assemblies, found {1} references:", refs.InspectedReferences, refs.ReferencesCount);
            Console.WriteLine();
            foreach (var reference in refs.References)
            {
                Console.WriteLine($"{reference.AssemblyName} - {reference.ReferencedBy.Count} refs");
            }

            if (!string.IsNullOrEmpty(filter))
            {
                var filteredRef = refs.References.Where(r => r.AssemblyName.StartsWith(filter, StringComparison.OrdinalIgnoreCase));
                Console.WriteLine($"Found {filteredRef.Count()} matches:");
                foreach (var m in filteredRef)
                {
                    Console.WriteLine($" = {m.AssemblyName}:");
                    foreach(var refsBy in m.ReferencedBy)
                    {
                        Console.WriteLine($"   - {refsBy}");
                    }
                }
            }
            return 0;
        }
    }
}
