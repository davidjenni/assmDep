namespace AssmDep
{
    class Options
    {
        public bool IncludeSystemAssemblies { get; set; }
        public string AssemblyName { get; set; }
        public string Template { get; set; }

        public static Options Parse(string[] args)
        {
            // TODO add actual parsing, for now, just preset manually
            var options = new Options();

            options.IncludeSystemAssemblies =false ;
            options.AssemblyName = @"d:\IS-Shared\Tele\Main\obj\Win32\Debug\MdsReferences\tools\MdsDataAccessClient.dll";
            //options.AssemblyName = @"d:\IS-Shared\Tele\Main\obj\Win32\Debug\MdsReferences\tools\Storageclientwrapper.dll";
            options.Template = @"<file src=""$MdsZipToolsDir$\{0}"" target=""lib\net40"" />";
            return options;
        }
    }
}
