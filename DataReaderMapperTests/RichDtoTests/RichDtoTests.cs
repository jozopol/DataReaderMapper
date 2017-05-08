using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Services.Description;
using DataReaderMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataReaderMapperTests.RichDtoTests
{
    [TestClass]
    public class RichDtoTests
    {
        private DataReaderMapper<DataTableReader> _sut;

        [TestInitialize]
        public void Setup()
        {
            var typeConvertors = TypeConvertors.DefaultConvertors;
            typeConvertors.Add(typeof(bool), (Expression<Func<object, bool>>)(o => o.ToString() == "1")); // common use case is that db store flags as varchar data type
            typeConvertors.Add(typeof(List<Order>), (Expression<Func<object, List<Order>>>)(o => o.ToString().Split(',').Select(x => new Order{Id = x}).ToList())); // lets say you also need to parse some data from some weird format

            _sut = new DataReaderMapper<DataTableReader>(typeConvertors);
            _sut.Configure<RichDto>();
        }

        [TestMethod]
        public void Should_Correctly_Map_All_Properties()
        {
            using (var reader = RichDtoData.BuildReader())
            {
                reader.Read();
                var actual = _sut.Map<RichDto>(reader);

                Assert.IsNotNull(actual.Data);
                Assert.AreEqual(RichDtoData.StrText ,actual.Data.StrText);
                Assert.AreEqual(RichDtoData.IntCount, actual.Data.Count);
                Assert.IsNotNull(actual.Data.CustomerData);
                
                Assert.AreEqual(RichDtoData.StrName, actual.Data.CustomerData.Name);
                Assert.AreEqual(RichDtoData.IntAge, actual.Data.CustomerData.Age);
                Assert.AreEqual(RichDtoData.ListOrders.Split(',').Length, actual.Data.CustomerData.Orders.Count);

                Assert.IsNotNull(actual.Data.CustomerData.Address);
                Assert.AreEqual(RichDtoData.StrCity, actual.Data.CustomerData.Address.City);
                Assert.AreEqual(RichDtoData.StrZip, actual.Data.CustomerData.Address.ZIP);

                Assert.IsNotNull(actual.Data.AdditionalData);
                Assert.AreEqual(RichDtoData.DatLastLogin, actual.Data.AdditionalData.LastLogin);
                Assert.AreEqual(RichDtoData.BoolConsent, actual.Data.AdditionalData.Consent);

                Assert.IsNotNull(actual.Error);
                Assert.AreEqual(RichDtoData.StrErrorCode, actual.Error.Code);
                Assert.AreEqual(RichDtoData.StrErrorCategory, actual.Error.Category);
            }
        }

        [TestMethod]
        public void Should_Correctly_Cache_Mapping_Function_For_Nested_Type()
        {
            using (var reader = RichDtoData.BuildReader())
            {
                reader.Read();
                var actual = _sut.Map<Error>(reader);

                Assert.IsNotNull(actual);
                Assert.AreEqual(RichDtoData.StrErrorCode, actual.Code);
                Assert.AreEqual(RichDtoData.StrErrorCategory, actual.Category);
            }
        }

        [TestMethod]
        public void Should_Correctly_Map_Nulls_To_Nullable_Fields()
        {
            using (var reader = RichDtoData.BuildReader(numberOfRows:1, generateNulls:true))
            {
                reader.Read();
                var actual = _sut.Map<RichDto>(reader);

                Assert.IsNotNull(actual);
                Assert.AreEqual(null, actual.Data.StrText);

                Assert.IsNotNull(actual.Error);
                Assert.AreEqual(null, actual.Error.Code);
                Assert.AreEqual(null, actual.Error.Category);
            }
        }
        
    }
}

