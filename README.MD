# LazyMapper

[![License](https://img.shields.io/github/license/jklzz02/LazyMapper?)](https://github.com/jklzz02/LazyMapper/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
![xUnit](https://img.shields.io/badge/xUnit-tested-5E1F87)
![BenchmarkDotNet](https://img.shields.io/badge/BenchmarkDotNet-benchmarked-orange)

Since AutoMapper's licensing change, I noticed that I didn't have a unified way to handle mapping in my .NET projects.
This library was written to satisfy the needs of my personal projects. Note that this is a hobby project meant for hobby projects — it doesn't pretend to be a real substitute for any established library or a viable option for any serious project.

## Creating a Map

To create and use a map, all that is needed is the [Mapper](LazyMapper.Lib/Mapper.cs) class.

```csharp
var mapper = new Mapper();
mapper.CreateMap();

var foo = new Foo();
var bar = mapper.Map(foo);
```

By convention, any property sharing the same name and type will be mapped automatically.

## Property Binding

To bind two properties that share the same type but have different names, use the `Bind()` method during map configuration.

```csharp
public record Foo(string FooName);
public record Bar(string BarName);

var mapper = new Mapper();
mapper.CreateMap(profile =>
{
    profile.Bind(f => f.FooName, b => b.BarName);
});
```

It's also possible to bind two properties with different types, as long as a map has already been registered for that source/destination pair.

```csharp
public record Foo(string FooName);
public record Bar(string BarName);
public record FooHolder(Foo Foo);
public record BarHolder(Bar Bar);

var mapper = new Mapper();

mapper.CreateMap(profile =>
{
    profile.Bind(f => f.FooName, b => b.BarName);
});

mapper.CreateMap(profile =>
{
    profile.Bind(fh => fh.Foo, bh => bh.Bar);
});
```

Nested colelction and circular referencing classes are both supported and [tested](LazyMapper.Test/NestedMappingTest.cs)

## Ignore properties

To exclude a property from being mapped, use the `Ignore()` method during map configuration.

```csharp
var mapper = new Mapper();
mapper.CreateMap<Foo, Bar>(profile =>
{
    profile.Ignore(f => f.Id);
});
```

Ignored properties will be skipped entirely and left at their default value on the destination object.

## Lifecycle Hooks

You can register callbacks to run before and after the mapping using `BeforeMap()` and `AfterMap()`.

```csharp
var mapper = new Mapper();
mapper.CreateMap<Foo, Bar>(profile =>
{
    profile
        .BeforeMap(f => f.Name = f.Name.Trim())
        .AfterMap((f, b) => b.Id = f.Id + 1);
});
```

- `BeforeMap` receives the **source** object and runs before any property is mapped.
- `AfterMap` receives both the **source** and the fully mapped **destination** object, making it useful for any post processing that depends on both.

## Class Profiles

For larger projects it may be useful to define [Mapping Profiles](LazyMapper.Lib/Profile/MapProfile.cs) in a class.

```csharp
public class FooProfile : MapProfile<Foo, Bar>
{
    public FooProfile()
    {
        Bind(f => f.FooName, b => b.BarName);
        Ignore(f => f.Id);
        BeforMap(f => f.FooName = f.FooName.Trim());
        AfterMap((f, b) =>
        {
            b.Id = f.Id + 1;
        })
    }
}

```

## Register Profiles

To register a profile simply call the `Register<T>()` method.

```csharp
var mapper = new Mapper();
mapper.Register<FooProfile>();
```

Alternatively is possible to load all profiles from a given `Assembly`. For a class to be a considered a Valid **Profile** all it takes is to be inheritors of [MapProfile<TSource, TDestination>](LazyMapper.Lib/Profile/MapProfile.cs).

```csharp
var mapper = new Mapper();
Assembly profilesAssembly = Assembly.GetAssembly(typeof(FooProfile));
mapper.RegisterProfilesFromAssembly(profilesAssembly);

```

This method will scan the given assembly and register all found profiles.
