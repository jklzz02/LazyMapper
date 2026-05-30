using BenchmarkDotNet.Attributes;
using LazyMapper.Benchmark.Factories;
using LazyMapper.TestFixtures.Dto;
using LazyMapper.TestFixtures.Models;

namespace LazyMapper.Benchmark.BenchMarks;

[MemoryDiagnoser]
public class FlatProjectionBenchmark
{
    private Mapper _mapper = null!;
    private IQueryable<Customer> _customers = null!;
    
    [Params(1, 10, 100, 1_000, 10_000)]
    public int Count { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _mapper = BenchmarkMapperFactory.CreateConfiguredMapper();
        _customers = BenchmarkDataFactory.CreateCustomers(Count).AsQueryable();
    }

    [Benchmark]
    public List<CustomerDto> ProjectFlatObjectCollection()
    {
        return _mapper
            .ProjectTo<Customer, CustomerDto>(_customers)
            .ToList();
    }
}