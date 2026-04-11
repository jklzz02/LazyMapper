namespace LazyMapper.Test.Objects;

internal class StudentArchive
{
    public string ArchiveName { get; set; } = "Lazy student Archive";
    
    public Student Student { get; set; } = new Student();
}