#xbuild build/Build.proj /p:BuildType=Release

dotnet --info
dotnet build src/MR.AspNetCore.Jobs
dotnet build src/MR.AspNetCore.Jobs.SqlServer
dotnet build test/MR.AspNetCore.Jobs.SqlServer.Tests
dotnet test test/MR.AspNetCore.Jobs.SqlServer.Tests