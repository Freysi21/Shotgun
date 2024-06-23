# `Shotgun`

_[![Shotgun NuGet Version](https://img.shields.io/badge/Nuget:Shotgun-1.0.0-blue)](https://www.nuget.org/packages/Shotgun)_ 

A .NET library that makes it easier to create beautiful, cross platform, webapi applications.  
It is heavily inspired by REST

## Table of Contents

1. [Features](#features)
1. [Installing](#installing)
1. [Documentation](#documentation)
1. [Examples](#examples)


## Features

* Supports CRUD operation on all entities within a given dbContext
* Filtering
* Paging
* Exporting

## Installing

The fastest way of getting started using `Shotgun` is to install the NuGet package.

```csharp
dotnet add package Shotgun
```

## Documentation

So you done did scaffolded a table with entity framework and you wanna make it available via http? Hoot nany lemme help ya out.
After running 
```csharp
dotnet ef scaffold "somedb" -t SomeTable
```

Change the generated code for the table to

 ```csharp
public class SomeTable : IEntity<int>
{
  public override int Id {get; set}
}
```

If your pk is not named Id change it to ID in the class and update the dbcontext model mapping to refrence the sql pk name.
Next create the repo

```csharp
public class SomeTableRepository : ShotgunRepo<SomeDbContext, SomeTable, int>
{
}
```

Next create the controller

```csharp
public class SomeTableController: Shotgun<SomeTableRepository, SomeTable, int>
{
}
```

Also in your program.cs you need to dependency inject ``` SomeDbContext, SomeTableRepository ```.

After all this you should have some of the more common functionality from a REST API for sometable.
If you want to remove some of the features you can just override the controller endpoints or just use the repository in your own controller.


## Examples

TODO

