﻿using BenchmarkDotNet.Running;
using Foundation.Benchmark;

namespace Foundation.Benchmarks
{

    public class Program
    {
        public static void Main(string[] args)
        {
            //BenchmarkRunner.Run<ReadOnlySpanAndMemoryExtensionsBenchmarks>();
            //BenchmarkRunner.Run<CollectionManipulationBenchmarks>();
            //BenchmarkRunner.Run<CollectionIterationBenchmarks>();
            //BenchmarkRunner.Run<CollectionsBenchMarks>();
            //BenchmarkRunner.Run<HashCodeBenchMarks>();
            //BenchmarkRunner.Run<EnumerableBenchMarks>();
            //BenchmarkRunner.Run<OptionBenchmarks>();
            BenchmarkRunner.Run<PermutationsBenchmark>();
        }
    }
}
