using LazyMapper.Lib;
using LazyMapper.TestFixtures.Profiles;

namespace LazyMapper.Benchmark.Factories;

public static class BenchmarkMapperFactory
{
    private static Mapper? _mapper;
    
    public static Mapper CreateConfiguredMapper()
    {
        if (_mapper is not null)
        {
            return _mapper;
        }
        
        _mapper = new Mapper();
        _mapper.RegisterProfilesFromAssembly(typeof(CustomerProfile).Assembly);
        return _mapper;
    }
}
