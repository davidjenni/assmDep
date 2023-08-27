# Enumerate .NET assemblies

List transient closure of assembly references of a given .NET assembly

## Build and test

```bash
dotnet build
dotnet test
```

Emits executables for both .NET FullFX (net48) and .NET Core (net6):

```bash
bin\Debug\net48\
bin\Debug\net6.0\
```

## Run

```bash
❯ .\bin\Debug\net48\assmDep.exe -h
Description:
  Enumerate all link-time defined references of a .NET assembly

Usage:
  assmDep [options]

Options:
  -a, --assembly <assembly>  .NET assembly to have its link references enumerated
  -s, --skipSystem           Skip system assemblies from enumeration [default: True]
  --version                  Show version information
  -?, -h, --help             Show help and usage information
```
