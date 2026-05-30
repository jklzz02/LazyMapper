using BenchmarkDotNet.Attributes;
using LazyMapper.Benchmark.Factories;
using LazyMapper.TestFixtures.Dto;
using LazyMapper.TestFixtures.Models;

namespace LazyMapper.Benchmark.BenchMarks;

[MemoryDiagnoser]
public class NestedProjectionBenchmark
{
    private Mapper _mapper = null!;
    private IQueryable<Order> _orders = null!;
    
    [Params(1, 10, 100, 1_000, 10_000)]
    public int Count { get; set; }
    
    [Params(1, 5, 25)]
    public int ItemCount { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _mapper = new Mapper();

        _mapper.CreateMap<Order, OrderDto>(profile =>
        {
            profile.Ignore(o => o.Metadata);
        });
        
        _orders = BenchmarkDataFactory.CreateOrders(Count, ItemCount).AsQueryable();
    }
    
    [Benchmark]
    public List<OrderDto> ProjectNestedObjectCollection()
    {
        return _mapper
            .ProjectTo<Order, OrderDto>(_orders)
            .ToList();
    }
}