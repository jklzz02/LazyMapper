namespace LazyMapper.Test.Objects;

public class Course
{
    public string Name { get; set; } = "Math";
    public int Credits { get; set; } = 3;
}

public class CourseDto
{
    public string Name { get; set; } = string.Empty;
    public int Credits { get; set; }
}

public class Teacher
{
    public string Name { get; set; } = "John Doe";
    public List<Course> Courses { get; set; } = new();
}

public class TeacherDto
{
    public string Name { get; set; } = string.Empty;
    public List<CourseDto> Courses { get; set; } = new();
}

public class Department
{
    public string Name { get; set; } = "Computer Science";
    public Course[] CourseArray { get; set; } = Array.Empty<Course>();
    public HashSet<Course> CourseSet { get; set; } = new();
    public IEnumerable<Course> CourseEnumerable { get; set; } = Enumerable.Empty<Course>();
}

public class DepartmentDto
{
    public string Name { get; set; } = string.Empty;
    public CourseDto[] CourseArray { get; set; } = Array.Empty<CourseDto>();
    public HashSet<CourseDto> CourseSet { get; set; } = new();
    public IEnumerable<CourseDto> CourseEnumerable { get; set; } = Enumerable.Empty<CourseDto>();
}

public class University
{
    public string Name { get; set; } = "State University";
    public List<List<Course>> NestedCourses { get; set; } = new();
}

public class UniversityDto
{
    public string Name { get; set; } = string.Empty;
    public List<List<CourseDto>> NestedCourses { get; set; } = new();
}

public class Classroom
{
    public string RoomNumber { get; set; } = "A101";
    public List<Teacher> Teachers { get; set; } = new();
}

public class ClassroomDto
{
    public string RoomNumber { get; set; } = string.Empty;
    public List<TeacherDto> Teachers { get; set; } = new();
}
