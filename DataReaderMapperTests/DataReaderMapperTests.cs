using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mapping;
using System.Data;
using System.Linq;
using static DataReaderMapper.Tests.TestDto;
using System.Collections.Generic;
using static DataReaderMapper.Tests.ExceptionTestDto;
using static DataReaderMapper.Tests.CollectionTestDto;
using static DataReaderMapper.Tests.FuncCacheTestDto;
using static DataReaderMapper.Tests.ConvertorIdTestDto;
using System.Linq.Expressions;

namespace DataReaderMapper.Tests
{

    [TestClass]
    public class DataReaderMapperTests
    {
        private DataReaderMapper<DataTableReader> _sut = new DataReaderMapper<DataTableReader>();

        [TestInitialize]
        public void Setup()
        {
            _sut.Configure<TestDto>();
        }

        [TestMethod]
        public void Should_Correctly_Map_String_Column()
        {
            using (var reader = TestDtoBuilder.BuildReader())
            {
                reader.Read();
                var actual = _sut.Map<TestDto>(reader);

                Assert.AreEqual(TestDtoBuilder.ExpectedStringColumnValue, actual.StringValue);
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_Integer_Column()
        {
            using (var reader = TestDtoBuilder.BuildReader())
            {
                reader.Read();
                var actual = _sut.Map<TestDto>(reader);

                Assert.AreEqual(TestDtoBuilder.ExpectedIntegerColumnValue, actual.IntegerValue);
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_DateTime_Column()
        {
            using (var reader = TestDtoBuilder.BuildReader())
            {
                reader.Read();
                var actual = _sut.Map<TestDto>(reader);

                Assert.AreEqual(TestDtoBuilder.ExpectedDateTimeColumnValue, actual.DateValue);
            }
        }

        [TestMethod]
        public void Should_Correctly_Ignore_Property_Without_MappableAttribute()
        {
            using (var reader = TestDtoBuilder.BuildReader())
            {
                reader.Read();
                var actual = _sut.Map<TestDto>(reader);

                Assert.IsNull(actual.PropertyWithoutMappableAttribute);
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_String_Column_To_Nested_Class_Property()
        {
            using (var reader = TestDtoBuilder.BuildReader())
            {
                reader.Read();
                var actual = _sut.Map<TestDto>(reader);

                Assert.AreEqual(TestDtoBuilder.ExpectedStringColumnValue, actual.NestedClass.StringValue);
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_String_Column_To_Double_Nested_Class_Property()
        {
            using (var reader = TestDtoBuilder.BuildReader())
            {
                reader.Read();
                var actual = _sut.Map<TestDto>(reader);

                Assert.AreEqual(TestDtoBuilder.ExpectedStringColumnValue, actual.NestedClass.NestedNestedClass.StringValue);
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_StringAsDateTime_Column()
        {
            using (var reader = TestDtoBuilder.BuildReader())
            {
                reader.Read();                

                var actual = _sut.Map<TestDto>(reader);

                Assert.AreEqual(TestDtoBuilder.DateTimeAsStringColumnValue, actual.DateTimeAsStringColumn);
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_IntegerAsString_Column()
        {
            using (var reader = TestDtoBuilder.BuildReader())
            {
                reader.Read();
                var actual = _sut.Map<TestDto>(reader);

                Assert.AreEqual(TestDtoBuilder.IntegerAsStringValue, actual.IntegerAsString);
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_Multiple_Rows()
        {
            int expectedNumberOfMappedEntities = 5;
            using (var reader = TestDtoBuilder.BuildReader(5))
            {                
                var actual = _sut.MapAll<TestDto>(reader);

                Assert.AreEqual(expectedNumberOfMappedEntities, actual.Count());
                Assert.IsTrue(actual.All(x => x.StringValue == TestDtoBuilder.ExpectedStringColumnValue));
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_Boolean_Column()
        {
            using (var reader = TestDtoBuilder.BuildReader())
            {
                reader.Read();
                var actual = _sut.Map<TestDto>(reader);

                Assert.AreEqual(TestDtoBuilder.ExpectedBooleanColumnValue, actual.BooleanValue);
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_Decimal_Column()
        {
            using (var reader = TestDtoBuilder.BuildReader())
            {
                reader.Read();
                var actual = _sut.Map<TestDto>(reader);

                Assert.AreEqual(TestDtoBuilder.ExpectedDecimalColumn, actual.DecimalColumn);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidCastException))]
        public void Should_Throw_When_Trying_To_Convert_Invalid_Types_Without_Specified_Custom_Converter()
        {
            _sut.Configure<ExceptionTestDto>();

            using (var reader = ExceptionTestDtoBuilder.BuildReader())
            {
                reader.Read();
                var actual = _sut.Map<ExceptionTestDto>(reader);
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_List_Of_Strings()
        {
            _sut.Configure<CollectionTestDto>();

            using (var reader = CollectionTestDtoBuilder.BuildReader())
            {
                reader.Read();
                var actual = _sut.Map<CollectionTestDto>(reader);
                Assert.AreEqual(actual.ListOfStrings.Count, CollectionTestDtoBuilder.SplitStrings.Count);
            }
        }

        [TestMethod]
        public void Should_Correctly_Cache_Mapping_Function_For_Class_Property_After_Configure_On_Parent()
        {
            var sut = new DataReaderMapper<DataTableReader>();
            sut.Configure<FuncCacheTestDto>();
            using (var reader = FuncCacheTestDtoBuilder.BuildReader())
            {
                reader.Read();

                var actualNestedOneLevel = sut.Map<NestedFuncDto>(reader);
                var actual2LevelsDeep = sut.Map<AnotherNestedDto>(reader);

                Assert.AreEqual(FuncCacheTestDtoBuilder.NestedClassPropertyExpectedValue, actualNestedOneLevel.Number);
                Assert.AreEqual(FuncCacheTestDtoBuilder.NestedClassPropertyExpectedValue, actual2LevelsDeep.AnotherNumber);
            }
        }

        [TestMethod]
        public void Should_Correctly_Convert_Property_With_Custom_Convertor_Specified_By_ID()
        {
            var idConvertors = new Dictionary<string, Expression>() {{ "convertorId=1", (Expression<Func<object, string>>)((object o) => $"Hello World {int.Parse(o.ToString())}" )}};

            var sut = new DataReaderMapper<DataTableReader>(null, idConvertors);
            sut.Configure<ConvertorIdTestDto>();
            string expected = $"Hello World {ConvertorIdTestDtoBuilder.NestedClassPropertyExpectedValue}";

            using (var reader = ConvertorIdTestDtoBuilder.BuildReader())
            {
                reader.Read();

                var actual = sut.Map<ConvertorIdTestDto>(reader);
                string actualMessage = actual.IntegerToBeConvertedToMessage;

                Assert.AreEqual(expected, actual.IntegerToBeConvertedToMessage);
            }
        }

        [TestMethod]
        public void Should_Give_Priority_To_ID_Convertor_Over_Custom_Type_Convertor()
        {
            var idConvertors = new Dictionary<string, Expression> { { "convertorId=1", (Expression<Func<object, string>>)((object o) => $"Hello World {int.Parse(o.ToString())}") } };
            var typeConvertors = new Dictionary<Type, Expression> { { typeof(string), (Expression<Func<object, string>>)((object o) => o.ToString()) } };

            var sut = new DataReaderMapper<DataTableReader>(typeConvertors, idConvertors);
            sut.Configure<ConvertorIdTestDto>();
            string expected = $"Hello World {ConvertorIdTestDtoBuilder.NestedClassPropertyExpectedValue}";

            using (var reader = ConvertorIdTestDtoBuilder.BuildReader())
            {
                reader.Read();

                var actual = sut.Map<ConvertorIdTestDto>(reader);
                string actualMessage = actual.IntegerToBeConvertedToMessage;

                Assert.AreEqual(expected, actual.IntegerToBeConvertedToMessage);
            }
        }

    }

    #region DTOsAndReaderBuilders
    public class ConvertorIdTestDto
    {
        [Mappable("IntegerColumn", "convertorId=1")]
        public string IntegerToBeConvertedToMessage { get; set; }

        internal static class ConvertorIdTestDtoBuilder
        {
            internal const int NestedClassPropertyExpectedValue = 10;

            internal static DataTableReader BuildReader(int numberOfRows = 1)
            {
                var dataTable = new DataTable();
                dataTable.Columns.Add(new DataColumn("IntegerColumn", typeof(int)));

                for (int i = 0; i < numberOfRows; i++)
                {
                    var dataRow = dataTable.NewRow();
                    dataRow["IntegerColumn"] = NestedClassPropertyExpectedValue;

                    dataTable.Rows.Add(dataRow);
                }

                return dataTable.CreateDataReader();
            }
        }
    }

    public class FuncCacheTestDto
    {
        [MappableSource]
        public NestedFuncDto NestedClassProperty { get; set; }

        internal static class FuncCacheTestDtoBuilder
        {
            internal const int NestedClassPropertyExpectedValue = 1;

            internal static DataTableReader BuildReader(int numberOfRows = 1)
            {
                var dataTable = new DataTable();
                dataTable.Columns.Add(new DataColumn("NestedFuncInteger", typeof(int)));
                dataTable.Columns.Add(new DataColumn("AnotherNestedDtoInteger", typeof(int)));

                for (int i = 0; i < numberOfRows; i++)
                {
                    var dataRow = dataTable.NewRow();
                    dataRow["NestedFuncInteger"] = NestedClassPropertyExpectedValue;
                    dataRow["AnotherNestedDtoInteger"] = NestedClassPropertyExpectedValue;

                    dataTable.Rows.Add(dataRow);
                }

                return dataTable.CreateDataReader();
            }
        }

        public class NestedFuncDto
        {
            [Mappable("NestedFuncInteger")]
            public int Number { get; set; }

            [MappableSource]
            public AnotherNestedDto AnotherNestedClass { get; set; }
        }

        public class AnotherNestedDto
        {
            [Mappable("AnotherNestedDtoInteger")]
            public int AnotherNumber { get; set; }
        }
    }

    public class ExceptionTestDto
    {
        [Mappable("InvalidCastExceptionColumn")]
        public int TryToParseIntFromString { get; set; }

        internal static class ExceptionTestDtoBuilder
        {
            internal const string InvalidCastExceptionColumnValue = "123";

            internal static DataTableReader BuildReader(int numberOfRows = 1)
            {
                var dataTable = new DataTable();
                dataTable.Columns.Add(new DataColumn("InvalidCastExceptionColumn", typeof(string)));

                for (int i = 0; i < numberOfRows; i++)
                {
                    var dataRow = dataTable.NewRow();
                    dataRow["InvalidCastExceptionColumn"] = InvalidCastExceptionColumnValue;

                    dataTable.Rows.Add(dataRow);
                }

                return dataTable.CreateDataReader();
            }
        }
    }

    public class CollectionTestDto
    {
        [Mappable("CollectionColumn", true)]
        public List<string> ListOfStrings { get; set; }

        internal static class CollectionTestDtoBuilder
        {
            internal const string StringsSplitByDelimiter = "some,strings,split,by,a,specified,delimiter";
            internal static List<string> SplitStrings = StringsSplitByDelimiter.Split(',').ToList();


            internal static DataTableReader BuildReader(int numberOfRows = 1)
            {
                var dataTable = new DataTable();
                dataTable.Columns.Add(new DataColumn("CollectionColumn", typeof(string)));

                for (int i = 0; i < numberOfRows; i++)
                {
                    var dataRow = dataTable.NewRow();
                    dataRow["CollectionColumn"] = StringsSplitByDelimiter;

                    dataTable.Rows.Add(dataRow);
                }

                return dataTable.CreateDataReader();
            }
        }
    }

    public class TestDto
    {
        [Mappable("IntegerAsString", true)]
        public int IntegerAsString { get; set; }

        [Mappable("StringColumn")]
        public string StringValue { get; set; }

        [Mappable("IntegerColumn")]
        public int IntegerValue { get; set; }

        [Mappable("BooleanColumn", true)]
        public bool BooleanValue { get; set; }

        [Mappable("DecimalColumn")]
        public decimal DecimalColumn { get; set; }

        [Mappable("DateTimeColumn")]
        public DateTime DateValue { get; set; }

        [Mappable("DateTimeAsStringColumn", true)]
        public DateTime DateTimeAsStringColumn { get; set; }

        [MappableSource]
        public NestedTestDto NestedClass { get; set; }

        public string PropertyWithoutMappableAttribute { get; set; }

        public class NestedTestDto
        {
            [Mappable("NestedStringColumn")]
            public string StringValue { get; set; }

            [MappableSource]
            public NestedNestedTestDto NestedNestedClass { get; set; }

            public class NestedNestedTestDto
            {
                [Mappable("NestedNestedStringColumn")]
                public string StringValue { get; set; }
            }
        }

        internal static class TestDtoBuilder
        {
            internal const string ExpectedStringColumnValue = "ExpectedValue";
            internal const int ExpectedIntegerColumnValue = int.MaxValue;
            internal static DateTime ExpectedDateTimeColumnValue = DateTime.MaxValue;
            internal static DateTime DateTimeAsStringColumnValue = new DateTime(9999, 12, 31);
            internal const int IntegerAsStringValue = int.MaxValue;
            internal const bool ExpectedBooleanColumnValue = true;
            internal const decimal ExpectedDecimalColumn = 11.123M;            

            internal static DataTableReader BuildReader(int numberOfRows = 1)
            {
                var dataTable = new DataTable();
                dataTable.Columns.Add(new DataColumn("StringColumn", typeof(string)));
                dataTable.Columns.Add(new DataColumn("IntegerColumn", typeof(int)));
                dataTable.Columns.Add(new DataColumn("DateTimeColumn", typeof(DateTime)));
                dataTable.Columns.Add(new DataColumn("NestedStringColumn", typeof(string)));
                dataTable.Columns.Add(new DataColumn("NestedNestedStringColumn", typeof(string)));
                dataTable.Columns.Add(new DataColumn("DateTimeAsStringColumn", typeof(string)));
                dataTable.Columns.Add(new DataColumn("IntegerAsString", typeof(string)));
                dataTable.Columns.Add(new DataColumn("BooleanColumn", typeof(string))); 
                dataTable.Columns.Add(new DataColumn("DecimalColumn", typeof(decimal)));

                for (int i = 0; i < numberOfRows; i++)
                {
                    var dataRow = dataTable.NewRow();
                    dataRow["StringColumn"] = ExpectedStringColumnValue;
                    dataRow["IntegerColumn"] = ExpectedIntegerColumnValue;
                    dataRow["DateTimeColumn"] = ExpectedDateTimeColumnValue;
                    dataRow["NestedStringColumn"] = ExpectedStringColumnValue;
                    dataRow["NestedNestedStringColumn"] = ExpectedStringColumnValue;
                    dataRow["DateTimeAsStringColumn"] = DateTimeAsStringColumnValue.ToString(); //31.12.9999
                    dataRow["IntegerAsString"] = IntegerAsStringValue;
                    dataRow["BooleanColumn"] = "1"; //as a db flag
                    dataRow["DecimalColumn"] = ExpectedDecimalColumn;

                    dataTable.Rows.Add(dataRow);
                }

                return dataTable.CreateDataReader();
            }
        }
    }
    #endregion

}
