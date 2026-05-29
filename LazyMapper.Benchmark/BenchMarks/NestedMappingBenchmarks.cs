using BenchmarkDotNet.Attributes;
using LazyMapper.Benchmark.Factories;
using LazyMapper.Lib;
using LazyMapper.TestFixtures.Dto;
using LazyMapper.TestFixtures.Models;

namespace LazyMapper.Benchmark.BenchMarks;

[MemoryDiagnoser]
public class NestedMappingBenchmarks
{
    private Mapper _mapper = null!;
    private Order _order = null!;

    [Params(1, 5, 25, 100)]
    public int ItemCount { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _mapper = BenchmarkMapperFactory.CreateConfiguredMapper();
        _order = BenchmarkDataFactory.CreateOrder(1, ItemCount);
    }

    [Benchmark]
    public OrderDto MapSingleNestedObject()
    {
        return _mapper.Map<Order, OrderDto>(_order);
    }
}
