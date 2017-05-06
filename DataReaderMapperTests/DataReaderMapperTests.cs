using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static DataReaderMapperTests.TestUtils;

namespace DataReaderMapperTests
{
    [TestClass]
    public class DataReaderMapperTests
    {
        [TestMethod]
        public void Should_Correctly_Map_String()
        {
            var sut = BuildAndConfigureFor<PrimitiveNullableDto<string>>();
            string expectedValue = "ShouldBeThis";

            using (var reader = PrimitiveNullableDto<string>.BuildReader(expectedValue))
            {
                reader.Read();
                var actual = sut.Map<PrimitiveNullableDto<string>>(reader);

                Assert.AreEqual(expectedValue, actual.PropertyToTest);
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_Integer()
        {
            var sut = BuildAndConfigureFor<PrimitiveDto<int>>();
            int expectedValue = 12345;

            using (var reader = PrimitiveDto<int>.BuildReader(12345))
            {
                reader.Read();
                var actual = sut.Map<PrimitiveDto<int>>(reader);

                Assert.AreEqual(expectedValue, actual.PropertyToTest);
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_DateTime()
        {
            var sut = BuildAndConfigureFor<PrimitiveDto<DateTime>>();
            var expectedValue = new DateTime(2000, 12, 25);

            using (var reader = PrimitiveDto<DateTime>.BuildReader(expectedValue))
            {
                reader.Read();
                var actual = sut.Map<PrimitiveDto<DateTime>>(reader);

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
            var sut = BuildAndConfigureFor<ContainsNestedClassesDto<string, PrimitiveDto<string>>>();
            const string expectedValue = "ThisShouldGetMappedToTheNestedClassProperty";

            using (var reader = ContainsNestedClassesDto<string, PrimitiveDto<string>>.BuildReader(expectedValue))
            {
                reader.Read();
                var actual = sut.Map<ContainsNestedClassesDto<string, PrimitiveDto<string>>>(reader);

                Assert.AreEqual(expectedValue, actual.NestedPropertyToTest.PropertyToTest);
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_String_Column_To_Double_Nested_Class_Property()
        {
            var sut =
                BuildAndConfigureFor<
                    ContainsNestedClassesDto<string,
                        ContainsNestedClassesDto<string,
                            PrimitiveDto<string>>>>();

            const string expectedValue = "2LevelsNestedClass";

            using (var reader = ContainsNestedClassesDto<string, ContainsNestedClassesDto<string, PrimitiveDto<string>>>
                .BuildReader(expectedValue))
            {
                reader.Read();
                var actual = sut
                    .Map<ContainsNestedClassesDto<string, ContainsNestedClassesDto<string, PrimitiveDto<string>>>
                    >(reader);

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
                Assert.AreEqual(WithMultiplePrimitiveAndComplexProperties.ExpectedString,
                    actual.NestedWithStringProperty.PropertyToTest);
                Assert.AreEqual(WithMultiplePrimitiveAndComplexProperties.ExpectedString,
                    actual.NestedWithAnotherStringProperty.PropertyToTest);
                Assert.AreEqual(WithMultiplePrimitiveAndComplexProperties.ExpectedString,
                    actual.DoubleNestedClassWithStringProperty.NestedPropertyToTest.PropertyToTest);
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_StringAsDateTime()
        {
            var sut = BuildAndConfigureFor<ConversionDto<DateTime, string>>();
            var expectedDateValue = new DateTime(9999, 12, 31);
            string expectedStringValue = expectedDateValue.ToShortDateString();

            using (var reader = ConversionDto<DateTime, string>.BuildReader(expectedStringValue))
            {
                reader.Read();
                var actual = sut.Map<ConversionDto<DateTime, string>>(reader);

                Assert.AreEqual(expectedDateValue, actual.PropertyToTest);
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_IntegerAsString()
        {
            var sut = BuildAndConfigureFor<ConversionDto<int, string>>();
            int expectedValue = 12345;
            string expectedStringValue = expectedValue.ToString();

            using (var reader = ConversionDto<int, string>.BuildReader(expectedStringValue))
            {
                reader.Read();
                var actual = sut.Map<ConversionDto<int, string>>(reader);

                Assert.AreEqual(expectedValue, actual.PropertyToTest);
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_Multiple_Rows()
        {
            var sut = BuildAndConfigureFor<PrimitiveNullableDto<string>>();
            const string expectedValue = "ShouldBeThisAgain";
            int expectedNumberOfMappedEntities = 5;

            using (var reader = PrimitiveNullableDto<string>.BuildReader(expectedValue, 5))
            {
                var actual = sut.MapAll<PrimitiveNullableDto<string>>(reader).ToList();

                Assert.AreEqual(expectedNumberOfMappedEntities, actual.Count);
                Assert.IsTrue(actual.All(x => x.PropertyToTest == expectedValue));
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_Boolean()
        {
            var sut = BuildAndConfigureFor<PrimitiveDto<bool>>();
            bool expectedValue = true;

            using (var reader = PrimitiveDto<bool>.BuildReader(expectedValue))
            {
                reader.Read();
                var actual = sut.Map<PrimitiveDto<bool>>(reader);

                Assert.AreEqual(expectedValue, actual.PropertyToTest);
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_Decimal()
        {
            var sut = BuildAndConfigureFor<PrimitiveDto<decimal>>();
            decimal expectedValue = 12.31M;

            using (var reader = PrimitiveDto<decimal>.BuildReader(expectedValue))
            {
                reader.Read();
                var actual = sut.Map<PrimitiveDto<decimal>>(reader);

                Assert.AreEqual(expectedValue, actual.PropertyToTest);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Should_Throw_When_Trying_To_Convert_Invalid_Types_Without_Specified_Custom_Converter()
        {
            var sut = BuildAndConfigureFor<ConversionDto<int, string>>(new Dictionary<Type, Expression>());

            using (var reader = ConversionDto<int, string>.BuildReader("12345"))
            {
                reader.Read();
                var actual = sut.Map<ConversionDto<int, string>>(reader);

                Assert.Fail(
                    "A conversion for this type should not be supported without explicit TypeConvertor Expression injection to the mapper.");
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_Null_For_Nullable_Property()
        {
            var sut = BuildAndConfigureFor<PrimitiveNullableDto<string>>();
            string expectedValue = null;
            using (var reader = PrimitiveNullableDto<string>.BuildReader(expectedValue))
            {
                reader.Read();

                var dtoWithNullableProperty = sut.Map<PrimitiveNullableDto<string>>(reader);

                Assert.AreEqual(expectedValue, dtoWithNullableProperty.PropertyToTest);
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_List_Of_Strings()
        {
            var sut = BuildAndConfigureFor<ConversionDto<List<string>, string>>();
            const string concatenatedValues = "this,will,be,split,to,multiple,strings";
            int expectedListCount = concatenatedValues.Split(',').Length;

            using (var reader = ConversionDto<List<string>, string>.BuildReader(concatenatedValues))
            {
                reader.Read();
                var actual = sut.Map<ConversionDto<List<string>, string>>(reader);
                Assert.AreEqual(expectedListCount, actual.PropertyToTest.Count);
                Assert.AreEqual("this", actual.PropertyToTest.First());
                Assert.AreEqual("split", actual.PropertyToTest[3]);
                Assert.AreEqual("strings", actual.PropertyToTest.Last());
            }
        }

        [TestMethod]
        public void Should_Correctly_Cache_Mapping_Function_For_Class_Property_After_Configure_On_Parent()
        {
            var sut = BuildAndConfigureFor<ContainsNestedClassesDto<string, PrimitiveDto<string>>>();
            const string expectedValue = "THIS";

            using (var reader = ContainsNestedClassesDto<string, PrimitiveDto<string>>.BuildReader(expectedValue))
            {
                reader.Read();

                var theParent = sut.Map<ContainsNestedClassesDto<string, PrimitiveDto<string>>>(reader);
                var actual = sut.Map<PrimitiveDto<string>>(reader);

                Assert.AreEqual(expectedValue, actual.PropertyToTest);
                Assert.AreEqual(expectedValue, theParent.NestedPropertyToTest.PropertyToTest);
            }
        }
    }
}
