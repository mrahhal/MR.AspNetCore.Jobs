$dotnetVersion = "1.0.0-preview1-002702"
$dotnetChannel = "beta"
$dotnetLocalInstallFolder = "$env:LOCALAPPDATA\Microsoft\dotnet\"
$newPath = "$dotnetLocalInstallFolder;$env:PATH"
echo %cd%
& "./dotnet-install.ps1" -Channel $dotnetChannel -Version $dotnetVersion -Architecture x64
if (!($env:Path.Split(';') -icontains $dotnetLocalInstallFolder))
{
	Write-Host "Adding $dotnetLocalInstallFolder to PATH"
	$env:Path = "$newPath"
}
