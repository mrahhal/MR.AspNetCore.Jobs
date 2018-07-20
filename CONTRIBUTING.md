# Development guide

## Running `dotnet ef` in a library

We can manage migrations in a library by using a host project.

```
dotnet ef migrations list --startup-project ..\..\host\MR.AspNetCore.Jobs.EFHost
```

## Releasing

- Commit: Update README.md (if necessary) and CHANGELOG.md
- Tag: `git tag -m x x`
- Commit: Update version for vnext
- Push: `git push --follow-tags`
