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

            if (!refs.Enumerate(assemblyFile.FullName))
            {
                Console.Error.WriteLine($"Cannot find file: {assemblyFile.FullName}.");
                return 2;
            }

            Console.Write("loaded root assembly:");
            PrintReference(refs.Root);
            var inclExcl = refs.IncludeSystemAssemblies ? "incl." : "excl.";
            Console.WriteLine($"Loaded {refs.InspectedReferences} assembly references, found {refs.ReferencesCount} unique references ({inclExcl} system assemblies):");
            Console.WriteLine();
            foreach (var reference in refs.References)
            {
                PrintReference(reference);
                // Console.WriteLine($"{reference.Name}({reference.Version}/{reference.FileVersion}) - {reference.ReferencedBy.Count} refs");
            }

            if (!string.IsNullOrEmpty(filter))
            {
                var filteredRef = refs.References.Where(r => r.Name.StartsWith(filter, StringComparison.OrdinalIgnoreCase));
                Console.WriteLine($"Found {filteredRef.Count()} matches:");
                foreach (var m in filteredRef)
                {
                    Console.WriteLine($" = {m.Name}:");
                    foreach(var refsBy in m.ReferencedBy)
                    {
                        Console.WriteLine($"   - {refsBy}");
                    }
                }
            }
            return 0;
        }

        static void PrintReference(Reference reference)
        {
            Console.WriteLine($"{reference.Name}({reference.Version}/{reference.FileVersion}) - {reference.ReferencedBy.Count} refs");
        }
    }
}
