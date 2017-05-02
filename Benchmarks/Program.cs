using BenchmarkDotNet.Running;
using Benchmarks.Benchmarks;

namespace Benchmarks
{
    public class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<Mapper_Mapping>();
            BenchmarkRunner.Run<Mapper_Configure>();
            BenchmarkRunner.Run<Mapper_IEnumerableMapping>();
        }
    }
}

