using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using DataReaderMapper;

namespace DataReaderMapperTests
{
    public static class TestUtils
    {
        public static DataReaderMapper<DataTableReader> BuildAndConfigureFor<TDto>(Dictionary<Type, Expression> typeConvertors = null) where TDto : class, new()
        {
            var mapper = new DataReaderMapper<DataTableReader>(typeConvertors);
            mapper.Configure<TDto>();
            return mapper;
        }

        public static DataTableReader BuildReader<TColumn>(TColumn expectedValue, int numberOfRows)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add(new DataColumn("PropertyToTest", typeof(TColumn)));

            for (int i = 0; i < numberOfRows; i++)
            {
                var dataRow = dataTable.NewRow();
                dataRow["PropertyToTest"] = expectedValue;

                dataTable.Rows.Add(dataRow);
            }

            return dataTable.CreateDataReader();
        }
    }

    public class PrimitiveDto<TProperty>
    {
        [Mappable("PropertyToTest")]
        public TProperty PropertyToTest { get; set; }


        public static DataTableReader BuildReader(TProperty expectedValue, int numberOfRows = 1)
        {
            return TestUtils.BuildReader(expectedValue, numberOfRows);
        }
    }

    public class PrimitiveNullableDto<TProperty>
    {
        [Mappable("PropertyToTest", useCustomConvertor:false, canBeNull:true)]
        public TProperty PropertyToTest { get; set; }
        

        public static DataTableReader BuildReader(TProperty expectedValue, int numberOfRows = 1)
        {
            return TestUtils.BuildReader(expectedValue, numberOfRows);
        }
    }

    public class ConversionDto<TProperty, TReaderColumnType>
    {
        [Mappable("PropertyToTest", true)]
        public TProperty PropertyToTest { get; set; }
        

        public static DataTableReader BuildReader(TReaderColumnType expectedValue, int numberOfRows = 1)
        {
            return TestUtils.BuildReader(expectedValue, numberOfRows);
        }
    }

    public class ContainsNestedClassesDto<TProperty, TNestedProperty> where TNestedProperty : class, new() 
    {

        [MappableSource]
        public TNestedProperty NestedPropertyToTest { get; set; }

        public static DataTableReader BuildReader(TProperty expectedValue, int numberOfRows = 1)
        {
            return TestUtils.BuildReader(expectedValue, numberOfRows); 
        }
    }

    public class WithoutMappableAttribute
    {
        public string NotMappedProperty { get; set; }

        public static DataTableReader BuildReader(string expectedValue, int numberOfRows = 1)
        {
            return TestUtils.BuildReader(expectedValue, numberOfRows);
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
        public PrimitiveNullableDto<string> NestedWithStringProperty { get; set; }

        [MappableSource]
        public PrimitiveNullableDto<string> NestedWithAnotherStringProperty { get; set; }

        [MappableSource]
        public ContainsNestedClassesDto<string, PrimitiveNullableDto<string>> DoubleNestedClassWithStringProperty { get; set; }

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