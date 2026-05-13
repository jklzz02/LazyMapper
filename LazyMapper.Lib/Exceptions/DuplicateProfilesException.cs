namespace LazyMapper.Lib.Exceptions;

/// <summary>
/// Represents an exception thrown when duplicate profile configurations are detected during mapping registration.
/// </summary>
public class DuplicateProfilesException(string message) : Exception(message)
{
    public DuplicateProfilesException(Type profile)
        : this($"Cannot register multiple profiles of type '{profile.FullName}'")
    {
    }
    
    public DuplicateProfilesException(Type source, Type destination)
        : this($"Cannot register multiple profiles with source '{source.FullName}' and destination '{destination.FullName}'")
    {
    }
}