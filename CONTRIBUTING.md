# Development guide

## Running `dotnet ef` in a library

We can manage migrations in a library by using a host project.

```
dotnet ef migrations list --startup-project ..\..\host\MR.AspNetCore.Jobs.SqlServer.Host
```
