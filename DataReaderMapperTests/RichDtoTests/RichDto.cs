using System;
using System.Collections.Generic;
using DataReaderMapper;

namespace DataReaderMapperTests.RichDtoTests
{
    internal class RichDto
    {
        public bool HasErrors => !string.IsNullOrWhiteSpace(Error?.Code);

        [MappableSource]
        public SomeData Data { get; set; }

        [MappableSource]
        public Error Error { get; set; }
        
    }

    internal class SomeData
    {
        [Mappable("strText", useCustomConvertor: false, canBeNull: true)]
        public string StrText { get; set; }

        [Mappable("intCount", useCustomConvertor: false, canBeNull: false)]
        public int Count { get; set; }

        [MappableSource]
        public CustomerData CustomerData { get; set; }

        [MappableSource]
        public AdditionalCustomerData AdditionalData { get; set; }
    }

    internal class Error
    {
        [Mappable("strErrorCode", useCustomConvertor: false, canBeNull: true)]
        public string Code { get; set; }

        [Mappable("strErrorCategory", useCustomConvertor: false, canBeNull: true)]
        public string Category { get; set; }
    }

    internal class CustomerData
    {
        [Mappable("strName", useCustomConvertor: false, canBeNull: false)]
        public string Name { get; set; }

        [Mappable("intAge", useCustomConvertor: false, canBeNull: false)]
        public int Age { get; set; }

        [MappableSource]
        public Address Address { get; set; }

        [Mappable("listOrders", useCustomConvertor: true, canBeNull: false)]
        public List<Order> Orders { get; set; }

        public CustomerData()
        {
            Orders = new List<Order>();
        }
    }

    internal class Order
    {
        // here, the attribute will be only used when we try to map an Order class directly
        [Mappable("strId", useCustomConvertor: false, canBeNull: false)]
        public string Id { get; set; }
    }

    internal class Address
    {
        [Mappable("strCity", useCustomConvertor: false, canBeNull: false)]
        public string City { get; set; }

        [Mappable("strZIP", useCustomConvertor: false, canBeNull: false)]
        public string ZIP { get; set; }
    }

    internal class AdditionalCustomerData
    {
        [Mappable("datLastLogin", useCustomConvertor: false, canBeNull: false)]
        public DateTime LastLogin { get; set; }

        [Mappable("boolConsent", useCustomConvertor: true, canBeNull: false)]
        public bool Consent { get; set; }
    }


}
