using BenchmarkDotNet.Attributes;
using LazyMapper.Benchmark.Factories;
using LazyMapper.Lib;
using LazyMapper.TestFixtures.Dto;
using LazyMapper.TestFixtures.Models;

namespace LazyMapper.Benchmark.BenchMarks;

[MemoryDiagnoser]
public class FlatMappingBenchmarks
{
    private Mapper _mapper = null!;
    private Customer _customer = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _mapper = BenchmarkMapperFactory.CreateConfiguredMapper();
        _customer = BenchmarkDataFactory.CreateCustomer(1);
    }

    [Benchmark(Baseline = true)]
    public CustomerDto MapSingleFlatObject()
    {
        return _mapper.Map<Customer, CustomerDto>(_customer);
    }
}
