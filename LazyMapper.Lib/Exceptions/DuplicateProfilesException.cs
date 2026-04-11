namespace LazyMapper.Lib.Exceptions;

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