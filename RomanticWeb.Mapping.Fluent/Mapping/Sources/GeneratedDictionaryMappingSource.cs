using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Remotion.Linq;
using RomanticWeb.Collections;
using RomanticWeb.Collections.Mapping;
using RomanticWeb.Diagnostics;
using RomanticWeb.Dynamic;
using RomanticWeb.Mapping.Conventions;
using RomanticWeb.Mapping.Fluent;
using RomanticWeb.Mapping.Providers;
using RomanticWeb.Mapping.Visitors;
using RomanticWeb.Ontologies;

namespace RomanticWeb.Mapping.Sources
{
    internal class GeneratedDictionaryMappingSource : IMappingProviderVisitor, IMappingProviderSource
    {
        private static readonly Type[] RequiredVisitors = { typeof(ConventionsVisitor) };
        private readonly List<EntityMap> _entityMaps = new List<EntityMap>();
        private readonly IFluentMapsVisitor _visitor;
        private readonly IOntologyProvider _ontologyProvider;
        private readonly EmitHelper _emitHelper;

        public GeneratedDictionaryMappingSource(MappingContext mappingContext, EmitHelper emitHelper, ILogger log)
        {
            _visitor = new FluentMappingProviderBuilder(log);
            _emitHelper = emitHelper;
            _ontologyProvider = mappingContext.OntologyProvider;
        }

        public string Description { get { return "Dictionary mappings"; } }

        public IEnumerable<Type> Requires { get { return RequiredVisitors; } }

        public IEnumerable<IEntityMappingProvider> GetMappingProviders()
        {
            return from map in _entityMaps
                   select map.Accept(_visitor);
        }

        public void Visit(ICollectionMappingProvider collectionMappingProvider)
        {
        }

        public void Visit(IPropertyMappingProvider propertyMappingProvider)
        {
        }

        public void Visit(IDictionaryMappingProvider dictionaryMappingProvider)
        {
            _entityMaps.Add(CreateDictionaryOwnerMapping(dictionaryMappingProvider));
            _entityMaps.Add(CreateDictionaryEntryMapping(dictionaryMappingProvider));
        }

        public void Visit(IClassMappingProvider classMappingProvider)
        {
        }

        public void Visit(IEntityMappingProvider entityMappingProvider)
        {
        }

        private EntityMap CreateDictionaryOwnerMapping(IDictionaryMappingProvider map)
        {
            // todo: refactoring
            var entry = GetOrCreateDictionaryEntryType(map.PropertyInfo);
            var owner = GetOrCreateDictionaryEntryType(map.PropertyInfo, true);
            var type = typeof(DictionaryOwnerMap<,,,>);
            var typeArguments = new[] { owner, entry }.Concat(map.PropertyInfo.PropertyType.GenericTypeArguments).ToArray();
            var ownerMapType = type.MakeGenericType(typeArguments);

            var defineDynamicModule = _emitHelper.GetDynamicModule();
            Type mapType = null;
            lock (defineDynamicModule)
            {
                mapType = defineDynamicModule.GetOrEmitType(owner.Name + "Map", builder => EmitOwnerMap(map, builder, owner, ownerMapType));
            }

            return (EntityMap)Activator.CreateInstance(mapType);
        }

        private TypeBuilder EmitOwnerMap(IDictionaryMappingProvider map, ModuleBuilder defineDynamicModule, Type owner, Type ownerMapType)
        {
            var typeBuilderHelper = defineDynamicModule.DefineType(owner.Name + "Map", TypeAttributes.Public, ownerMapType);
            var methodBuilderHelper = typeBuilderHelper.DefineMethod(
                "SetupEntriesCollection",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig,
                typeof(void),
                new[] { typeof(ITermPart<ICollectionMap>) });

            var ilGenerator = methodBuilderHelper.GetILGenerator();
            ilGenerator.Emit(OpCodes.Nop);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldstr, map.GetTerm(_ontologyProvider).ToString());
            ilGenerator.Emit(OpCodes.Newobj, typeof(Uri).GetConstructor(new[] { typeof(string) }));
            ilGenerator.Emit(OpCodes.Callvirt, typeof(ITermPart<CollectionMap>).GetMethod("Is", new Type[] { typeof(Uri) }));
            ilGenerator.Emit(OpCodes.Pop);
            ilGenerator.Emit(OpCodes.Ret);
            return typeBuilderHelper;
        }

        private EntityMap CreateDictionaryEntryMapping(IDictionaryMappingProvider map)
        {
            var entry = GetOrCreateDictionaryEntryType(map.PropertyInfo);
            var type = typeof(DictionaryEntryMap<,,>);
            var typeArguments = new[] { entry }.Concat(map.PropertyInfo.PropertyType.GenericTypeArguments).ToArray();
            var ownerMapType = type.MakeGenericType(typeArguments);

            var defineDynamicModule = _emitHelper.GetDynamicModule();
            Type mapType;
            lock (defineDynamicModule)
            {
                mapType = defineDynamicModule.GetOrEmitType(entry.Name + "Map", builder => EmitEntryMap(map, builder, entry, ownerMapType));
            }

            return (EntityMap)Activator.CreateInstance(mapType);
        }

        private TypeBuilder EmitEntryMap(IDictionaryMappingProvider map, ModuleBuilder defineDynamicModule, Type entry, Type ownerMapType)
        {
            var typeBuilderHelper = defineDynamicModule.DefineType(entry.Name + "Map", TypeAttributes.Public, ownerMapType);
            var setupKeyMethod = typeBuilderHelper.DefineMethod(
                "SetupKeyProperty",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig,
                typeof(void),
                new[] { typeof(ITermPart<IPropertyMap>) });
            EmitSetupPropertyOverride(setupKeyMethod, map.Key);

            var setupValueMethod = typeBuilderHelper.DefineMethod(
                "SetupValueProperty",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig,
                typeof(void),
                new[] { typeof(ITermPart<IPropertyMap>) });
            EmitSetupPropertyOverride(setupValueMethod, map.Value);
            
            return typeBuilderHelper;
        }

        private void EmitSetupPropertyOverride(MethodBuilder methodBuilder, IPredicateMappingProvider termMapping)
        {
            var ilGenerator = methodBuilder.GetILGenerator();
            ilGenerator.Emit(OpCodes.Nop);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldstr, termMapping.GetTerm(_ontologyProvider).ToString());
            ilGenerator.Emit(OpCodes.Newobj, typeof(Uri).GetConstructor(new[] { typeof(string) }));
            ilGenerator.Emit(OpCodes.Callvirt, typeof(ITermPart<PropertyMap>).GetMethod("Is", new Type[] { typeof(Uri) }));
            if (termMapping.ConverterType != null)
            {
                ilGenerator.Emit(OpCodes.Callvirt, typeof(IPropertyMap).GetMethod("ConvertWith").MakeGenericMethod(termMapping.ConverterType));
            }

            ilGenerator.Emit(OpCodes.Pop);
            ilGenerator.Emit(OpCodes.Ret);  
        }

        private Type GetOrCreateDictionaryEntryType(PropertyInfo property, bool obtainOwnerType = false)
        {
            var dictionaryEntityNames = new DictionaryEntityNames(
                property.DeclaringType.Namespace,
                property.DeclaringType.Name,
                property.Name,
                property.DeclaringType.GetTypeInfo().Assembly.GetName().Name);

            Type type = null;
            string typeName;
            if (obtainOwnerType)
            {
                var entryType = GetOrCreateDictionaryEntryType(property);
                typeName = dictionaryEntityNames.FullOwnerTypeName;
                type = typeof(IDictionaryOwner<,,>).MakeGenericType(new[] { entryType }.Concat(property.PropertyType.GetGenericArguments()).ToArray());
            }
            else
            {
                typeName = dictionaryEntityNames.FullEntryTypeName;
                type = typeof(IDictionaryEntry<,>).MakeGenericType(property.PropertyType.GetGenericArguments());
            }

            return property.DeclaringType.GetTypeInfo().Assembly.GetType(typeName) ??
                   (Type)GetType().GetMethod("CreateDictionaryEntryType", BindingFlags.Instance | BindingFlags.NonPublic)
                       .MakeGenericMethod(type)
                       .Invoke(this, new object[] { typeName });
        }

        private Type CreateDictionaryEntryType<T>(string typeName)
        {
            var dynamicModule = _emitHelper.GetDynamicModule();
            Type result;
            lock (dynamicModule)
            {
                result = dynamicModule.GetOrEmitType(
                    typeName,
                    builder => dynamicModule.DefineType(typeName, TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract, null, new[] { typeof(T) }));
            }

            return result;
        }
    }
}