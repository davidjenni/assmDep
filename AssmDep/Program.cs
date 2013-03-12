namespace AssmDep
{
    using System;

    class Program
    {
        static int Main(string[] args)
        {
            var options = Options.Parse(args);
            if (options == null)
            {
                return 1;
            }
            var program = new Program();
            return program.Run(options);
        }

        int Run(Options options)
        {
            var assemblyReferences = new AssemblyReferences()
            {
                IncludeSystemAssemblies = options.IncludeSystemAssemblies,
            };

            assemblyReferences.Enumerate(options.AssemblyName);

            Console.WriteLine("Inspected {0} assemblies, found {1} references:", assemblyReferences.InspectedReferences, assemblyReferences.ReferencesCount);
            Console.WriteLine();
            foreach (var reference in assemblyReferences.References)
            {
                Console.WriteLine(options.Template, reference);
            }
            return 0;
        }
    }
}
