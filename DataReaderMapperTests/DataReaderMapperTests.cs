using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using DataReaderMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataReaderMapperTests
{
    

    [TestClass]
    public class DataReaderMapperTests
    {
        private static DataReaderMapper<DataTableReader> BuildAndConfigureFor<TDto>(Dictionary<Type, Expression> typeConvertors = null) where TDto : class, new()
        {
            var mapper = new DataReaderMapper<DataTableReader>(typeConvertors);
            mapper.Configure<TDto>();
            return mapper;
        }
        
        [TestMethod]
        public void Should_Correctly_Map_String()
        {
            var sut = BuildAndConfigureFor<PrimitiveDTO<string>>();
            string expectedValue = "ShouldBeThis";

            using (var reader = PrimitiveDTO<string>.BuildReader(expectedValue))
            {
                reader.Read();
                var actual = sut.Map<PrimitiveDTO<string>>(reader);

                Assert.AreEqual(expectedValue, actual.PropertyToTest);
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_Integer()
        {
            var sut = BuildAndConfigureFor<PrimitiveDTO<int>>();
            int expectedValue = 12345;

            using (var reader = PrimitiveDTO<int>.BuildReader(12345))
            {
                reader.Read();
                var actual = sut.Map<PrimitiveDTO<int>>(reader);

                Assert.AreEqual(expectedValue, actual.PropertyToTest);
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_DateTime()
        {
            var sut = BuildAndConfigureFor<PrimitiveDTO<DateTime>>();
            DateTime expectedValue = new DateTime(2000, 12, 25);

            using (var reader = PrimitiveDTO<DateTime>.BuildReader(expectedValue))
            {
                reader.Read();
                var actual = sut.Map<PrimitiveDTO<DateTime>>(reader);

                Assert.AreEqual(expectedValue, actual.PropertyToTest);
            }
        }

        [TestMethod]
        public void Should_Correctly_Ignore_Property_Without_MappableAttribute()
        {
            var sut = BuildAndConfigureFor<WithoutMappableAttribute>();
            
            using (var reader = WithoutMappableAttribute.BuildReader("This will not be mapped"))
            {
                reader.Read();
                var actual = sut.Map<WithoutMappableAttribute>(reader);

                Assert.IsNull(actual.NotMappedProperty);
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_String_Column_To_Nested_Class_Property()
        {
            var sut = BuildAndConfigureFor<ContainsNestedClassesDTO<string, PrimitiveDTO<string>>>();
            const string expectedValue = "ThisShouldGetMappedToTheNestedClassProperty";

            using (var reader = ContainsNestedClassesDTO<string, PrimitiveDTO<string>>.BuildReader(expectedValue))
            {
                reader.Read();
                var actual = sut.Map<ContainsNestedClassesDTO<string, PrimitiveDTO<string>>>(reader);

                Assert.AreEqual(expectedValue, actual.NestedPropertyToTest.PropertyToTest);
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_String_Column_To_Double_Nested_Class_Property()
        {
            var sut =
                BuildAndConfigureFor<
                    ContainsNestedClassesDTO<string,
                        ContainsNestedClassesDTO<string, 
                            PrimitiveDTO<string>>>>();

            const string expectedValue = "2LevelsNestedClass";

            using (var reader = ContainsNestedClassesDTO<string, ContainsNestedClassesDTO<string, PrimitiveDTO<string>>>.BuildReader(expectedValue))
            {
                reader.Read();
                var actual = sut.Map<ContainsNestedClassesDTO<string, ContainsNestedClassesDTO<string, PrimitiveDTO<string>>>>(reader);

                Assert.AreEqual(expectedValue, actual.NestedPropertyToTest.NestedPropertyToTest.PropertyToTest);
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_Class_With_Multiple_Primitive_And_Complex_Properties()
        {
            var sut = BuildAndConfigureFor<WithMultiplePrimitiveAndComplexProperties>();

            using (var reader = WithMultiplePrimitiveAndComplexProperties.BuildReader())
            {
                reader.Read();
                var actual = sut.Map<WithMultiplePrimitiveAndComplexProperties>(reader);

                Assert.AreEqual(WithMultiplePrimitiveAndComplexProperties.ExpectedString, actual.StringProperty);
                Assert.AreEqual(WithMultiplePrimitiveAndComplexProperties.ExpectedString, actual.AnotherStringProperty);
                Assert.AreEqual(WithMultiplePrimitiveAndComplexProperties.ExpectedInteger, actual.IntegerProperty);
                Assert.AreEqual(WithMultiplePrimitiveAndComplexProperties.ExpectedString, actual.NestedWithStringProperty.PropertyToTest);
                Assert.AreEqual(WithMultiplePrimitiveAndComplexProperties.ExpectedString, actual.NestedWithAnotherStringProperty.PropertyToTest);
                Assert.AreEqual(WithMultiplePrimitiveAndComplexProperties.ExpectedString, actual.DoubleNestedClassWithStringProperty.NestedPropertyToTest.PropertyToTest);
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_StringAsDateTime()
        {
            var sut = BuildAndConfigureFor<MapperDTO<DateTime, string>>();
            DateTime expectedDateValue = new DateTime(9999, 12, 31);
            string expectedStringValue = expectedDateValue.ToShortDateString();

            using (var reader = MapperDTO<DateTime, string>.BuildReader(expectedStringValue))
            {
                reader.Read();
                var actual = sut.Map<MapperDTO<DateTime, string>>(reader);

                Assert.AreEqual(expectedDateValue, actual.PropertyToTest);
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_IntegerAsString()
        {
            var sut = BuildAndConfigureFor<MapperDTO<int, string>> ();
            int expectedValue = 12345;
            string expectedStringValue = expectedValue.ToString();

            using (var reader = MapperDTO<int, string>.BuildReader(expectedStringValue))
            {
                reader.Read();
                var actual = sut.Map<MapperDTO<int, string>>(reader);

                Assert.AreEqual(expectedValue, actual.PropertyToTest);
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_Multiple_Rows()
        {
            var sut = BuildAndConfigureFor<PrimitiveDTO<string>>();
            const string expectedValue = "ShouldBeThisAgain";
            int expectedNumberOfMappedEntities = 5;

            using (var reader = PrimitiveDTO<string>.BuildReader(expectedValue, 5))
            {                
                var actual = sut.MapAll<PrimitiveDTO<string>>(reader).ToList();

                Assert.AreEqual(expectedNumberOfMappedEntities, actual.Count);
                Assert.IsTrue(actual.All(x => x.PropertyToTest == expectedValue));
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_Boolean()
        {
            var sut = BuildAndConfigureFor<PrimitiveDTO<bool>>();
            bool expectedValue = true;

            using (var reader = PrimitiveDTO<bool>.BuildReader(expectedValue))
            {
                reader.Read();
                var actual = sut.Map<PrimitiveDTO<bool>>(reader);

                Assert.AreEqual(expectedValue, actual.PropertyToTest);
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_Decimal()
        {
            var sut = BuildAndConfigureFor<PrimitiveDTO<decimal>>();
            decimal expectedValue = 12.31M;

            using (var reader = PrimitiveDTO<decimal>.BuildReader(expectedValue))
            {
                reader.Read();
                var actual = sut.Map<PrimitiveDTO<decimal>>(reader);

                Assert.AreEqual(expectedValue, actual.PropertyToTest);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Should_Throw_When_Trying_To_Convert_Invalid_Types_Without_Specified_Custom_Converter()
        {
            var sut = BuildAndConfigureFor<MapperDTO<int, string>>(new Dictionary<Type, Expression>());

            using (var reader = MapperDTO<int, string>.BuildReader("12345"))
            {
                reader.Read();
                var actual = sut.Map<MapperDTO<int, string>>(reader);

                Assert.Fail("A conversion for this type should not be supported without explicit TypeConvertor Expression injection to the mapper.");
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_Null_For_Nullable_Property()
        {
            var sut = BuildAndConfigureFor<PrimitiveDTO<string>>();
            string expectedValue = null;
            using (var reader = PrimitiveDTO<string>.BuildReader(expectedValue))
            {
                reader.Read();

                var dtoWithNullableProperty = sut.Map<PrimitiveDTO<string>>(reader);

                Assert.AreEqual(expectedValue, dtoWithNullableProperty.PropertyToTest);
            }
        }

        //[TestMethod]
        //public void Should_Correctly_Map_List_Of_Strings()
        //{
        //    _sut.Configure<CollectionTestDto>();

        //    using (var reader = CollectionTestDtoBuilder.BuildReader())
        //    {
        //        reader.Read();
        //        var actual = _sut.Map<CollectionTestDto>(reader);
        //        Assert.AreEqual(actual.ListOfStrings.Count, CollectionTestDtoBuilder.SplitStrings.Count);
        //    }
        //}

        //[TestMethod]
        //public void Should_Correctly_Cache_Mapping_Function_For_Class_Property_After_Configure_On_Parent()
        //{
        //    var sut = new DataReaderMapper<DataTableReader>();
        //    sut.Configure<FuncCacheTestDto>();
        //    using (var reader = FuncCacheTestDtoBuilder.BuildReader())
        //    {
        //        reader.Read();

        //        var actualNestedOneLevel = sut.Map<NestedFuncDto>(reader);
        //        var actual2LevelsDeep = sut.Map<AnotherNestedDto>(reader);

        //        Assert.AreEqual(FuncCacheTestDtoBuilder.NestedClassPropertyExpectedValue, actualNestedOneLevel.Number);
        //        Assert.AreEqual(FuncCacheTestDtoBuilder.NestedClassPropertyExpectedValue, actual2LevelsDeep.AnotherNumber);
        //    }
        //}
        



    }
}
