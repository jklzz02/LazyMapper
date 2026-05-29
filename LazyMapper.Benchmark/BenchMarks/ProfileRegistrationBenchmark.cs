using BenchmarkDotNet.Attributes;
using LazyMapper.TestFixtures.Profiles;

namespace LazyMapper.Benchmark.BenchMarks;

[MemoryDiagnoser]
public class ProfileRegistrationBenchmarks
{
    [Benchmark(Baseline = true)]
    public Mapper RegisterProfilesIndividually()
    {
        var mapper = new Mapper();

        mapper.Register<CustomerProfile>();
        mapper.Register<AddressProfile>();
        mapper.Register<OrderProfile>();
        mapper.Register<OrderItemProfile>();
        mapper.Register<ProductProfile>();
        return mapper;
    }

    [Benchmark]
    public Mapper RegisterProfilesFromAssembly()
    {
        var mapper = new Mapper();

        mapper.RegisterProfilesFromAssembly(typeof(CustomerProfile).Assembly);

        return mapper;
    }
}
