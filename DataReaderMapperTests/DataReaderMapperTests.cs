using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mapping;
using System.Data;
using System.Linq;
using static DataReaderMapper.Tests.TestDto;

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
            _sut.Configure<ExceptionTestDto>();
        }

        [TestMethod]
        public void Should_Correctly_Map_String_Column_From_Reader()
        {
            using (var reader = TestDtoBuilder.BuildReader())
            {
                reader.Read();
                var actual = _sut.Map<TestDto>(reader);

                Assert.AreEqual(TestDtoBuilder.ExpectedStringColumnValue, actual.StringValue);
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_Integer_Column_From_Reader()
        {
            using (var reader = TestDtoBuilder.BuildReader())
            {
                reader.Read();
                var actual = _sut.Map<TestDto>(reader);

                Assert.AreEqual(TestDtoBuilder.ExpectedIntegerColumnValue, actual.IntegerValue);
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_DateTime_Column_From_Reader()
        {
            using (var reader = TestDtoBuilder.BuildReader())
            {
                reader.Read();
                var actual = _sut.Map<TestDto>(reader);

                Assert.AreEqual(TestDtoBuilder.ExpectedDateTimeColumnValue, actual.DateValue);
            }
        }

        [TestMethod]
        public void Should_Ignore_Property_Without_MappableAttribute()
        {
            using (var reader = TestDtoBuilder.BuildReader())
            {
                reader.Read();
                var actual = _sut.Map<TestDto>(reader);

                Assert.IsNull(actual.PropertyWithoutMappableAttribute);
            }
        }

        [TestMethod]
        public void Should_Map_String_Column_To_Nested_Class_Property()
        {
            using (var reader = TestDtoBuilder.BuildReader())
            {
                reader.Read();
                var actual = _sut.Map<TestDto>(reader);

                Assert.AreEqual(TestDtoBuilder.ExpectedStringColumnValue, actual.NestedClass.StringValue);
            }
        }

        [TestMethod]
        public void Should_Map_String_Column_To_Double_Nested_Class_Property()
        {
            using (var reader = TestDtoBuilder.BuildReader())
            {
                reader.Read();
                var actual = _sut.Map<TestDto>(reader);

                Assert.AreEqual(TestDtoBuilder.ExpectedStringColumnValue, actual.NestedClass.NestedNestedClass.StringValue);
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_StringAsDateTime_Column_From_Reader()
        {
            using (var reader = TestDtoBuilder.BuildReader())
            {
                reader.Read();                

                var actual = _sut.Map<TestDto>(reader);

                Assert.AreEqual(TestDtoBuilder.DateTimeAsStringColumnValue, actual.DateTimeAsStringColumn);
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_IntegerAsString_Column_From_Reader()
        {
            using (var reader = TestDtoBuilder.BuildReader())
            {
                reader.Read();
                var actual = _sut.Map<TestDto>(reader);

                Assert.AreEqual(TestDtoBuilder.IntegerAsStringValue, actual.IntegerAsString);
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_Multiple_Rows_From_Reader()
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
        public void Should_Correctly_Map_Boolean_Column_From_Reader()
        {
            using (var reader = TestDtoBuilder.BuildReader())
            {
                reader.Read();
                var actual = _sut.Map<TestDto>(reader);

                Assert.AreEqual(TestDtoBuilder.ExpectedDateTimeColumnValue, actual.DateValue);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidCastException))]
        public void Should_Throw_When_Trying_To_Convert_Invalid_Types_Without_Specified_Custom_Converter()
        {
            using (var reader = ExceptionTestDto.ExceptionTestDtoBuilder.BuildReader())
            {
                reader.Read();
                var actual = _sut.Map<ExceptionTestDto>(reader);
            }
        }
    }

    public class ExceptionTestDto
    {
        [Mappable("InvalidCastException")]
        public int TryToParseIntFromString { get; set; }

        internal static class ExceptionTestDtoBuilder
        {
            internal const string InvalidCastExceptionColumnValue = "123";

            internal static DataTableReader BuildReader(int numberOfRows = 1)
            {
                var dataTable = new DataTable();
                dataTable.Columns.Add(new DataColumn("InvalidCastException", typeof(string)));

                for (int i = 0; i < numberOfRows; i++)
                {
                    var dataRow = dataTable.NewRow();
                    dataRow["InvalidCastException"] = InvalidCastExceptionColumnValue;

                    dataTable.Rows.Add(dataRow);
                }

                return dataTable.CreateDataReader();
            }
        }
    }

    public class TestDto
    {
        [Mappable("IntegerAsString", UseCustomConvertor = true)]
        public int IntegerAsString { get; set; }

        [Mappable("StringColumn")]
        public string StringValue { get; set; }

        [Mappable("IntegerColumn")]
        public int IntegerValue { get; set; }

        [Mappable("BooleanColumn")]
        public bool BooleanValue { get; set; }

        [Mappable("DateTimeColumn")]
        public DateTime DateValue { get; set; }

        [Mappable("DateTimeAsStringColumn", UseCustomConvertor = true)]
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
            internal const bool BooleanColumnValue = true;

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
                dataTable.Columns.Add(new DataColumn("BooleanColumn", typeof(bool)));

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
                    dataRow["BooleanColumn"] = BooleanColumnValue;

                    dataTable.Rows.Add(dataRow);
                }

                return dataTable.CreateDataReader();
            }
        }
    }

    
}
