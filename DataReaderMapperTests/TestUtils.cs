using System.Data;
using DataReaderMapper;
using Mapping;

namespace DataReaderMapperTests
{
    public static class TestUtils
    {
        public static DataTableReader BuildReader<TProperty, TColumn>(TColumn expectedValue, int numberOfRows)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add(new DataColumn("PropertyToTest", typeof(TProperty)));

            for (int i = 0; i < numberOfRows; i++)
            {
                var dataRow = dataTable.NewRow();
                dataRow["PropertyToTest"] = expectedValue;

                dataTable.Rows.Add(dataRow);
            }

            return dataTable.CreateDataReader();
        }
    }

    public class PrimitiveDTO<TProperty>
    {
        [Mappable("PropertyToTest")]
        public TProperty PropertyToTest { get; set; }
        

        public static DataTableReader BuildReader(TProperty expectedValue, int numberOfRows = 1)
        {
            return TestUtils.BuildReader<TProperty, TProperty>(expectedValue, numberOfRows);
        }
    }

    public class MapperDTO<TProperty, TReaderColumnType>
    {
        [Mappable("PropertyToTest", true)]
        public TProperty PropertyToTest { get; set; }
        

        public static DataTableReader BuildReader(TReaderColumnType expectedValue, int numberOfRows = 1)
        {
            return TestUtils.BuildReader<TProperty, TReaderColumnType>(expectedValue, numberOfRows);
        }
    }

    public class ContainsNestedClassesDTO<TProperty, TNestedProperty> where TNestedProperty : class, new() 
    {

        [MappableSource]
        public TNestedProperty NestedPropertyToTest { get; set; }

        public static DataTableReader BuildReader(TProperty expectedValue, int numberOfRows = 1)
        {
            return TestUtils.BuildReader<TProperty, TProperty>(expectedValue, numberOfRows); 
        }
    }

    public class WithoutMappableAttribute
    {
        public string NotMappedProperty { get; set; }

        public static DataTableReader BuildReader(string expectedValue, int numberOfRows = 1)
        {
            return TestUtils.BuildReader<string, string>(expectedValue, numberOfRows);
        }
    }

    public class WithMultiplePrimitiveAndComplexProperties
    {
        public const string ExpectedString = "ShouldBeThis";
        public const int ExpectedInteger = 12345;

        [Mappable("StringProperty")]
        public string StringProperty { get; set; }

        [Mappable("IntegerProperty")]
        public int IntegerProperty { get; set; }

        [Mappable("AnotherStringProperty")]
        public string AnotherStringProperty { get; set; }

        [MappableSource]
        public PrimitiveDTO<string> NestedWithStringProperty { get; set; }

        [MappableSource]
        public PrimitiveDTO<string> NestedWithAnotherStringProperty { get; set; }

        [MappableSource]
        public ContainsNestedClassesDTO<string, PrimitiveDTO<string>> DoubleNestedClassWithStringProperty { get; set; }

        public static DataTableReader BuildReader(int numberOfRows = 1)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add(new DataColumn("StringProperty", typeof(string)));
            dataTable.Columns.Add(new DataColumn("IntegerProperty", typeof(int)));
            dataTable.Columns.Add(new DataColumn("AnotherStringProperty", typeof(string)));
            dataTable.Columns.Add(new DataColumn("PropertyToTest", typeof(string))); // the nested classes should use this column

            for (int i = 0; i < numberOfRows; i++)
            {
                var dataRow = dataTable.NewRow();
                dataRow["StringProperty"] = ExpectedString;
                dataRow["IntegerProperty"] = ExpectedInteger;
                dataRow["AnotherStringProperty"] = ExpectedString;
                dataRow["PropertyToTest"] = ExpectedString;

                dataTable.Rows.Add(dataRow);
            }

            return dataTable.CreateDataReader();
        }
    }
}