using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

namespace Benchmarks
{
    public class BenchmarkConfiguration : ManualConfig
    {
        public BenchmarkConfiguration()
        {
            Add(MemoryDiagnoser.Default);
            Add(
            Job.Clr
            .With(Platform.X64)
            .With(Jit.RyuJit)
            .With(Runtime.Clr)
            .WithId("Mapper_Map"));
        }
    }
}
