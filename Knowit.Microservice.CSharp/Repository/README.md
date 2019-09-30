﻿# Repository

## Database migrations

### Generate migrations 
```console
 dotnet ef migrations add -s Host -p Repository <name of migration>
```
(from solution root dir)

### Apply migrations

```console
dotnet ef database update -s Host -p Repository
```

## Useful links
* [EF Core](https://docs.microsoft.com/en-us/ef/core/)
* [EF Core - Migrations](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/)