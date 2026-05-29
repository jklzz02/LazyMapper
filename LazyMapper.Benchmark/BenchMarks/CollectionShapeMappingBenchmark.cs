using BenchmarkDotNet.Attributes;
using LazyMapper.Benchmark.Factories;
using LazyMapper.Lib;
using LazyMapper.TestFixtures.Dto;
using LazyMapper.TestFixtures.Models;

namespace LazyMapper.Benchmark.BenchMarks;

[MemoryDiagnoser]
public class CollectionShapeBenchmarks
{
    private Mapper _mapper = null!;
    private List<Product> _products = null!;

    [Params(1, 10, 100, 1_000, 10_000)]
    public int Count { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _mapper = BenchmarkMapperFactory.CreateConfiguredMapper();
        _products = BenchmarkDataFactory.CreateProducts(Count);
    }

    [Benchmark]
    public List<ProductDto> MapObjectsWithArrayToListProperty()
    {
        return _mapper
            .Map<Product, ProductDto>(_products)
            .ToList();
    }
}
