using BenchmarkDotNet.Attributes;
using LazyMapper.Lib;
using LazyMapper.TestFixtures.Profiles;

namespace LazyMapper.Benchmark;

[MemoryDiagnoser]
public class ProfileRegistrationBenchmarks
{
    [Benchmark(Baseline = true)]
    public Mapper RegisterProfilesIndividually()
    {
        var mapper = new Mapper();

        mapper.Register<CustomerProfile>();
        mapper.Register<AddressProfile>();
        mapper.Register<OrderItemProfile>();
        mapper.Register<ProductProfile>();
        mapper.Register<OrderProfile>();

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
