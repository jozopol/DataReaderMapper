using DataReaderMapper;

namespace Benchmarks.Benchmarks.DTOs
{
    public class Dto1Prop
    {
        [Mappable("1")]
        public string MyProperty { get; set; }
    }
}
