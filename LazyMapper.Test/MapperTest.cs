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
        mapper.CreateMap<Person, Student>(profile =>
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
    public void CollectionSmokeTest()
    {
        List<Person> people = 
        [
            new Person { Name = "1", Surname = "2" },
            new Person { Name = "3", Surname = "4" }
        ];
        
        Mapper mapper = new Mapper();
        mapper.CreateMap<Person, Student>(profile =>
        {
            profile.Bind(p => p.Name, s => s.StudentName);
        });
        
        List<Student> students = mapper.Map<Person, Student>(people).ToList();
        
        Assert.Equal(2, students.Count);
        Assert.Equal("1", students[0].StudentName);
        Assert.Equal("3", students[1].StudentName);
    }

    [Fact]
    public void BeforeMapHook_ShouldRunBeforeMapping()
    {
        Person person = new Person();
        Mapper mapper = new Mapper();

        mapper.CreateMap<Person, Student>(profile => profile.Bind(p => p.Name, s => s.StudentName))
            .BeforeMap(p => p.Name = "Before hook Name");
        
        Student student = mapper.Map<Person, Student>(person);
        
        Assert.Equal("Before hook Name", student.StudentName);
    }

    [Fact]
    public void AfterMapHook_ShouldRunAfterMapping()
    {
        Person person = new Person();
        Mapper mapper = new Mapper();
        
        mapper.CreateMap<Person, Student>(profile => profile.Bind(p => p.Name, s => s.StudentName))
            .AfterMap((p, s) => s.BirthDate = p.BirthDate - TimeSpan.FromDays(1));
        
        Student student = mapper.Map<Person, Student>(person);
        
        Assert.Equal(person.BirthDate - TimeSpan.FromDays(1), student.BirthDate);
    }

    [Fact]
    public void IgnoredProperty_ShouldNotBeMapped()
    {
        Person person = new Person
        {
            Surname = "Should not be mapped"
        };
        
        Mapper mapper = new Mapper();
        mapper.CreateMap<Person, Student>(profile =>
        {
            profile.Bind(p => p.Name, s => s.StudentName);
            profile.Ignore(p => p.Surname);
        });
        
        Student student = mapper.Map<Person, Student>(person);
        
        Assert.Equal(person.Age, student.Age);
        Assert.Equal(person.Name, student.StudentName);
        Assert.Equal(person.BirthDate, student.BirthDate);
        Assert.NotEqual(person.Surname, student.Surname);
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
    
        mapper.CreateMap<A, ADto>(profile =>
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