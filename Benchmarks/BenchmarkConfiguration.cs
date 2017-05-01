using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

namespace Benchmarks
{
    public class MappingConfiguration : ManualConfig
    {
        public MappingConfiguration()
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

    public class ConfigureConfiguration : ManualConfig
    {
        public ConfigureConfiguration()
        {
            Add(MemoryDiagnoser.Default);
            Add(
            Job.Clr
            .With(Platform.X64)
            .With(Jit.RyuJit)
            .With(Runtime.Clr)
            .WithId("Mapper_Configure"));
        }
    }
}
