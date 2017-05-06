using BenchmarkDotNet.Running;
using Benchmarks.Benchmarks;

namespace Benchmarks
{
    public static class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<MapperMapping>();
            //BenchmarkRunner.Run<Mapper_Configure>();
            //BenchmarkRunner.Run<Mapper_IEnumerableMapping>();
            //BenchmarkRunner.Run<Mapper_ReaderAccess>();            
        }
    }
}

