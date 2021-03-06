﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using RomanticWeb.Entities;
using RomanticWeb.LightInject;
using RomanticWeb.TestEntities.Foaf;
using RomanticWeb.TestEntities.LargeDataset;
using RomanticWeb.Tests.Stubs;

namespace RomanticWeb.Tests.IntegrationTests
{
    public abstract class LoadingTestsBase : IntegrationTestsBase
    {
        private static readonly EntityId EntityId = new EntityId("http://magi/people/Tomasz");

        internal interface ITrackedScopes { IEnumerable<Scope> TrackedScopes { get; } }

        [Test]
        public void Should_load_as_best_derived_type_in_inheritance_tree()
        {
            // given
            LoadTestFile("InheritingEntities.trig");

            // when
            var entity = EntityContext.Load<IAgent>("http://magi/people/Tomasz");

            // then
            entity.Should().BeAssignableTo<IPerson>();
        }

        [Test]
        public void Should_load_entity_as_best_types()
        {
            // given
            LoadTestFile("InheritingEntities.trig");

            // when
            var entity = EntityContext.Load<IEntity>(EntityId);

            // then
            entity.Should().BeAssignableTo<IPerson>();
            entity.Should().BeAssignableTo<IAlsoPerson>();
        }

        [Test]
        public void Entity_should_be_castable_to_multiple_types()
        {
            // given
            LoadTestFile("InheritingEntities.trig");

            // when
            dynamic entity = EntityContext.Load<IEntity>(EntityId);

            // then
            Assert.That(entity is IPerson);
            Assert.That(entity is IAlsoPerson);
            ((IPerson)entity).Name.Should().Be("Tomasz");
            ((IAlsoPerson)entity).LastName.Should().Be("Pluskiewicz");
        }

        [Test]
        public void Should_load_entity_with_all_matching_derived_types()
        {
            // given
            LoadTestFile("EntityWithManyTypes.trig");

            // when
            var entity = EntityContext.Load<IAgent>(EntityId);

            // then
            entity.Should().BeAssignableTo<TestEntities.Foaf.IPerson>();
        }

        [Test]
        public void Should_load_associated_entities_with_all_matching_derived_types()
        {
            // given
            LoadTestFile("EntityWithManyTypes.trig");
            var entity = EntityContext.Load<IPerson>(EntityId);

            // when
            var karol = entity.KnowsOne;

            // then
            karol.Should().BeAssignableTo<TestEntities.Foaf.IPerson>();
        }

        [Test]
        public void Should_change_entity_type_best_matching_derived_type()
        {
            // given
            LoadTestFile("EntityWithManyTypes.trig");
            var entity = EntityContext.Load<ITypedEntity>(EntityId);

            // when
            var agent = entity.AsEntity<IAgent>();

            // then
            agent.Should().BeAssignableTo<TestEntities.Foaf.IPerson>();
        }

        [Test]
        [Ignore("This test is supposed to fail, as it is only for tuning performance.")]
        public void Should_list_entities_from_large_dataset_in_a_timely_fashion_way()
        {
            // given
            LoadTestFile("LargeDataset.nq");
            var stopwatch = new Stopwatch();

            // when
            stopwatch.Start();
            IEnumerable<IProduct> entities = EntityContext.AsQueryable<IProduct>().ToList();
            stopwatch.Stop();

            // then
            Console.Write("Time taken: {0}", stopwatch.ElapsedMilliseconds);
            stopwatch.ElapsedMilliseconds.Should().BeLessOrEqualTo(2000);
        }

        [Test]
        public void Should_enumerate_entities_from_large_dataset_in_a_timely_fashion_way()
        {
            // given
            LoadTestFile("LargeDataset.nq");
            IEnumerable<IProduct> entities = EntityContext.AsQueryable<IProduct>().ToList();
            var stopwatch = new Stopwatch();

            // when
            stopwatch.Start();
            foreach (IProduct product in entities)
            {
                string name = product.Name;
                string comments = product.Comments;
                string viscosity = (product.Viscosity != null ? System.String.Join(", ", product.Viscosity.Viscosity.Select(item => System.String.Format("{0}{1}", item.Unit, item.Value))) : System.String.Empty);
                string cureSystem = (product.CureSystem != null ? product.CureSystem.ToString() : System.String.Empty);
                string cureTemperature = System.String.Join(", ", product.CureTemperature.Select(item => System.String.Format("{0}{1}", item.Unit, item.Value)));
                string durometer = System.String.Join(", ", product.Durometer.Select(item => System.String.Format("{0}{1}", item.Unit, item.Value)));
                string tensile = System.String.Join(", ", product.Tensile.Select(item => System.String.Format("{0}{1}", item.Unit, item.Value)));
                string elongation = System.String.Join(", ", product.Elongation.Select(item => System.String.Format("{0}{1}", item.Unit, item.Value)));
                string tear = (product.Tear != null ? System.String.Format("{0}{1}", product.Tear.Unit, product.Tear.Value) : System.String.Empty);
                string rheology = System.String.Join(", ", product.Rheology.Select(item => System.String.Format("{0}{1}", item.Unit, item.Value)));
                string specificGravity = System.String.Join(", ", product.SpecificGravity);
                string industry = (product.Industry ?? System.String.Empty).ToString();
                string grade = System.String.Join(", ", product.Grade.Select(item => item.ToString()));
                string productCategory = System.String.Join(", ", product.ProductCategory);
                string msdsFile = System.String.Join(", ", product.MsdsFile.Select(item => item.Id.ToString()));
                string function = System.String.Join(", ", product.Function.Select(item => item.ToString()));
            }

            stopwatch.Stop();

            // then
            Console.Write("Time taken: {0}", stopwatch.ElapsedMilliseconds);
            stopwatch.ElapsedMilliseconds.Should().BeLessOrEqualTo(2000);
        }

        [Test]
        [TestCase("http://chem.com/product/CV1-2566/Cure")]
        public void Should_select_entities_by_sub_query(string propertyId)
        {
            // given
            LoadTestFile("ComplexDataset.nq");

            ICureComplex property = EntityContext.Load<ICureComplex>(new EntityId(new Uri(propertyId)));

            // when
            IList<IProduct> products = (from product in EntityContext.AsQueryable<IProduct>()
                                        where product.CureComplex.Contains(property)
                                        select product).ToList();

            // then
            Assert.That(products.Count, Is.Not.EqualTo(0));
        }

        [Test]
        [Repeat(10)]
        [TestCase("SCV", 2)]
        public void Should_return_limited_number_of_entities_from_complex_dataset(string searchString, int expectedCount)
        {
            // given
            LoadTestFile("ComplexDataset.nq");

            // when
            IList<IProduct> products = EntityContext.AsQueryable<IProduct>()
                .Where(item => item.Name.ToLower().Contains(searchString.ToLower()))
                .Take(2)
                .ToList();

            // then
            Assert.That(products.Count, Is.EqualTo(expectedCount));
        }

        [Test]
        [TestCase("SCV", 10)]
        public void Should_return_count_of_entities(string searchString, int expectedCount)
        {
            // given
            LoadTestFile("ComplexDataset.nq");

            // when
            int count = EntityContext.AsQueryable<IProduct>().Count(item => item.Name.ToLower().Contains(searchString.ToLower()));

            // then
            Assert.That(count, Is.EqualTo(expectedCount));
        }

        [Test]
        public void Should_return_filtered_entities()
        {
            // given
            LoadTestFile("ComplexDataset.nq");

            // when
            IList<IProduct> products = (from product in EntityContext.AsQueryable<IProduct>()
                                        where (product.Group.Contains("Northrop Grumman")) || (!product.Group.Any())
                                        select product).ToList();

            // then
            Assert.That(products.Count, Is.Not.EqualTo(0));
        }

        [Test]
        [TestCase("http://chem.com/product/R32-2186", "http://chem.com/vocab/cureSystem")]
        public void Should_read_entity_predicates(string productId, string predicateUri)
        {
            // given
            LoadTestFile("ComplexDataset.nq");

            // when
            IProduct product = EntityContext.Load<IProduct>(new EntityId(productId));

            // then
            Assert.That(product.Predicates().Count(), Is.Not.EqualTo(0));
            Assert.That(product.Predicate(new Uri(predicateUri)), Is.Not.Null);
        }

        [Test]
        [TestCase("http://chem.com/product/CV1-2566", "http://chem.com/vocab/viscosityComplex")]
        public void Should_read_blank_node(string productId, string predicateUri)
        {
            // given
            LoadTestFile("ComplexDataset.nq");

            // when
            IProduct product = EntityContext.Load<IProduct>(new EntityId(productId));

            // then
            Assert.That(product.Predicate(new Uri(predicateUri)), Is.InstanceOf<IViscosityComplex>());
            Assert.That(product.Tensile.FirstOrDefault(), Is.InstanceOf<IQuantitativeFloatProperty>());
        }

        [Test]
        public void Should_find_entities_with_subquery()
        {
            LoadTestFile("ComplexDataset.nq");
            EntityId searched = new EntityId("http://chem.com/vocab/AdhereSealPromoteAdhesion");
            IEnumerable<IProduct> products = from product in EntityContext.AsQueryable<IProduct>()
                                             from func in product.Function
                                             where func == searched
                                             select product;
            Assert.That(products.Count(), Is.Not.EqualTo(0));
        }

        [Test]
        public void Select_with_multiple_combined_statements()
        {
            LoadTestFile("ComplexDataset.nq");
            IQueryable<IProduct> result = null;
            var fUri = new EntityId("http://chem.com/vocab/PotEncapsulate");
            result = from product in EntityContext.AsQueryable<IProduct>()
                     from func in product.Function
                     where func == fUri
                     select product;

            var iUri = new EntityId("http://chem.com/vocab/LifeSciences");
            result = from product in result ?? EntityContext.AsQueryable<IProduct>()
                     where product.Industry == iUri
                     select product;

            IEnumerable<IProduct> products = result.ToList();
            Assert.That(products.Count(), Is.Not.EqualTo(0));
        }

        [Test]
        public void Select_with_alternative_property_conditions()
        {
            LoadTestFile("ComplexDataset.nq");
            EntityId id = new EntityId("http://chem.com/vocab/LifeSciences");
            IEnumerable<IProduct> result = (from product in EntityContext.AsQueryable<IProduct>()
                                            where (product.Industry == id) || (product.Industry == null)
                                            select product).ToList();
            Assert.That(result.Count(), Is.Not.EqualTo(0));
        }

        [Test]
        public void Select_with_alternative_property_conditions_expressed_as_inner_predicate()
        {
            LoadTestFile("ComplexDataset.nq");
            EntityId id = new EntityId("http://chem.com/vocab/LifeSciences");
            IProduct result = EntityContext.AsQueryable<IProduct>().FirstOrDefault(product => (product.Industry == id) || (product.Industry == null));
            Assert.That(result.Id, Is.EqualTo(new EntityId("http://chem.com/product/MED3-6300")));
        }

        [Test]
        public void Select_with_type_check()
        {
            LoadTestFile("ComplexDataset.nq");
            IEnumerable<IEntity> products = (from product in EntityContext.AsQueryable()
                                              where product.Is(new Uri("http://chem.com/vocab/Product"))
                                              select product).ToList();
            Assert.That(products.Count(), Is.Not.EqualTo(0));
        }

        [Test]
        public void Should_allow_setting_relative_EntityId_values()
        {
            IProduct product = EntityContext.Create<IProduct>(new EntityId("http://test.com/test"));
            product.Industry = new EntityId(new Uri("/test", UriKind.Relative));
            Assert.That(product.Industry.Uri.ToString().StartsWith("http"));
        }

        [Test]
        [TestCase("http://chem.com/vocab/tensile", 400.0, 600.0)]
        public void Select_with_predicate_value_type_casted_to_collection_of_IQuantitativeFloatProperty(string predicateUriString, double minValue, double maxValue)
        {
            LoadTestFile("ComplexDataset.nq");
            Uri predicateUri = new Uri(predicateUriString);
            IEnumerable<IProduct> result = (from product in EntityContext.AsQueryable<IProduct>()
                                            let predicateValue = product.Predicate(predicateUri) as ICollection<IQuantitativeFloatProperty>
                                            from quantitativeValue in predicateValue
                                            where quantitativeValue.Value > minValue && quantitativeValue.Value < maxValue
                                            select product).ToList();
            Assert.That(result.Count(), Is.Not.EqualTo(0));
        }

        [Test]
        [TestCase("http://chem.com/vocab/cureSystem", "http://chem.com/vocab/Acetoxy", "http://chem.com/vocab/Oxime")]
        public void Select_with_predicate_value_type_casted_to_EntityId(string predicateUriString, params string[] filterValues)
        {
            LoadTestFile("ComplexDataset.nq");
            Uri predicateUri = new Uri(predicateUriString);
            List<EntityId> filter = new List<EntityId>();
            foreach (string filterValue in filterValues)
            {
                filter.Add(new EntityId(filterValue));
            }

            IEnumerable<IProduct> result = (from product in EntityContext.AsQueryable<IProduct>()
                                            where filter.Contains(product.Predicate(predicateUri) as EntityId)
                                            select product).ToList();

            Assert.That(result.Count(), Is.Not.EqualTo(0));
        }

        [Test]
        [TestCase("http://chem.com/vocab/cureTime", 0.0, 100.0)]
        public void Select_with_nested_where_clause(string predicateUriString, double minValue, double maxValue)
        {
            LoadTestFile("ComplexDataset.nq");
            Uri predicateUri = new Uri(predicateUriString);

            IEnumerable<IProduct> result = (from product in EntityContext.AsQueryable<IProduct>()
                                            from complex in product.CureComplex
                                            from quantitativeValue in complex.Predicate(predicateUri) as ICollection<IQuantitativeFloatProperty>
                                            where quantitativeValue.Value > minValue && quantitativeValue.Value < maxValue
                                            select product).ToList();

            Assert.That(result.Count(), Is.Not.EqualTo(0));
        }

        [Test]
        [TestCase("http://chem.com/vocab/tear", 0.0, 400.0)]
        public void Select_with_predicate_value_type_casted_to_IQuantitativeFloatProperty(string predicateUriString, double minValue, double maxValue)
        {
            LoadTestFile("ComplexDataset.nq");
            Uri predicateUri = new Uri(predicateUriString);
            IEnumerable<IProduct> result = (from product in EntityContext.AsQueryable<IProduct>()
                                            let predicateValue = product.Predicate(predicateUri) as IQuantitativeFloatProperty
                                            where predicateValue.Value > minValue && predicateValue.Value < maxValue
                                            select product).ToList();
            Assert.That(result.Count(), Is.Not.EqualTo(0));
        }

        [Test]
        [TestCase("http://chem.com/vocab/specificGravity", 0.0, 400.0)]
        public void Select_with_predicate_value_type_casted_to_collection_of_double(string predicateUriString, double minValue, double maxValue)
        {
            LoadTestFile("ComplexDataset.nq");
            Uri predicateUri = new Uri(predicateUriString);
            IEnumerable<IProduct> result = (from product in EntityContext.AsQueryable<IProduct>()
                                            from predicateValue in product.Predicate(predicateUri) as ICollection<double>
                                            where predicateValue > minValue && predicateValue < maxValue
                                            select product).ToList();
            Assert.That(result.Count(), Is.Not.EqualTo(0));
        }

        [Test]
        [TestCase("http://chem.com/vocab/viscosity", 0.0, 400.0)]
        public void Select_with_predicate_value_type_casted_to_IViscosityComplex(string predicateUriString, double minValue, double maxValue)
        {
            LoadTestFile("ComplexDataset.nq");
            Uri predicateUri = new Uri(predicateUriString);
            IEnumerable<IProduct> result = (from product in EntityContext.AsQueryable<IProduct>()
                                            from predicateValue in product.Viscosity.Predicate(predicateUri) as ICollection<IQuantitativeFloatProperty>
                                            where predicateValue.Value > minValue && predicateValue.Value < maxValue
                                            select product).ToList();
            Assert.That(result.Count(), Is.Not.EqualTo(0));
        }

        [Test]
        [TestCase("http://chem.com/vocab/tear", 0.0, 400.0, "http://chem.com/vocab/tensile", 0.0, 400.0)]
        public void Select_with_concatenation_of_two_predicate_casts(string predicateUriString1, double minValue1, double maxValue1, string predicateUriString2, double minValue2, double maxValue2)
        {
            LoadTestFile("ComplexDataset.nq");
            Uri predicateUri1 = new Uri(predicateUriString1);
            IQueryable<IProduct> query = from product in EntityContext.AsQueryable<IProduct>()
                                         from predicateValue in product.Predicate(predicateUri1) as ICollection<IQuantitativeFloatProperty>
                                         where predicateValue.Value > minValue1 && predicateValue.Value < maxValue1
                                         select product;

            Uri predicateUri2 = new Uri(predicateUriString2);
            query = from product in query
                    from predicateValue in product.Predicate(predicateUri2) as ICollection<IQuantitativeFloatProperty>
                    where predicateValue.Value > minValue2 && predicateValue.Value < maxValue2
                    select product;

            IEnumerable<IProduct> result = query.ToList();
            Assert.That(result.Count(), Is.Not.EqualTo(0));
        }

        [Test]
        [TestCase(0.0, 400.0)]
        public void Select_with_complex_type_casts_and_where_clauses(double minValue, double maxValue)
        {
            LoadTestFile("ComplexDataset.nq");
            IEnumerable<IProduct> result = (from product in EntityContext.AsQueryable<IProduct>()
                                            where (product.Viscosity.Viscosity.Any(viscosity => (viscosity.Value >= minValue && viscosity.Value <= maxValue))) ||
                                                  (product.Viscosity.PartA.Any(viscosityA => (viscosityA.Value >= minValue && viscosityA.Value <= maxValue))) ||
                                                  (product.Viscosity.PartB.Any(viscosityB => (viscosityB.Value >= minValue && viscosityB.Value <= maxValue)))
                                            select product).ToList();
            Assert.That(result.Count(), Is.Not.EqualTo(0));
        }

        [Test]
        [TestCase("http://chem.com/vocab/tensile", 0.0, 400.0, "http://chem.com/vocab/mixRatio", "1:1")]
        public void Select_with_cross_result_of_multiple_queries(string predicateUriString1, double minValue1, double maxValue1, string predicateUriString2, params string[] filterValues)
        {
            LoadTestFile("ComplexDataset.nq");
            Uri predicateUri1 = new Uri(predicateUriString1);
            IQueryable<IProduct> query = from product in EntityContext.AsQueryable<IProduct>()
                                         from predicateValue in product.Predicate(predicateUri1) as ICollection<IQuantitativeFloatProperty>
                                         where predicateValue.Value > minValue1 && predicateValue.Value < maxValue1
                                         select product;

            Uri predicateUri2 = new Uri(predicateUriString2);
            query = from product in query
                    from filterValue in filterValues
                    where (product.Predicate(predicateUri2) as ICollection<string>).Contains(filterValue)
                    select product;

            IEnumerable<IProduct> result = query.ToList();
            Assert.That(result.Count(), Is.Not.EqualTo(0));
        }

        [Test]
        [TestCase("http://chem.com/vocab/appearance", "Yellow")]
        public void Select_with_regular_expression_filter(string predicateUriString, params string[] filterValues)
        {
            LoadTestFile("ComplexDataset.nq");
            Uri predicateUri = new Uri(predicateUriString);
            string pattern = System.String.Join("|", filterValues.Select(item => System.Text.RegularExpressions.Regex.Escape(item)));
            IEnumerable<IProduct> result = (from product in EntityContext.AsQueryable<IProduct>()
                                            from value in product.Predicate(predicateUri) as ICollection<string>
                                            where System.Text.RegularExpressions.Regex.IsMatch(value, pattern)
                                            select product).ToList();

            Assert.That(result.Count(), Is.Not.EqualTo(0));
        }

        [Test]
        [TestCase("http://chem.com/vocab/mixRatio", "1:1", "http://chem.com/vocab/cureSystem", "http://chem.com/vocab/Platinum")]
        public void Select_with_multiple_array_of_string_filters(string predicateUriString1, string filterValue1, string predicateUriString2, string filterValue2)
        {
            LoadTestFile("ComplexDataset.nq");
            Uri predicateUri1 = new Uri(predicateUriString1);
            string pattern = System.Text.RegularExpressions.Regex.Escape(filterValue1);
            IQueryable<IProduct> query = from product in EntityContext.AsQueryable<IProduct>()
                                         from value in product.Predicate(predicateUri1) as ICollection<string>
                                         where System.Text.RegularExpressions.Regex.IsMatch(value, pattern)
                                         select product;

            Uri predicateUri2 = new Uri(predicateUriString2);
            EntityId[] filterValues2 = new EntityId[] { new EntityId(filterValue2) };
            query = from product in query
                    where filterValues2.Contains(product.Predicate(predicateUri2) as EntityId)
                    select product;

            IEnumerable<IProduct> result = query.ToList();
            Assert.That(result.Count(), Is.Not.EqualTo(0));
        }

        [Test]
        [TestCase("http://chem.com/vocab/cureTime", 0.0, 400.0, "http://chem.com/vocab/appearance", "Black")]
        public void Select_with_concatenation_of_cast_and_regular_expression_test(string predicateUriString1, double minValue1, double maxValue1, string predicateUriString2, params string[] filterValues)
        {
            LoadTestFile("ComplexDataset.nq");
            Uri predicateUri1 = new Uri(predicateUriString1);
            IQueryable<IProduct> query = from product in EntityContext.AsQueryable<IProduct>()
                                         from predicateValue in product.Predicate(predicateUri1) as ICollection<IQuantitativeFloatProperty>
                                         where predicateValue.Value > minValue1 && predicateValue.Value < maxValue1
                                         select product;

            Uri predicateUri2 = new Uri(predicateUriString2);
            string pattern = System.String.Join("|", filterValues.Select(item => System.Text.RegularExpressions.Regex.Escape(item)));
            query = from product in query
                    from predicateValue in product.Predicate(predicateUri2) as ICollection<string>
                    where System.Text.RegularExpressions.Regex.IsMatch(predicateValue, pattern)
                    select product;

            IEnumerable<IProduct> result = query.ToList();
            Assert.That(result.Count(), Is.Not.EqualTo(0));
        }

        [Test]
        [TestCase("http://chem.com/vocab/tear", 0.0, 400.0, "http://chem.com/vocab/specificGravity", 0.0, 400.0)]
        public void Select_with_concatenation_of_cast_and_another_cast(string predicateUriString1, double minValue1, double maxValue1, string predicateUriString2, double minValue2, double maxValue2)
        {
            LoadTestFile("ComplexDataset.nq");
            Uri predicateUri1 = new Uri(predicateUriString1);
            IQueryable<IProduct> query = from product in EntityContext.AsQueryable<IProduct>()
                                         where product.Industry == new EntityId("http://chem.com/vocab/AdvancedEngineering")
                                         select product;

            query = from product in query
                    from predicateValue in product.Predicate(predicateUri1) as ICollection<IQuantitativeFloatProperty>
                    where predicateValue.Value < maxValue1
                    select product;

            Uri predicateUri2 = new Uri(predicateUriString2);
            query = from product in query
                    from predicateValue in product.Predicate(predicateUri2) as ICollection<double>
                    where predicateValue >= minValue2 && predicateValue <= maxValue2
                    select product;

            IEnumerable<IProduct> result = query.ToList();
            Assert.That(result.Count(), Is.Not.EqualTo(0));
        }

        [Test]
        public void Rollback_should_revert_changes_to_entities()
        {
            // given
            LoadTestFile("InheritingEntities.trig");
            var entity = EntityContext.Load<IPerson>(EntityId);
            entity.Name = "Tomek";
            entity.Gender = "male";

            // when
            EntityContext.Rollback();

            // then
            entity.Name.Should().Be("Tomasz");
            entity.Gender.Should().BeNull();
        }

        [Test]
        public void Having_disposed_a_context_should_be_able_to_use_a_next_one()
        {
            // given
            LoadTestFile("InheritingEntities.trig");
            IPerson person;
            using (var context = Factory.CreateContext())
            {
                person = context.Load<IPerson>(EntityId);
            }

            Factory.CreateContext();
            Factory.CreateContext();
            Factory.CreateContext();

            // when
            person = EntityContext.Load<IPerson>(EntityId);

            // then
            person.Name.Should().Be("Tomasz");
        }

        [Test]
        public void Disposing_contexts_should_be_possble_in_any_order()
        {
            // given
            var first = Factory.CreateContext();
            var second = Factory.CreateContext();
            var third = Factory.CreateContext();

            // when
            second.Dispose();
            third.Dispose();
            first.Dispose();

            // then
            Factory.TrackedScopes.Should().BeEmpty();
        }

        [Test]
        public void Should_load_multiple_levels_of_nested_blanks_to_store()
        {
            // given
            LoadTestFile("BlankNodes.trig");

            // when
            EntityContext.Load<INoTypeAgent>(EntityId);

            // then
            EntityContext.Store.GetEntityQuads(EntityId).Should().HaveCount(6);
        }

        protected override void ChildSetup()
        {
            Factory.WithNamedGraphSelector(new TestGraphSelector());
        }
    }
}