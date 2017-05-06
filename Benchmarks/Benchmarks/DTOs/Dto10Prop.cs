using DataReaderMapper;

namespace Benchmarks.Benchmarks.DTOs
{
    public class Dto10Prop
    {
        [Mappable("1")]
        public string MyProperty { get; set; }

        [Mappable("2")]
        public string MyProperty2 { get; set; }

        [Mappable("3")]
        public string MyProperty3 { get; set; }

        [Mappable("4")]
        public string MyProperty4 { get; set; }

        [Mappable("5")]
        public string MyProperty5 { get; set; }

        [Mappable("6")]
        public string MyProperty6 { get; set; }

        [Mappable("7")]
        public string MyProperty7 { get; set; }

        [Mappable("8")]
        public string MyProperty8 { get; set; }

        [Mappable("9")]
        public string MyProperty9 { get; set; }

        [Mappable("10")]
        public string MyProperty10 { get; set; }
    }

    public class Dto10NullableProp
    {
        [Mappable("1", false, true)]
        public string MyProperty { get; set; }

        [Mappable("2", false, true)]
        public string MyProperty2 { get; set; }

        [Mappable("3", false, true)]
        public string MyProperty3 { get; set; }

        [Mappable("4", false, true)]
        public string MyProperty4 { get; set; }

        [Mappable("5", false, true)]
        public string MyProperty5 { get; set; }

        [Mappable("6", false, true)]
        public string MyProperty6 { get; set; }

        [Mappable("7", false, true)]
        public string MyProperty7 { get; set; }

        [Mappable("8", false, true)]
        public string MyProperty8 { get; set; }

        [Mappable("9", false, true)]
        public string MyProperty9 { get; set; }

        [Mappable("10", false, true)]
        public string MyProperty10 { get; set; }
    }
}
