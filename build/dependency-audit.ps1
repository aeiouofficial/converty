[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_NOLOGO = '1'
$root = Split-Path -Parent $PSScriptRoot
Push-Location $root
try {
    $expected = '10.0.400'
    $actual = (& dotnet --version).Trim()
    if ($actual -ne $expected) {
        throw "FileConvert dependency audit requires .NET SDK $expected; active SDK is $actual."
    }

    $artifactDir = Join-Path $root 'artifacts/dependency-audit'
    New-Item -ItemType Directory -Force -Path $artifactDir | Out-Null
    $report = Join-Path $artifactDir 'nuget-vulnerabilities.json'

    $auditOutput = & dotnet package list --project FileConvert.slnx --include-transitive --vulnerable --format json --output-version 1 --no-restore
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet package list vulnerability audit failed with exit code $LASTEXITCODE."
    }
    $auditOutput | Out-File -FilePath $report -Encoding utf8NoBOM

    & python scripts/verify_dependency_audit.py $report
    if ($LASTEXITCODE -ne 0) { throw 'dependency audit report verification failed.' }

    Write-Host "Dependency vulnerability report verified: $report"
}
finally {
    Pop-Location
}
