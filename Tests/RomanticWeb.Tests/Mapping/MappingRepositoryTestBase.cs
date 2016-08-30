using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using RomanticWeb.ComponentModel;
using RomanticWeb.Converters;
using RomanticWeb.Diagnostics;
using RomanticWeb.LightInject;
using RomanticWeb.Mapping;
using RomanticWeb.Mapping.Conventions;
using RomanticWeb.Mapping.Model;
using RomanticWeb.Mapping.Sources;
using RomanticWeb.Ontologies;

namespace RomanticWeb.Tests.Mapping
{
    public abstract class MappingRepositoryTestBase
    {
        private Mock<IOntologyProvider> _ontologies;
        private MappingsRepository _mappingsRepository;

        internal MappingsRepository MappingsRepository
        {
            get
            {
                return _mappingsRepository;
            }
        }

        [SetUp]
        public void Setup()
        {
            _ontologies = new Mock<IOntologyProvider>();
            _ontologies.Setup(o => o.ResolveUri(It.IsAny<string>(), It.IsAny<string>()))
                       .Returns((string p, string t) => GetUri(p, t));
            IServiceContainer container = new ServiceContainer();
            container.RegisterFrom<ConventionsCompositionRoot>();
            container.RegisterFrom<MappingComposition>();
            container.RegisterFrom<AttributesMappingComposition>();
            container.RegisterFrom<FluentMappingComposition>();
            container.RegisterFrom<InternalComponentsCompositionRoot>();
            container.RegisterInstance(_ontologies.Object);
            var logger = new Mock<ILogger>(MockBehavior.Strict);
            logger.Setup(instance => instance.Log(It.IsAny<LogLevel>(), It.IsAny<string>(), It.IsAny<object[]>()));
            logger.Setup(instance => instance.Log(It.IsAny<LogLevel>(), It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()));
            container.RegisterInstance(logger.Object);
            foreach (var mappingProviderSource in CreateMappingSources())
            {
                container.RegisterInstance(mappingProviderSource, mappingProviderSource.GetType().FullName);
            }

            var conventions = container.GetInstance<IEnumerable<IConvention>>();
            var mappingModelBuilder = new MappingModelBuilder(new MappingContext(_ontologies.Object, conventions), new ConverterCatalog());
            container.RegisterInstance(mappingModelBuilder);
            _mappingsRepository = (MappingsRepository)container.GetInstance<IMappingsRepository>();
        }

        protected abstract IEnumerable<IMappingProviderSource> CreateMappingSources();

        private static Uri GetUri(string prefix, string term)
        {
            return new Uri("http://example/livingThings#" + term);
        }
    }
}