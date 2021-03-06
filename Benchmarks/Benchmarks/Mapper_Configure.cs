﻿using BenchmarkDotNet.Attributes;
using DataReaderMapper;
using System.Data;
using Benchmarks.Benchmarks.DTOs;

namespace Benchmarks.Benchmarks
{
    [Config(typeof(ConfigureConfiguration))]
    public class MapperConfigure
    {
        private readonly DataReaderMapper<DataTableReader> _mapper = new DataReaderMapper<DataTableReader>();

        [Benchmark(Baseline = true)]
        public void Configure1()
        {
            _mapper.Configure<Dto1Prop>();
        }

        [Benchmark]
        public void Configure5()
        {
            _mapper.Configure<Dto5Prop>();
        }

        [Benchmark]
        public void Configure10()
        {
            _mapper.Configure<Dto10Prop>();
        }
    }
}
