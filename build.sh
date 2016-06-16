#xbuild build/Build.proj /p:BuildType=Release

dotnet --info
dotnet restore
dotnet build test/MR.AspNetCore.Jobs.SqlServer.Tests
dotnet test test/MR.AspNetCore.Jobs.SqlServer.Tests -f netcoreapp1.0