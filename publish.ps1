param (
    [string]
    [Parameter(Mandatory=$true, HelpMessage='Semantic version for the library; example: 1.0.2')]
    [ValidatePattern('^\d+(\.\d+){2}(-[a-z0-9]+)?$')]
    $SemVer,

    [string]
    $NugetApiKey
 )

Push-Location (Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)

$nupkg = Resolve-Path "./publish/App.Metrics.Reporting.ApplicationInsights.$SemVer.nupkg" -ErrorAction Stop
$snupkg = Resolve-Path "./publish/App.Metrics.Reporting.ApplicationInsights.$SemVer.snupkg" -ErrorAction Stop

if ($NugetApiKey -eq '') {
    $NugetApiKey = $env:NUGET_APIKEY_APPMETRICS
    if ($NugetApiKey -eq '') {
        Throw 'Missing nuget API key'
    }
}

& dotnet nuget push $nupkg -k $NugetApiKey -s https://api.nuget.org/v3/index.json --skip-duplicate
& dotnet nuget push $snupkg -k $NugetApiKey -s https://api.nuget.org/v3/index.json --skip-duplicate

Pop-Location
