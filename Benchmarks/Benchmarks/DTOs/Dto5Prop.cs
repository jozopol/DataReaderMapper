using DataReaderMapper;

namespace Benchmarks.Benchmarks.DTOs
{
    public class Dto5Prop
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
    }
}
