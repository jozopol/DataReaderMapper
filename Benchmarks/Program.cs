using BenchmarkDotNet.Running;

namespace Benchmarks
{
    public class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<Mapper_Mapping>();
            BenchmarkRunner.Run<Mapper_Configure>();
        }
    }
}

