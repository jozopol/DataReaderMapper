using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mapping;
using System.Data;

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
        public void Should_Correctly_Map_String_Column_From_Reader()
        {
            using (var reader = DataReaderTestBuilder.BuildReader())
            {
                reader.Read();
                var actual = _sut.Map<TestDto>(reader);

                Assert.AreEqual(DataReaderTestBuilder.ExpectedStringColumnValue, actual.StringValue);
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_Integer_Column_From_Reader()
        {
            using (var reader = DataReaderTestBuilder.BuildReader())
            {
                reader.Read();
                var actual = _sut.Map<TestDto>(reader);

                Assert.AreEqual(DataReaderTestBuilder.ExpectedIntegerColumnValue, actual.IntegerValue);
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_DateTime_Column_From_Reader()
        {
            using (var reader = DataReaderTestBuilder.BuildReader())
            {
                reader.Read();
                var actual = _sut.Map<TestDto>(reader);

                Assert.AreEqual(DataReaderTestBuilder.ExpectedDateTimeColumnValue, actual.DateValue);
            }
        }

        [TestMethod]
        public void Should_Ignore_Property_Without_MappableAttribute()
        {
            using (var reader = DataReaderTestBuilder.BuildReader())
            {
                reader.Read();
                var actual = _sut.Map<TestDto>(reader);

                Assert.IsNull(actual.PropertyWithoutMappableAttribute);
            }
        }

        [TestMethod]
        public void Should_Map_String_Column_To_Nested_Class_Property()
        {
            using (var reader = DataReaderTestBuilder.BuildReader())
            {
                reader.Read();
                var actual = _sut.Map<TestDto>(reader);

                Assert.AreEqual(DataReaderTestBuilder.ExpectedStringColumnValue, actual.NestedClass.StringValue);
            }
        }

        [TestMethod]
        public void Should_Map_String_Column_To_Double_Nested_Class_Property()
        {
            using (var reader = DataReaderTestBuilder.BuildReader())
            {
                reader.Read();
                var actual = _sut.Map<TestDto>(reader);

                Assert.AreEqual(DataReaderTestBuilder.ExpectedStringColumnValue, actual.NestedClass.NestedNestedClass.StringValue);

            }
        }
    }

    internal class TestDto
    {      
        [Mappable("StringColumn")]
        internal string StringValue { get; set; }

        [Mappable("IntegerColumn")]
        internal int IntegerValue { get; set; }

        [Mappable("DateTimeColumn")]
        internal DateTime DateValue { get; set; }

        [MappableSource]
        internal NestedTestDto NestedClass { get; set; }

        internal string PropertyWithoutMappableAttribute { get; set; }

        internal class NestedTestDto
        {
            [Mappable("NestedStringColumn")]
            internal string StringValue { get; set; }

            [MappableSource]
            internal NestedNestedTestDto NestedNestedClass { get; set; }

            internal class NestedNestedTestDto
            {
                [Mappable("NestedNestedStringColumn")]
                public string StringValue { get; set; }
            }
        }
    }

    internal static class DataReaderTestBuilder
    {
        internal const string ExpectedStringColumnValue = "ExpectedValue";
        internal const int ExpectedIntegerColumnValue = int.MaxValue;
        internal static DateTime ExpectedDateTimeColumnValue = DateTime.MaxValue;

        internal static DataTableReader BuildReader()
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add(new DataColumn("StringColumn", typeof(string)));
            dataTable.Columns.Add(new DataColumn("IntegerColumn", typeof(int)));
            dataTable.Columns.Add(new DataColumn("DateTimeColumn", typeof(DateTime)));
            dataTable.Columns.Add(new DataColumn("NestedStringColumn", typeof(string)));
            dataTable.Columns.Add(new DataColumn("NestedNestedStringColumn", typeof(string)));

            var dataRow = dataTable.NewRow();
            dataRow["StringColumn"] = ExpectedStringColumnValue;
            dataRow["IntegerColumn"] = ExpectedIntegerColumnValue;
            dataRow["DateTimeColumn"] = ExpectedDateTimeColumnValue;
            dataRow["NestedStringColumn"] = ExpectedStringColumnValue;
            dataRow["NestedNestedStringColumn"] = ExpectedStringColumnValue;
            dataTable.Rows.Add(dataRow);

            return dataTable.CreateDataReader();
        }
    }
}
