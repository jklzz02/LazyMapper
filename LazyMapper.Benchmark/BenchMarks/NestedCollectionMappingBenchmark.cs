using BenchmarkDotNet.Attributes;
using LazyMapper.Lib;
using LazyMapper.TestFixtures.Dto;
using LazyMapper.TestFixtures.Models;

namespace LazyMapper.Benchmark;

[MemoryDiagnoser]
public class NestedCollectionMappingBenchmarks
{
    private Mapper _mapper = null!;
    private List<Order> _orders = null!;

    [Params(1, 10, 100, 1_000)]
    public int OrderCount { get; set; }

    [Params(1, 5, 25)]
    public int ItemCount { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _mapper = BenchmarkMapperFactory.CreateConfiguredMapper();
        _orders = BenchmarkDataFactory.CreateOrders(OrderCount, ItemCount);
    }

    [Benchmark]
    public List<OrderDto> MapNestedObjectCollection()
    {
        return _mapper
            .Map<Order, OrderDto>(_orders)
            .ToList();
    }
}
