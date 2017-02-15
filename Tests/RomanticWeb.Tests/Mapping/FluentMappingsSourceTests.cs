using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using RomanticWeb.Mapping.Sources;
using RomanticWeb.TestEntities.FluentMappings;

namespace RomanticWeb.Tests.Mapping
{
    [TestFixture]
    public class FluentMappingsSourceTests : MappingSourceTests
    {
        protected override IEnumerable<IMappingProviderSource> CreateMappingSources()
        {
            yield return new FluentMappingsSource(typeof(AnimalMap).GetTypeInfo().Assembly, null);
        }
    }
}