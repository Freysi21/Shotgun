# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What This Is

Shotgun is a two-package .NET library that eliminates CRUD boilerplate for EF Core–backed APIs:

- **Shotgun.Entity** (`netstandard2.0`) — entity base interface and attributes
- **Shotgun** (`net6.0`) — generic ASP.NET Core controller + EF Core repository

Consumer apps inherit from the base classes to get search, paging, ordering, date-range filtering, and CSV export with no extra code.

## Commands

```bash
dotnet build Shotgun.sln
dotnet test -c Release
dotnet pack Shotgun.Entity/Shotgun.Entity.csproj -c Release --output .
dotnet pack Shotgun/Shotgun.csproj -c Release --output .
```

Tests are currently commented out in the CI workflow; run them locally with the above command.

## Architecture

### Generic layering

```
HTTP request
  └─ Shotgun<TEntity, TRepository, IDType>   (Controller/Shotgun.cs)
       └─ EFCoreRepository<TEntity, TContext, IDType>   (Repository/EFCoreRepository.cs)
            └─ DbSet<TEntity> via EF Core
```

Both base classes are abstract and fully generic. A consumer provides one concrete subclass of each and registers them with DI — that is enough to expose all eight endpoints.

### The eight endpoints (Shotgun controller)

`GET /`, `GET /{id}`, `POST /`, `PUT /{id}`, `DELETE /{id}`,  
`GET /search`, `GET /search/ordered`, `GET /csv`

Search endpoints accept query-string parameters whose keys match entity property names. Pagination metadata is written to the `X-Pagination` response header as JSON.

### Expression builders (`Shotgun/Expressions/`)

All dynamic querying is done by building LINQ expression trees — no string interpolation:

| File | Purpose |
|------|---------|
| `Search.cs` | Dictionary → LINQ predicate; dispatches on property type (string → Contains, bool/Guid → exact match, numeric → equality) |
| `OrderBy.cs` | First sort: attribute → DateTime fallback → Id fallback |
| `ThenBy.cs` | Secondary sorts |
| `Range.cs` | From/to date range on nullable `DateTime` properties |
| `Include.cs` | Reflection over `NavigationPropertyAttribute` / `SingleNavigationPropertyAttribute` to eager-load related entities |

### Entity contract (`Shotgun.Entity/`)

Any entity must inherit `IEntity<T>` which enforces a typed `Id` property. Three attributes control library behaviour:

- `[DefaultSortProperty]` — marks the default sort column
- `[NavigationProperty]` — marks collection navigation props for eager loading
- `[SingleNavigationProperty]` — marks single-object navigation props for eager loading

### Paging

`PagingQuery` binds `pageNumber`, `pageSize` (max 50), `orderby`, and `asc` from the query string. `PagedList<T>` wraps results and carries `TotalCount`, `TotalPages`, `HasNext`, `HasPrevious`.

### CSV export

Uses `CsvHelper` with Icelandic locale (`is-IS`). Triggered by `GET /csv` with the same filter parameters as `/search`.

## Release / CI

Releases are published automatically via `.github/workflows/release-shotgun.yml` when a GitHub Release is created. The workflow builds, packs, and pushes both packages to NuGet.org using the `NUGET_API_KEY` secret. The `--skip-duplicate` flag prevents errors on re-runs.

Current package version: **6.2.0** (set in each `.csproj`).
