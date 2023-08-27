using System;
using System.CommandLine;
using System.IO;

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

            var cmdParser = new RootCommand("Enumerate all link-time defined references of a .NET assembly")
            {
                assmOption,
                skipSystemOption
            };

            int exitCode = 99;
            cmdParser.SetHandler((assemblyFile, skipSystem) => exitCode = Enumerate(assemblyFile, skipSystem),
            assmOption, skipSystemOption);
            cmdParser.Invoke(args);
            return exitCode;
        }

        static int Enumerate(FileInfo assemblyFile, bool skipSystem)
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
                Console.WriteLine(reference);
            }
            return 0;
        }
    }
}
