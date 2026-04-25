using LazyMapper.Lib;
using LazyMapper.Test.Objects;

namespace LazyMapper.Test;

public class CollectionMappingTest
{
    [Fact]
    public void MapList_ShouldMapAllElements()
    {
        var teacher = new Teacher
        {
            Name = "Jane Smith",
            Courses =
            [
                new Course { Name = "1", Credits = 4 },
                new Course { Name = "2", Credits = 3 },
                new Course { Name = "3", Credits = 5 }
            ]
        };

        var mapper = new Mapper();
        mapper.CreateMap<Course, CourseDto>();
        mapper.CreateMap<Teacher, TeacherDto>(profile =>
        {
            profile.Bind(t => t.Courses, dto => dto.Courses);
        });

        var result = mapper.Map<Teacher, TeacherDto>(teacher);

        Assert.NotNull(result);
        Assert.Equal(teacher.Name, result.Name);
        Assert.NotNull(result.Courses);
        Assert.Equal(3, result.Courses.Count);
        Assert.Equal("1", result.Courses[0].Name);
        Assert.Equal(4, result.Courses[0].Credits);
        Assert.Equal("2", result.Courses[1].Name);
        Assert.Equal(3, result.Courses[1].Credits);
        Assert.Equal("3", result.Courses[2].Name);
        Assert.Equal(5, result.Courses[2].Credits);
    }

    [Fact]
    public void MapArray_ShouldMapAllElements()
    {
        var department = new Department
        {
            Name = "1",
            CourseArray =
            [
                new Course { Name = "x", Credits = 3 },
                new Course { Name = "y", Credits = 4 }
            ]
        };

        var mapper = new Mapper();
        mapper.CreateMap<Course, CourseDto>();
        mapper.CreateMap<Department, DepartmentDto>(profile =>
        {
            profile.Bind(d => d.CourseArray, dto => dto.CourseArray);
        });

        var result = mapper.Map<Department, DepartmentDto>(department);

        Assert.NotNull(result);
        Assert.Equal(department.Name, result.Name);
        Assert.NotNull(result.CourseArray);
        Assert.Equal(2, result.CourseArray.Length);
        Assert.Equal("x", result.CourseArray[0].Name);
        Assert.Equal(3, result.CourseArray[0].Credits);
        Assert.Equal("y", result.CourseArray[1].Name);
        Assert.Equal(4, result.CourseArray[1].Credits);
    }

    [Fact]
    public void MapHashSet_ShouldMapAllElements()
    {
        var department = new Department
        {
            Name = "1",
            CourseSet =
            [
                new Course { Name = "x", Credits = 4 },
                new Course { Name = "y", Credits = 3 }
            ]
        };

        var mapper = new Mapper();
        mapper.CreateMap<Course, CourseDto>();
        mapper.CreateMap<Department, DepartmentDto>(profile =>
        {
            profile.Bind(d => d.CourseSet, dto => dto.CourseSet);
        });

        var result = mapper.Map<Department, DepartmentDto>(department);

        Assert.NotNull(result);
        Assert.Equal(department.Name, result.Name);
        Assert.NotNull(result.CourseSet);
        Assert.Equal(2, result.CourseSet.Count);
        Assert.Contains(result.CourseSet, c => c.Name == "x" && c.Credits == 4);
        Assert.Contains(result.CourseSet, c => c.Name == "y" && c.Credits == 3);
    }

    [Fact]
    public void MapIEnumerable_ShouldMapAllElements()
    {
        var department = new Department
        {
            Name = "1",
            CourseEnumerable = new List<Course>
            {
                new Course { Name = "x", Credits = 5 },
                new Course { Name = "y", Credits = 4 }
            }
        };

        var mapper = new Mapper();
        mapper.CreateMap<Course, CourseDto>();
        mapper.CreateMap<Department, DepartmentDto>(profile =>
        {
            profile.Bind(d => d.CourseEnumerable, dto => dto.CourseEnumerable);
        });

        var result = mapper.Map<Department, DepartmentDto>(department);

        Assert.NotNull(result);
        Assert.Equal(department.Name, result.Name);
        Assert.NotNull(result.CourseEnumerable);
        var courseList = result.CourseEnumerable.ToList();
        Assert.Equal(2, courseList.Count);
        Assert.Equal("x", courseList[0].Name);
        Assert.Equal(5, courseList[0].Credits);
        Assert.Equal("y", courseList[1].Name);
        Assert.Equal(4, courseList[1].Credits);
    }

    [Fact]
    public void MapEmptyCollection_ShouldReturnEmptyCollection()
    {
        var teacher = new Teacher
        {
            Name = "1",
            Courses = new List<Course>()
        };

        var mapper = new Mapper();
        mapper.CreateMap<Course, CourseDto>();
        mapper.CreateMap<Teacher, TeacherDto>(profile =>
        {
            profile.Bind(t => t.Courses, dto => dto.Courses);
        });

        var result = mapper.Map<Teacher, TeacherDto>(teacher);

        Assert.NotNull(result);
        Assert.NotNull(result.Courses);
        Assert.Empty(result.Courses);
    }

    [Fact]
    public void MapNestedCollection_ShouldMapAllLevels()
    {
        var university = new University
        {
            Name = "1",
            NestedCourses =
            [
                [
                    new Course { Name = "CS101", Credits = 3 },
                    new Course { Name = "CS102", Credits = 4 }
                ],
                [
                    new Course { Name = "CS201", Credits = 4 },
                    new Course { Name = "CS202", Credits = 5 }
               ] 
            ]
        };

        var mapper = new Mapper();
        mapper.CreateMap<Course, CourseDto>();
        mapper.CreateMap<Teacher, TeacherDto>(profile =>
        {
            profile.Bind(t => t.Courses, dto => dto.Courses);
        });
        mapper.CreateMap<University, UniversityDto>(profile =>
        {
            profile.Bind(u => u.NestedCourses, dto => dto.NestedCourses);
        });

        var result = mapper.Map<University, UniversityDto>(university);

        Assert.NotNull(result);
        Assert.Equal(university.Name, result.Name);
        Assert.NotNull(result.NestedCourses);
        Assert.Equal(2, result.NestedCourses.Count);
        
        Assert.Equal(2, result.NestedCourses[0].Count);
        Assert.Equal("CS101", result.NestedCourses[0][0].Name);
        Assert.Equal(3, result.NestedCourses[0][0].Credits);
        Assert.Equal("CS102", result.NestedCourses[0][1].Name);
        Assert.Equal(4, result.NestedCourses[0][1].Credits);
        
        Assert.Equal(2, result.NestedCourses[1].Count);
        Assert.Equal("CS201", result.NestedCourses[1][0].Name);
        Assert.Equal(4, result.NestedCourses[1][0].Credits);
        Assert.Equal("CS202", result.NestedCourses[1][1].Name);
        Assert.Equal(5, result.NestedCourses[1][1].Credits);
    }

    [Fact]
    public void MapCollectionOfComplexObjects_ShouldMapAllNestedProperties()
    {
        var classroom = new Classroom
        {
            RoomNumber = "1",
            Teachers =
            [
                new Teacher
                {
                    Name = "Prof. Anderson",
                    Courses =
                    [
                        new Course { Name = "Database Systems", Credits = 4 },
                        new Course { Name = "Data Structures", Credits = 3 }
                    ]
                },

                new Teacher
                {
                    Name = "Dr. Brown",
                    Courses = [new Course { Name = "Operating Systems", Credits = 5 }]
                }
            ]
        };

        var mapper = new Mapper();
        mapper.CreateMap<Course, CourseDto>();
        mapper.CreateMap<Teacher, TeacherDto>(profile =>
        {
            profile.Bind(t => t.Courses, dto => dto.Courses);
        });
        mapper.CreateMap<Classroom, ClassroomDto>(profile =>
        {
            profile.Bind(c => c.Teachers, dto => dto.Teachers);
        });

        var result = mapper.Map<Classroom, ClassroomDto>(classroom);

        Assert.NotNull(result);
        Assert.Equal("1", result.RoomNumber);
        Assert.NotNull(result.Teachers);
        Assert.Equal(2, result.Teachers.Count);
        
        Assert.Equal("Prof. Anderson", result.Teachers[0].Name);
        Assert.Equal(2, result.Teachers[0].Courses.Count);
        Assert.Equal("Database Systems", result.Teachers[0].Courses[0].Name);
        Assert.Equal(4, result.Teachers[0].Courses[0].Credits);
        
        Assert.Equal("Dr. Brown", result.Teachers[1].Name);
        Assert.Equal(1, result.Teachers[1].Courses.Count);
        Assert.Equal("Operating Systems", result.Teachers[1].Courses[0].Name);
        Assert.Equal(5, result.Teachers[1].Courses[0].Credits);
    }

    [Fact]
    public void MapMultipleCollectionTypes_InSameObject_ShouldMapAllCorrectly()
    {
        var department = new Department
        {
            Name = "Engineering",
            CourseArray =
            [
                new Course { Name = "Course1", Credits = 3 }
            ],
            CourseSet = [new Course { Name = "Course2", Credits = 4 }],
            CourseEnumerable = new List<Course>
            {
                new Course { Name = "Course3", Credits = 5 }
            }
        };

        var mapper = new Mapper();
        mapper.CreateMap<Course, CourseDto>();
        mapper.CreateMap<Department, DepartmentDto>(profile =>
        {
            profile.Bind(d => d.CourseArray, dto => dto.CourseArray);
            profile.Bind(d => d.CourseSet, dto => dto.CourseSet);
            profile.Bind(d => d.CourseEnumerable, dto => dto.CourseEnumerable);
        });

        var result = mapper.Map<Department, DepartmentDto>(department);

        Assert.NotNull(result);
        Assert.Single(result.CourseArray);
        Assert.Equal("Course1", result.CourseArray[0].Name);
        
        Assert.Single(result.CourseSet);
        Assert.Contains(result.CourseSet, c => c.Name == "Course2");
        
        Assert.Single(result.CourseEnumerable);
        Assert.Equal("Course3", result.CourseEnumerable.First().Name);
    }
}
