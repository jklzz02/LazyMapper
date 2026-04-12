using LazyMapper.Lib;
using LazyMapper.Test.Objects;

namespace LazyMapper.Test;

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
        }).ReverseMap();
        
        Student s = mapper.Map<Person, Student>(p);
        
        Assert.Equal(p.Age, s.Age);
        Assert.Equal(p.Name, s.StudentName);
        Assert.Equal(p.BirthDate, s.BirthDate);
        
        Person p2 = mapper.Map<Student, Person>(s);
        Assert.Equal(p2.Age, p.Age);
        Assert.Equal(p2.Name, p.Name);
        Assert.Equal(p2.BirthDate, p.BirthDate);
    }
    
    [Fact]
    public void NestedMapSmokeTest()
    {
        Student student = new Student();
        StudentArchive studentArchive = new StudentArchive();

        var mapper = new Mapper();
        mapper.CreateMap<Student, Worker>(profile =>
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
        Assert.Equal(student.BirthDate.Date, workerArchive.Worker.BirthDate.Date);
    }

    [Fact]
    public void CircularDependency_ShouldNotStackOverflow()
    {
        var a = new A { Name = "NodeA" };
        var b = new B { Title = "NodeB", Parent = a };
        a.Child = b;
        b.Parent = a;

        var mapper = new Mapper();
    
        mapper.CreateMap<A, ADto>((profile) =>
        {
            profile.Bind(x => x.Child, dto => dto.Child);
        });
    
        mapper.CreateMap<B, BDto>((profile) =>
        {
            profile.Bind(x => x.Parent, dto => dto.Parent);
        });

        ADto mapped = mapper.Map<A, ADto>(a);
        Assert.NotNull(mapped);
        Assert.NotNull(mapped.Child);
        Assert.NotNull(mapped.Child.Parent);
        Assert.Equal(mapped.Child.Parent.Name, a.Child.Parent.Name);
    }
}