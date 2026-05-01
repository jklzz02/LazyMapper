using System.Reflection;
using LazyMapper.Lib.Profile.Keys;
using LazyMapper.Lib.Profile.Binding;

namespace LazyMapper.Lib.Profile;

internal interface IMapProfile
{
    ProfileKey Key { get; }
    
    bool IsIgnored(PropertyInfo sourceProperty);
    
    MapBinding? Binding(BindingKey key);
    
    void InvokeBeforeMap(object source);
    
    void InvokeAfterMap(object source, object destination);
}
