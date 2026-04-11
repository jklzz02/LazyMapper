using LazyMapper.Lib;
using LazyMapper.Test.Objects;

namespace LazyMapping.Test;

public class MapperTest
{
    [Fact]
    public void SmokeTest()
    {
        Person p = new Person();

        var mapper = new Mapper();
        mapper.CreateMap<Person, Student>((profile) =>
        {
            profile.Bind(p => p.Name, s => s.StudentName);
        });
        
        Student s = mapper.Map<Person, Student>(p);
        
        Assert.Equal(p.Age, s.Age);
        Assert.Equal(p.Name, s.StudentName);
        Assert.Equal(p.BirthDate, s.BirthDate);
    }
    
    [Fact]
    public void NestedMapSmokeTest()
    {
        Student student = new Student();
        StudentArchive studentArchive = new StudentArchive();

        var mapper = new Mapper();
        mapper.CreateMap<Student, Worker>((profile) =>
        {
            profile
                .Bind(s => s.School, w => w.Company)
                .Bind(s => s.Class, w => w.Position);
        });
        
        mapper.CreateMap<StudentArchive, WorkerArchive>((profile) =>
        {
            profile.Bind(sa => sa.Student, wa => wa.Worker);
        });
        
        WorkerArchive workerArchive = mapper.Map<StudentArchive, WorkerArchive>(studentArchive);
        Assert.NotNull(workerArchive);
        Assert.NotNull(workerArchive.Worker);
        Assert.Equal(studentArchive.ArchiveName, workerArchive.ArchiveName);
        Assert.Equal(student.Class, workerArchive.Worker.Position);
        Assert.Equal(student.School, workerArchive.Worker.Company);
        Assert.Equal(student.Name, workerArchive.Worker.Name);
        Assert.Equal(student.Age, workerArchive.Worker.Age);
        Assert.Equal(student.BirthDate, workerArchive.Worker.BirthDate);
    }
}