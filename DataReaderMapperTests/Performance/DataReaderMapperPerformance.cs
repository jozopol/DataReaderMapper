using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static DataReaderMapperTests.TestUtils;

namespace DataReaderMapperTests.Performance
{
    [TestClass]
    public class DataReaderMapperPerformance
    {
        [TestMethod]
        public void Performance_PrimitiveDto_One_String_Property()
        {
            var sut = BuildAndConfigureFor<PrimitiveNullableDto<string>>();
            string expectedValue = "ShouldBeThis";

            const int numberOfRows = 10000;
            using (var reader = PrimitiveNullableDto<string>.BuildReader(expectedValue, numberOfRows))
            {
                var list = new List<PrimitiveNullableDto<string>>();
                while (reader.Read())
                {
                    var dto = sut.Map<PrimitiveNullableDto<string>>(reader);
                    list.Add(dto);
                }

                Assert.AreEqual(numberOfRows, list.Count);
            }
        }
    }
}