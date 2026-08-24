[CmdletBinding()]
param(
    [switch]$GenerateLockFiles
)

$ErrorActionPreference = 'Stop'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_NOLOGO = '1'
$root = Split-Path -Parent $PSScriptRoot
Push-Location $root
try {
    $expected = '10.0.400'
    $actual = (& dotnet --version).Trim()
    if ($actual -ne $expected) {
        throw "FileConvert requires .NET SDK $expected; active SDK is $actual."
    }

    $projects = Get-ChildItem -Recurse -Filter *.csproj |
        Where-Object { $_.FullName -notmatch '[\\/]artifacts[\\/]' }

    function Get-MissingLockFiles {
        @(
            foreach ($project in $projects) {
                $lock = Join-Path $project.Directory.FullName 'packages.lock.json'
                if (-not (Test-Path $lock)) { $project.FullName }
            }
        )
    }

    if ($GenerateLockFiles) {
        dotnet restore FileConvert.slnx --use-lock-file --force-evaluate
        if ($LASTEXITCODE -ne 0) { throw 'dotnet restore failed while generating lock files.' }

        $missingAfterGeneration = Get-MissingLockFiles
        if ($missingAfterGeneration) {
            throw "Dependency lock files are missing after generation for: $($missingAfterGeneration -join ', ')"
        }

        dotnet restore FileConvert.slnx --locked-mode
        if ($LASTEXITCODE -ne 0) { throw 'generated lock files failed immediate locked-mode verification.' }
        Write-Host 'NuGet lock files generated and rechecked in locked mode. Review every packages.lock.json before committing.'
    }
    else {
        $missing = Get-MissingLockFiles
        if ($missing) {
            throw "Dependency lock files are missing. Run ./build/bootstrap.ps1 -GenerateLockFiles, review the generated locks, then commit them. Missing for: $($missing -join ', ')"
        }

        dotnet restore FileConvert.slnx --locked-mode
        if ($LASTEXITCODE -ne 0) { throw 'locked dotnet restore failed.' }
    }
}
finally {
    Pop-Location
}
