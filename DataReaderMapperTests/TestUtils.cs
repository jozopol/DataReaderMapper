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
        public TNestedProperty NestedProperty { get; set; }

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
}