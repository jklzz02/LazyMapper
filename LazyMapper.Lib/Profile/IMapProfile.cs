using System.Reflection;
using LazyMapper.Binding;

namespace LazyMapper.Profile;

internal interface IMapProfile
{
    ProfileKey Key { get; }
    
    bool IsIgnored(PropertyInfo sourceProperty);
    
    MapBinding? Binding(BindingKey key);
    
    IResolverBinding? Resolver(BindingKey key);
    
    void InvokeBeforeMap(object source);
    
    void InvokeAfterMap(object source, object destination);
}
