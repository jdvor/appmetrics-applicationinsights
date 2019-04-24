param (
    [string]
    [Parameter(Mandatory=$true, HelpMessage='Semantic version for the library; example: 1.0.2')]
    [ValidatePattern('^\d+(\.\d+){2}(-[a-z0-9]+)?$')]
    $SemVer,

    [string]
    [ValidateSet('Debug', 'Release')]
    $Configuration = 'Release',

    [string]
    $RID = 'win10-x64', # https://docs.microsoft.com/en-us/dotnet/core/rid-catalog, linux-x64, win10-x64

    [switch]
    $Sandbox
 )

Push-Location (Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)

# config
$publishDir = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath('./publish')

# cleanup
Get-ChildItem ./ -Include bin,obj,publish,packages -Recurse | ForEach-Object ($_) { Remove-Item $_.FullName -Force -Recurse }

# pack App.Metrics.Reporting.ApplicationInsights
Write-Host '>>> Packaging App.Metrics.Reporting.ApplicationInsights >>>' -ForegroundColor Yellow
$csproj = Resolve-Path './src\App.Metrics.Reporting.ApplicationInsights/App.Metrics.Reporting.ApplicationInsights.csproj'
& dotnet pack $csproj -c $Configuration -o $publishDir /p:SemVer=$SemVer /p:Platform=x64 /nologo

# publish SandboxConsoleApp
if ($Sandbox -eq $true) {
    Write-Host '>>> Publishing SandboxConsoleApp >>>' -ForegroundColor Yellow
    $csproj = Resolve-Path './sample/SandboxConsoleApp/SandboxConsoleApp.csproj'
    $output = Join-Path $publishDir 'sandbox'
    & dotnet publish $csproj -r $RID -c $Configuration -o $output /p:SemVer=$SemVer /p:RuntimeIdentifier=$RID /p:Platform=x64 /nologo
}

Pop-Location