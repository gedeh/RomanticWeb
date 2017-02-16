using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using RomanticWeb.ComponentModel;
using RomanticWeb.Converters;
using RomanticWeb.Diagnostics;
using RomanticWeb.Dynamic;
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
        private IServiceContainer _container;

        internal MappingsRepository MappingsRepository { get { return _mappingsRepository; } }

        [SetUp]
        public void Setup()
        {
            _ontologies = new Mock<IOntologyProvider>();
            _ontologies.Setup(o => o.ResolveUri(It.IsAny<string>(), It.IsAny<string>()))
                       .Returns((string p, string t) => GetUri(p, t));
            _container = new ServiceContainer();
            _container.RegisterFrom<ConventionsCompositionRoot>();
            _container.RegisterFrom<MappingComposition>();
            _container.RegisterFrom<AttributesMappingComposition>();
            _container.RegisterFrom<FluentMappingComposition>();
            _container.RegisterFrom<InternalComponentsCompositionRoot>();
            _container.RegisterInstance(_ontologies.Object);
            var logger = new Mock<ILogger>(MockBehavior.Strict);
            logger.Setup(instance => instance.Log(It.IsAny<LogLevel>(), It.IsAny<string>(), It.IsAny<object[]>()));
            logger.Setup(instance => instance.Log(It.IsAny<LogLevel>(), It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()));
            _container.RegisterInstance(logger.Object);
            foreach (var mappingProviderSource in CreateMappingSources())
            {
                _container.RegisterInstance(mappingProviderSource, mappingProviderSource.GetType().FullName);
            }

            var conventions = _container.GetInstance<IEnumerable<IConvention>>();
            var mappingModelBuilder = new MappingModelBuilder(new MappingContext(_ontologies.Object, conventions), new ConverterCatalog());
            _container.RegisterInstance(mappingModelBuilder);
            _mappingsRepository = (MappingsRepository)_container.GetInstance<IMappingsRepository>();
        }

        protected Type GetDynamicType(string name)
        {
            var result = Type.GetType(name);
            if (result != null)
            {
                return result;
            }

            var emitHelper = _container.GetInstance<EmitHelper>();
            return emitHelper.GetDynamicModule().Assembly.GetType(name.Split(',')[0]);
        }

        protected abstract IEnumerable<IMappingProviderSource> CreateMappingSources();

        private static Uri GetUri(string prefix, string term)
        {
            return new Uri("http://example/livingThings#" + term);
        }
    }
}