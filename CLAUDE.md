# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

**Shotgun** is a .NET library that provides a generic base controller and repository pattern for ASP.NET Core. It ships as two NuGet packages:

- **Shotgun.Entity** (netstandard 2.0) — core `IEntity<T>` interface and attributes
- **Shotgun** (net 6.0) — full controller + EF Core repository implementation

## Build & Test

```powershell
# Build
dotnet build Shotgun.sln

# Run all tests
dotnet test TEST.Shotgun/TEST.Shotgun.csproj

# Run a single test
dotnet test TEST.Shotgun/TEST.Shotgun.csproj --filter "FullyQualifiedName=TEST.Shotgun.UnitTest1.Test1"

# Pack NuGet packages
dotnet pack Shotgun.Entity/Shotgun.Entity.csproj -c Release --output .
dotnet pack Shotgun/Shotgun.csproj -c Release --output .
```

## Architecture

### The three-layer pattern consumers use

1. **Entity** — inherit `IEntity<TId>`, decorate with attributes:
   - `[DefaultSortProperty]` — marks the default sort column
   - `[NavigationProperty]` — one-to-many collections
   - `[SingleNavigationProperty]` — one-to-one references
2. **Repository** — subclass `EFCoreRepository<TEntity, TContext, TId>`. Pass a `searchIncludes` string array to the base constructor to always eager-load specific relations during search.
3. **Controller** — subclass `Shotgun<TEntity, TRepository, TId>`. All REST endpoints are inherited.

### Built-in endpoints (from the base controller)

| Method | Route | Notes |
|--------|-------|-------|
| GET | `/api/[controller]` | Paginated list; `?orderby=Prop&asc=true` |
| GET | `/api/[controller]/{id}` | Single entity; `?detail=true` loads nav properties |
| POST | `/api/[controller]` | Create |
| PUT | `/api/[controller]/{id}` | Update |
| DELETE | `/api/[controller]/{id}` | Delete |
| GET | `/api/[controller]/search` | Filter + sort; accepts `dict`, `dateDict`, `orderByDict` query params |
| GET | `/api/[controller]/GetAsCSV` | Full export (semicolon-delimited, is-IS culture) |
| GET | `/api/[controller]/GetSearchAsCSV` | Filtered export |

Pagination metadata is returned in the `X-Pagination` response header as JSON. Max page size is 50 (`PagingQuery`).

### Dynamic query building (Expressions/)

`Search.cs` converts `Dictionary<string, string[]>` into WHERE clauses via reflection + LINQ expression trees. Multiple values per key → OR; multiple keys → AND. Handles `string` (contains), numeric types, `bool`, `Guid`. Type-conversion failures are caught and silently skipped (null expression returned).

`OrderBy.cs` resolves sort column at runtime. Fallback priority: `[DefaultSortProperty]` attribute → first `DateTime`/`DateTime?` property → `Id`.

### Navigation property loading — two distinct strategies

- **Detail endpoint** (`?detail=true`): dynamically builds `Include` calls for all properties decorated with `[NavigationProperty]` or `[SingleNavigationProperty]`.
- **Search/list**: uses the `searchIncludes` string array passed to the repository constructor.

### Publishing

The GitHub Actions workflow (`.github/workflows/release-shotgun.yml`) builds and packs both projects on release, adds the local output as a NuGet source (so `Shotgun` can resolve its dependency on the locally-packed `Shotgun.Entity`), then pushes both to nuget.org. Version bumps require editing `<Version>` in both `.csproj` files.
