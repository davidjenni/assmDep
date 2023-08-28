using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Xunit;

namespace AssmDep.Test
{
    public class AssmDepTests
    {
        string ThisAssembly => Assembly.GetExecutingAssembly().Location;

        [Fact]
        public void EnumerateTestAssembly_NoSystem()
        {
            var refs = new AssemblyReferences { IncludeSystemAssemblies = false };
            refs.Enumerate(ThisAssembly);

            refs.ReferencesCount.Should().BeGreaterThanOrEqualTo(4);
            refs.InspectedReferences.Should().BeGreaterThanOrEqualTo(30);
            var fluent = FindReference(refs, "FluentAssertions");
            fluent.Should().NotBeNull();
            fluent!.ReferencedBy.Should().NotBeNull();
            fluent.ReferencedBy.Should().Contain("assmDep.Test.dll");

            var mscorlib = FindReference(refs, "mscorlib");
            mscorlib.Should().BeNull();
        }

        [Fact]
        public void EnumerateTestAssembly_IncludeSystem()
        {
            var refs = new AssemblyReferences { IncludeSystemAssemblies = true };
            refs.Enumerate(ThisAssembly);

            refs.ReferencesCount.Should().BeGreaterThanOrEqualTo(70);
            refs.InspectedReferences.Should().BeGreaterThan(70);
            var fluent = FindReference(refs, "FluentAssertions");
            fluent.Should().NotBeNull();
            fluent!.ReferencedBy.Should().NotBeNull();
            fluent.ReferencedBy.Should().Contain("assmDep.Test.dll");

#if NETFRAMEWORK
            var mscorlib = FindReference(refs, "mscorlib");
            mscorlib.Should().NotBeNull();
            mscorlib!.ReferencedBy.Should().NotBeNull();
            mscorlib.ReferencedBy.Count().Should().BeGreaterThan(70);
#else
            var netstd = FindReference(refs, "netstandard");
            netstd.Should().NotBeNull();
            netstd!.ReferencedBy.Should().NotBeNull();
            netstd.ReferencedBy.Count().Should().BeGreaterThanOrEqualTo(3);
#endif
        }

        Reference? FindReference(AssemblyReferences refs, string name) => refs
            .References
            .Where(r => r.AssemblyName.StartsWith(name, StringComparison.InvariantCultureIgnoreCase))
            .FirstOrDefault();
    }
}

