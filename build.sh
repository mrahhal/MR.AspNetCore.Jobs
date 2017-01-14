#xbuild build/Build.proj /p:BuildType=Release

dotnet --info
dotnet restore
dotnet test test/MR.AspNetCore.Jobs.Tests -f netcoreapp1.1